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

	private float chipMoveSpeed = 24f;

	private Image[] chipImages;
	private Image[] chipPositionImages;
	private Text[] chipCountLabels;

	private Vector3[] chipOriginPositions;

	private bool[] isMoveChipArray;
	private bool[] isBetCompletedArray;
	private bool hasBet = false;

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

	public void SetChipImages(Image[] chipImages) {
		this.chipImages = chipImages;
		chipOriginPositions = new Vector3[chipImages.Length];
		for (int i = 0; i < chipImages.Length; i++) {
			chipOriginPositions [i] = chipImages [i].gameObject.transform.position;
		}
	}

	public void SetChipPositionImages(Image[] chipPositionImages) {
		this.chipPositionImages = chipPositionImages;
	}

	public void SetChipCountLabels(Text[] labels) {
		this.chipCountLabels = labels;
	}
		
	public void Awake() {
		isMoveChipArray = new bool[Game.SeatCount];
		isBetCompletedArray = new bool[Game.SeatCount];
		for (int i = 0; i < Game.SeatCount; i++) {
			isMoveChipArray [i] = false;
			isBetCompletedArray [i] = false;
		}
	}

	public override void Reset() {
		hasBet = false;
		for (int i = 0; i < chipImages.Length; i++) {
			chipImages [i].gameObject.SetActive (false);
			chipImages [i].transform.position = chipOriginPositions [i];
			chipPositionImages [i].gameObject.SetActive (false);
			chipCountLabels [i].gameObject.SetActive (false);
			isMoveChipArray [i] = false;
			isBetCompletedArray [i] = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (gamePlayController.state == GameState.Bet) {
			if (!hasBet) {
				betPanel.SetActive (true);
			} else {
				betPanel.SetActive (false);
			}

			for (int i = 0; i < isMoveChipArray.Length; i++) {
				
				if (isMoveChipArray [i]) {
					chipImages [i].gameObject.SetActive (true);


					Vector3 targetPosition = chipPositionImages [i].transform.position;
					float step = chipMoveSpeed * Time.deltaTime;
					chipImages [i].gameObject.transform.position = Vector3.MoveTowards (chipImages [i].gameObject.transform.position,
						targetPosition, step);

					if (Utils.isTwoPositionIsEqual (chipImages [i].gameObject.transform.position, targetPosition)) {
						isMoveChipArray [i] = false;
						chipCountLabels [i].text = gamePlayController.game.currentRound.playerBets[i] + "";
						chipCountLabels [i].gameObject.SetActive (true);
						isBetCompletedArray [i] = true;

						if (IsAllBetCompleted && secondDealController.canSecondDeal) {
							Debug.Log ("In BetControoler: change to SecondSeal state");
							gamePlayController.state = GameState.SecondDeal;
						}
					}
				}
			}

		} else {
			betPanel.SetActive (false);
		}


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
			betPanel.gameObject.SetActive (false);
			hasBet = true;
			isMoveChipArray[0] = true;
			gamePlayController.game.currentRound.playerBets[0] = 8;
		}); 

	}

	public void HandleResponse(SomePlayerBetNotify notify) {
		Game game = gamePlayController.game;
		int index = game.GetSeatIndex (notify.userId);

		if (game.currentRound.playerBets[index] == 0){
			game.currentRound.playerBets [index] = notify.bet;
			isMoveChipArray [index] = true;
		}
	}
}
