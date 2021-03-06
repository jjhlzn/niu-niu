﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void Callback();

public class Game
{
	public Vector3 originBankerSignPosition;
	private Image _bankerSignImage;
	public Image bankerSignImage {
		get {
			return _bankerSignImage;
		}
		set {
			_bankerSignImage = value;
			originBankerSignPosition = value.transform.position;
		}
	}

	public Text gameStateLabel;
	public Image gameStateLabelBackground;
	
	public static int SeatCount = 6;
	
	public Game ()
	{
		state = GameState.BeforeStart;
		//rounds = new List<Round> ();
		//currentRound = new Round();
		//rounds.Add(currentRound);
		deck = new Deck ();
	}  
		
	public bool isMeSeated {   //我是否已经坐下
		get {
			foreach (Seat seat in seats) {
				if (seat.hasPlayer () && seat.player.userId == me.userId) {
					return true;
				}
			}
			return false;
		}
	}
		
	public bool isInited;
	public bool isPause;

	public GameState state;
	public string roomNo;
	public Round[] rounds;
	public Player me = Player.Me;
	public Round currentRound;
	public int totalRoundCount;
	public int currentRoundNo;
	public string creater;

	public Seat[] seats;
	public Deck deck;
	public Sprite[] niuSprites;
	public Sprite[] multipleSprites;

	public Button[] betButtons;
	public Text[] betLabels;
	public Vector3[] betButtonPositionsFor3Button;
	public Vector3[] betLabelPositionsFor3Button;
	public Vector3[] betButtonPositionsFor4Button;
	public Vector3[] betLabelPositionsFor4Button;
	public Text roomLabel;
	public Text roundLabel;

	public void Reset() {
		//rounds = new List<Round> ();
		currentRound = null;
	}

	public int LastPlayerSeatIndex {
		get {
			List<Player> players = PlayingPlayers;
			if (players.Count == 0) {
				return -1;
			}
			return players [players.Count - 1].seat.seatIndex; 
		}
	}

	public int PlayerCount {
		get {
			int count = 0;
			foreach (Seat seat in seats) {
				if (seat.player != null) {
					count++;
				}
			}
			return count;
		}
	}

