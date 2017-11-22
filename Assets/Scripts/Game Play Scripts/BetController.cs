﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;

public class BetController : BaseStateController {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private SecondDealController secondDealController;

	private float user0ChipMoveSpeed = 180f;
	private float chipMoveSpeed = 180f;

	private Seat[] seats;

	private bool[] isMoveChipArray;
	private bool[] isBetCompletedArray;
	private bool hasBet = false;
	private float stateTimeLeft; //这状态停留的时间
	private float animationTime = 1f;

	public bool IsAllBetCompleted {
		get {
			bool result = true;
			var seats = gamePlayController.game.seats;
			for (var i = 0; i < seats.Length; i++) {
				var player = seats [i].player;
				if (seats [i].hasPlayer () && player.isPlaying && player.userId != gamePlayController.game.currentRound.banker) {
					result = result && isBetCompletedArray [i];
				}
			}
			return result;
		}
	}

	public void Init() {
		seats = gamePlayController.game.seats;
		stateTimeLeft = Constants.MaxStateTimeLeft - animationTime;
		gamePlayController.game.HideBetButtons ();
	}
		
	public void Awake() {
		isMoveChipArray = new bool[Game.SeatCount];
		isBetCompletedArray = new bool[Game.SeatCount];
	}

	public override void Reset() {
		hasBet = false;
		isMoveChipArray = new bool[Game.SeatCount];
		isBetCompletedArray = new bool[Game.SeatCount];
		stateTimeLeft = Constants.MaxStateTimeLeft - animationTime;
	}
	
	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	// Update is called once per frame
	public new void Update ()  {
		base.Update ();
		if (gamePlayController.state == GameState.Bet) {

			if (stateTimeLeft > 0) {
				gamePlayController.game.ShowStateLabel ("请选择下注分数: " + Mathf.Round (stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}
		} 

		if ( Player.Me.isPlaying
			&& gamePlayController.state == GameState.Bet 
			&& !hasBet 
			&& gamePlayController.game.currentRound.banker != gamePlayController.game.PlayingPlayers [0].userId) {

			gamePlayController.game.ShowBetButtons ();
		} else {
			gamePlayController.game.HideBetButtons ();
		}


		BetAnimation ();
	}




	public void SetUI() {
		var game = gamePlayController.game;
		var round = game.currentRound;
		for (int i = 0; i < round.playerBets.Length; i++) {
			if (round.playerBets [i] != -1) {

				seats[i].chipImageForBet.gameObject.transform.position = seats [i].chipPositionWhenBet;
				seats[i].chipImageForBet.gameObject.SetActive (true);
				seats[i].chipCountLabel.text = gamePlayController.game.currentRound.playerBets[i] + "";
				seats[i].chipCountLabel.gameObject.SetActive (true);
				isBetCompletedArray [i] = true;
			} 


		}
	}
		

	private void BetAnimation() {
		for (int i = 0; i < Game.SeatCount; i++) {
			if (isMoveChipArray [i]) {
				seats[i].chipImageForBet.gameObject.SetActive (true);

				Vector3 targetPosition = seats[i].chipPositionWhenBet;
				float step = 0;
				if (i == 0) {
					step = user0ChipMoveSpeed * Time.deltaTime;
				} else {
					step = chipMoveSpeed * Time.deltaTime;
				}

				seats[i].chipImageForBet.gameObject.transform.position = Vector3.MoveTowards (seats[i].chipImageForBet.gameObject.transform.position,
					targetPosition, step);

				if (Utils.isTwoPositionIsEqual (seats[i].chipImageForBet.gameObject.transform.position, targetPosition)) {
					isMoveChipArray [i] = false;
					seats[i].chipCountLabel.text = gamePlayController.game.currentRound.playerBets[i] + "";
					seats[i].chipCountLabel.gameObject.SetActive (true);
					isBetCompletedArray [i] = true;

					if (gamePlayController.state == GameState.Bet) {
						//if (IsAllBetCompleted && secondDealController.canSecondDeal) {
						if (IsAllBetCompleted) {
							Debug.Log ("In BetControoler: change to SecondSeal state");
							gamePlayController.game.HideStateLabel ();
							//gamePlayController.state = GameState.SecondDeal;
						}
					}
				}
			}
		}
	}

	private void HandleUser0BetNotify(int bet) {
		hasBet = true;
		gamePlayController.game.currentRound.playerBets[0] = bet;
		isMoveChipArray[0] = true;
	}

	public void SetBetClick(Button[] buttons) {
		for (int i = 0; i < buttons.Length; i++) {
			buttons [i].onClick.AddListener (BetClick);
		}
	}

	public void BetClick() {
		Debug.Log ("bet button click");
		if (gamePlayController.state != GameState.Bet) {
			Debug.LogError ("current state is not bet, the state is " + gamePlayController.state.value);
			return;
		}

		string betName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
		int index = int.Parse(betName.Substring (betName.Length - 1));
		int mybet = gamePlayController.game.currentRound.myBets [index];
			
		Socket socket = gamePlayController.gameSocket; 
		var req = new {
			roomNo = gamePlayController.game.roomNo,
			userId = Player.Me.userId,
			bet = mybet
		};

		socket.EmitJson (Messages.Bet, JsonConvert.SerializeObject(req), (string msg) => {
			HandleUser0BetNotify(mybet);
		}); 

	}

	public void HandleResponse(SomePlayerBetNotify notify) {
		Game game = gamePlayController.game;
		int index = game.GetSeatIndex (notify.userId);

		if (index == 0) {
			HandleUser0BetNotify (notify.bet);
		} else {
			game.currentRound.playerBets [index] = notify.bet;
			isMoveChipArray [index] = true;
		}
	}
}
