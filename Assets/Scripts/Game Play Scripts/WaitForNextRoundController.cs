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
	private Button readyButton;

	private Seat[] seats;

	private float stateTimeLeft; //这状态停留的时间

	public void Init() {
		seats = gamePlayerController.game.seats;
		stateTimeLeft = Constants.MaxStateTimeLeft;
	}

	/**
	 * 游戏在一局之后，在下一局开始之前，需要重新设置界面或者变量
	 * */
	public override void Reset() {
		stateTimeLeft = Constants.MaxStateTimeLeft;
	}

	// Update is called once per frame
	void Update () {
		if (gamePlayerController.state == GameState.WaitForNextRound) {
			if (stateTimeLeft < 0) {
				gamePlayerController.game.ShowStateLabel ("下一局游戏即将开始: " + Mathf.Round(stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}
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
			//界面的元素全部还原，各个Controller全部Reset
			readyButton.gameObject.SetActive(false);

			gamePlayerController.game.UpdateGameInfos ();
			gamePlayerController.game.seats[0].player.isReady = true;
			gamePlayerController.game.seats[0].UpdateUI(gamePlayerController.game);

		});  
	}


	public void HandleResponse(SomePlayerReadyNotify notify) {
		int seatIndex = gamePlayerController.game.GetSeatIndex (notify.userId);
		seats [seatIndex].readyImage.gameObject.SetActive (true);
		gamePlayerController.game.seats[seatIndex].player.isReady = true;
		gamePlayerController.game.seats[seatIndex].UpdateUI(gamePlayerController.game);

	}
}