	public int GetSeatIndex(string userId) {
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer () && userId == seats [i].player.userId) {
				return i;
			}
		}
		return -1;
	}



	public int GetSeatIndexThroughSeatNo(string seatNo) {
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].seatNo == seatNo) {
				return i;
			}
		}
		return -1;
	}

	public List<Player> PlayingPlayers {
		get {
			List<Player>  players = new List<Player>();
			for (int i = 0; i < seats.Length; i++) {

				if (seats [i].hasPlayer () && seats[i].player.isPlaying) {
					players.Add (seats [i].player);
				}
			}
			return players;
		}
	}

	public List<Player> SittedPlayers {
		get {
			List<Player>  players = new List<Player>();
			for (int i = 0; i < seats.Length; i++) {
				if (seats [i].hasPlayer () ) {
					players.Add (seats [i].player);
				}
			}
			return players;
		}
	}

	public void StartGame() {
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer ()) {
				seats [i].player.isPlaying = true;
			}
		}
	}

	public bool HasNextRound() {
		if (currentRoundNo < totalRoundCount) {
			return true;
		}
		return false;
	}


	public Player GetPlayer(string userId) {
		foreach (Player player in PlayingPlayers) {
			if (player.userId == userId)
				return player;
		}
		return null;
	}


	public void GoToNextRound() {
		//currentRoundNo++;
		bankerSignImage.transform.position = originBankerSignPosition;
		bankerSignImage.gameObject.SetActive (false);
		//设置CurrentRound的数据
		currentRound = new Round();
		rounds [currentRoundNo - 1] = currentRound;
		for (int i = 0; i < Game.SeatCount; i++) {
			if (seats [i].hasPlayer ()) {
				seats [i].player.isPlaying = true;
			}
		}
	}

	public void ShowStateLabel(string msg) {
		if (this.gameStateLabel.gameObject.activeInHierarchy && this.gameStateLabel.text.Equals(msg))
			return;
		this.gameStateLabel.text = msg;
		this.gameStateLabel.gameObject.SetActive (true);
		this.gameStateLabelBackground.gameObject.SetActive (true);
		//Debug.Log (state.value+ ": ShowStateLabel, msgs = " + msg);
	}

	public void HideStateLabel() {
		this.gameStateLabel.gameObject.SetActive (false);
		this.gameStateLabelBackground.gameObject.SetActive (false);
		//Debug.Log (state.value+ ": hideStateLabel");
	}

	public bool IsStateLabelVisible() {
		return this.gameStateLabel.gameObject.activeInHierarchy;
	}

	public Sprite getNiuSprite(int niu) {
		string spriteName = "niu" + niu;
		for(int i = 0; i < niuSprites.Length; i++) {
			if (niuSprites [i].name == spriteName) {
				return niuSprites [i];
			}
		}
		throw new UnityException ("找不到Sprite " + spriteName);
	}

	public Sprite getMultipleSprite(int multiple) {
		string multipleName = "x" + multiple;
		for(int i = 0; i < multipleSprites.Length; i++) {
			if (multipleSprites [i].name == multipleName) {
				return multipleSprites [i];
			}
		}
		throw new UnityException ("找不到Sprite " + multipleName);
	}


	public void ShowBetButtons() {
		int[] myBets = currentRound.myBets;
		if (myBets == null)
			return;
		bool is4Button = myBets.Length == 4;
		for (var i = 0; i < myBets.Length; i++) {
			
			if (is4Button) {
				betLabels [i].transform.position = betLabelPositionsFor4Button [i];
				betButtons [i].transform.position = betButtonPositionsFor4Button [i];
			} else {
				betLabels [i].transform.position = betLabelPositionsFor3Button [i];
				betButtons [i].transform.position = betButtonPositionsFor3Button [i];
			}
			betLabels [i].text = "x" + myBets [i];
			betLabels [i].gameObject.SetActive (true);
			betButtons [i].gameObject.SetActive (true);

		}
		
	}

	public void HideBetButtons() {
		if (!betButtons [0].gameObject.activeInHierarchy)
			return;
		for(int i = 0; i < betButtons.Length; i++) {
			betButtons[i].gameObject.SetActive (false);
			betLabels[i].gameObject.SetActive (false);
		}
	}

	public void HideIsRobImages() {
		foreach(Player player in PlayingPlayers) {
			if (player.seat.isRobImage.gameObject.activeInHierarchy) {
				player.seat.isRobImage.gameObject.SetActive (false);
			}
		}
	}

	public void UpdateGameInfos() {
		this.roomLabel.text =  "房号 : " + this.roomNo;
		if (state == GameState.BeforeStart)
			this.roundLabel.text = "局数 : 0/" + this.totalRoundCount;
		else 
			this.roundLabel.text = "局数 : " + this.currentRoundNo + "/" + this.totalRoundCount;

		//Debug.Log ("state = " + state.value);
		//Debug.Log ("this.currentRoundNo = " + this.currentRoundNo);
		//Debug.Log ("this.totalRoundCount = " + this.totalRoundCount);
	}

	public void UpdateSeatUIs() {
		for (int i = 0; i < Game.SeatCount; i++) {
			if (seats [i].hasPlayer ()) {
				seats [i].UpdateUI (this);
			}
		}
	}



	public void FirstDeal() {
		
		for (int i = 0; i < PlayingPlayers.Count; i++) {
			FirstDeal (PlayingPlayers[i]);
		}
	}



	private void FirstDeal(Player player) {
		player.seat.cards [0] = deck.Deal ();
		player.seat.cards [0].sprite = deck.cardBack;

		player.seat.cards [1] = deck.Deal ();
		player.seat.cards [1].sprite = deck.cardBack;

		player.seat.cards [2] = deck.Deal ();
		player.seat.cards [2].sprite = deck.cardBack;

		player.seat.cards [3] = deck.Deal ();
		player.seat.cards [3].sprite = deck.cardBack;
	}

	public IEnumerator TurnCardUp(Image card, string cardValue, Callback handler = null) {
		if (!string.IsNullOrEmpty (cardValue)) {
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("Turn90_2");
			yield return new WaitForSeconds (0.15f);
			card.gameObject.SetActive (false);
			card.sprite = deck.GetCardFaceImage (cardValue);
			card.gameObject.SetActive (true);
			anim.Play ("Turn85_3");
			yield return new WaitForSeconds (0.1f);

		} else {
			yield return new WaitForSeconds (0.25f);
		}
		if (handler != null) {
			yield return new WaitForSeconds (0.2f);
			handler ();
		}
	}

}


