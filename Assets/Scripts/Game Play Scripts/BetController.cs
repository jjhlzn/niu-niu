using System.Collections;
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

	[SerializeField]
	private GameObject betPanel;

	private float user0ChipMoveSpeed = 180f;
	private float chipMoveSpeed = 150f;

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
				if (seats [i].hasPlayer () && seats[i].player.userId != gamePlayController.game.currentRound.banker) {
					result = result && isBetCompletedArray [i];
				}
			}
			return result;
		}
	}

	public void Init() {
		seats = gamePlayController.game.seats;
		stateTimeLeft = Constants.MaxStateTimeLeft - animationTime;
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
	
	// Update is called once per frame
	void Update () {


		if (gamePlayController.state == GameState.Bet) {

			if (stateTimeLeft > 0) {
				gamePlayController.game.ShowStateLabel ("请选择下注分数: " + Mathf.Round(stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}
				
			if (!hasBet) {
				betPanel.SetActive (true);
			} else {
				betPanel.SetActive (false);
			}

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

						if (IsAllBetCompleted && secondDealController.canSecondDeal) {
							Debug.Log ("In BetControoler: change to SecondSeal state");
							gamePlayController.game.HideStateLabel ();
							gamePlayController.state = GameState.SecondDeal;
						}
					}
				}
			}

		} else {
			betPanel.SetActive (false);
		}


	}

	private void HandleUser0BetNotify(int bet) {
		betPanel.gameObject.SetActive (false);
		hasBet = true;
		gamePlayController.game.currentRound.playerBets[0] = 8;
		isMoveChipArray[0] = true;
	}

	public void BetClick() {
		Debug.Log ("bet button click");
		if (gamePlayController.state != GameState.Bet) {
			Debug.LogError ("current state is not bet, the state is " + gamePlayController.state.value);
			return;
		}
			
		Socket socket = gamePlayController.gameSocket; 
		var req = new {
			roomNo = gamePlayController.game.roomNo,
			userId = Player.Me.userId,
			bet = 8
		};

		socket.EmitJson (Messages.Bet, JsonConvert.SerializeObject(req), (string msg) => {
			HandleUser0BetNotify(8);
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
		//if (game.currentRound.playerBets[index] == 0){

		//}
	}
}
