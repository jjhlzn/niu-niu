using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;


public class GamePlayController : MonoBehaviour {
	[SerializeField]
	private SetupCardGame setupCardGame;

	[SerializeField]
	private BeforeGameStartController beforeGameStartController;

	[SerializeField]
	private FirstDealerController firstDealerController;

	[SerializeField]
	private RobBankerController robBankerController;

	[SerializeField]
	private ChooseBankerController chooseBankerController;

	[SerializeField]
	private SecondDealController secondDealController;

	[SerializeField]
	private CheckCardController checkCardController;

	public GameState state;

	public Socket gameSocket;

	public Game game;


	// Use this for initialization
	void Start () {
		Debug.Log ("GamePlayController Start");
		SetGameData ();
		setupCardGame.resetCards ();
		beforeGameStartController.SetPlayerSeatUI ();
		beforeGameStartController.SetSeatClick ();
	}

	private void SetGameData() {
		game = new Game ();
		game.totalRoundCount = 10;
		game.currentRoundNo = 1;
		state = GameState.Ready;
	}

	public void StartClick() {
		if (gameSocket == null || !gameSocket.IsConnected) {
			return;
		}
		setupCardGame.resetCards ();
		Debug.Log ("start game click");

		//make start game request
		var request = new {
			type = "startGame",
			data = new {
				roomNo = "123456"
			},
			userId = "1234566"
		};

		string json = JsonConvert.SerializeObject(request);

		gameSocket.EmitJson (Connect.GameInstructionEvent, json, (string msg) => {
			//TODO 处理游戏开始的结果
		});

	}

	public void goToNextState() {
		state = state.nextState ();
	}

	public void SetGameSocket(Socket socket) {
		gameSocket = socket;

		gameSocket.On (Messages.GoToFirstDeal, (string msg) => {
			//检查消息的类型，根据消息的类型，将消息转为为相应的类型。
			Debug.Log("GoToFirstDeal: " + msg);
			FirstDealResponse resp = JsonConvert.DeserializeObject<FirstDealResponse>(msg);
			//Debug.Log(resp);
			firstDealerController.HandleResponse(resp);
		});

		gameSocket.On (Messages.GoToChooseBanker, (string msg) => {
			Debug.Log("GoToChooseBanker: " + msg);
			GoToChooseBankerNotity resp = JsonConvert.DeserializeObject<GoToChooseBankerNotity>(msg);
			chooseBankerController.HandleResponse(resp);
		});

		gameSocket.On (Messages.GoToSecondDeal, (string msg) => {
			GoToSecondDealNotify resp = JsonConvert.DeserializeObject<GoToSecondDealNotify>(msg);
			secondDealController.HandleResponse(resp);
		});

		gameSocket.On (Messages.GoToCheckCard, (string msg) => {
			GoToCheckCardNotify resp = JsonConvert.DeserializeObject<GoToCheckCardNotify>(msg);
			checkCardController.HandleResponse(resp);
		});
	}

}
