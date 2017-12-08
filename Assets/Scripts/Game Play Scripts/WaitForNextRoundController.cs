using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using socket.io;

public class WaitForNextRoundController : BaseStateController {

	private static float moveSeatSpeed = 30f;

	[SerializeField]
	private GamePlayController gamePlayerController;
	[SerializeField]
	private SetupCardGame setUpGameController; 
	[SerializeField]
	private BeforeGameStartController beforeGameStartController;

	[SerializeField]
	private Button readyButton;

	private Seat[] seats {
		get {
			return gamePlayerController.game.seats;
		}
	}

	private float stateTimeLeft; //这状态停留的时间
	private bool hasPlayCountDown;
	//public bool hasReady;

	public void Init() {
		stateTimeLeft = Constants.MaxStateTimeLeft;
		//hasReady = false;
	}

	/**
	 * 游戏在一局之后，在下一局开始之前，需要重新设置界面或者变量
	 * */
	public override void Reset() {
		stateTimeLeft = Constants.MaxStateTimeLeft;
		hasPlayCountDown = false;
		//hasReady = false;
	}

	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayerController;
	}

	// Update is called once per frame
	public new void Update() {
		if (gamePlayerController.state == GameState.WaitForNextRound) {

			if (stateTimeLeft <= 3.5f && !hasPlayCountDown) {
				hasPlayCountDown = true;
				MusicController.instance.Play (AudioItem.CountDown);
			}

			if (stateTimeLeft > 0) {
				gamePlayerController.game.ShowStateLabel ("下一局游戏即将开始: " + Mathf.Round(stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}

			var game = gamePlayerController.game;
			if (Player.Me.isPlaying && !Player.Me.hasReady) {
				readyButton.gameObject.SetActive (true);
			} else {
				readyButton.gameObject.SetActive (false);
			}

			//Debug.Log ("Player.Me.seat != null is " + (Player.Me.seat != null));
			//Debug.Log ("Player.Me.seat.seatIndex is " + Player.Me.seat.seatIndex);
			if (!Player.Me.isPlaying && Player.Me.seat != null && Player.Me.seat.seatIndex != 0) {
				beforeGameStartController.MoveSeats (Player.Me.seat.seatIndex);
				Player.Me.isPlaying = true;
			}
		}
	}

	private void CheckMeReady() {
		var game = gamePlayerController.game;
		//var round = game.currentRound;
		int seatIndex = game.GetSeatIndex (Player.Me.userId);
		if (seatIndex != -1 && game.seats [seatIndex].player.isReady) {
			Player.Me.hasReady = true;
		} else {
			Player.Me.hasReady = false;
		}
	}

	public void SetUI() {
		var game = gamePlayerController.game;
		CheckMeReady ();

		for (int i = 0; i < game.seats.Length; i++) {
			game.seats [i].UpdateUI (game);
		}
	}


	public void ReadyClick() {
		Socket gameSocket = gamePlayerController.gameSocket;
		Debug.Log ("ready  click");

		//make start game request
		var request = new {
			roomNo = gamePlayerController.game.roomNo,
			userId = Player.Me.userId
		};

		gameSocket.EmitJson (Messages.Ready, JsonConvert.SerializeObject(request), (string msg) => {
		});  
	}


	public void HandleResponse(SomePlayerReadyNotify notify) {
		int seatIndex = gamePlayerController.game.GetSeatIndex (notify.userId);
		seats [seatIndex].readyImage.gameObject.SetActive (true);
		if (seats [seatIndex].player.userId == Player.Me.userId) {
			//界面的元素全部还原，各个Controller全部Reset
			Player.Me.hasReady = true;
			readyButton.gameObject.SetActive(false);

			gamePlayerController.game.UpdateGameInfos ();
			gamePlayerController.game.seats[0].player.isReady = true;
			gamePlayerController.game.seats[0].UpdateUI(gamePlayerController.game);
			MusicController.instance.Play (AudioItem.Ready, seats [0].player.sex);
		} else {
			gamePlayerController.game.seats[seatIndex].player.isReady = true;
			gamePlayerController.game.seats[seatIndex].UpdateUI(gamePlayerController.game);
			MusicController.instance.Play (AudioItem.Ready, seats [seatIndex].player.sex);
		}
	}
}
