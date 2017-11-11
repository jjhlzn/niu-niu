﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	
	public static int SeatCount = 6;
	
	public Game ()
	{
		state = GameState.BeforeStart;
		rounds = new List<Round> ();
		currentRound = new Round();
		rounds.Add(currentRound);
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

	public GameState state;
	public string roomNo;
	public List<Round> rounds;
	public Player me = Player.Me;
	public Round currentRound;
	public int totalRoundCount;
	public int currentRoundNo;

	public Seat[] seats;
	public Deck deck;

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

	public void GoToNextRound() {
		currentRoundNo++;
		bankerSignImage.transform.position = originBankerSignPosition;
		bankerSignImage.gameObject.SetActive (false);
		//设置CurrentRound的数据
		currentRound = new Round();
		rounds.Add (currentRound);
		for (int i = 0; i < Game.SeatCount; i++) {
			if (seats [i].hasPlayer ()) {
				seats [i].player.isPlaying = true;
			}
		}
	}

	public void ShowStateLabel(string msg) {
		this.gameStateLabel.text = msg;
		this.gameStateLabel.gameObject.SetActive (true);
	}

	public void HideStateLabel() {
		this.gameStateLabel.gameObject.SetActive (false);
	}

	/*
	public void UpdateGateStateLabel() {
		if (state == GameState.RobBanker) {
		} else if (state == GameState.ChooseBanker) {
		} else if (state == Game.
	} */
}


