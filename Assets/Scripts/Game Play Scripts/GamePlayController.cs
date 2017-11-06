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

	[SerializeField]
	private BetController betController;

	[SerializeField]
	private CompareCardController compareController;

	public GameState state {
		get {
			return game.state;
		}

		set {
			game.state = value;
		}
	}

	public Socket gameSocket;

	public Game game;


	// Use this for initialization
	void Start () {
		Debug.Log ("GamePlayController Start");
		SetGameData ();
		//setupCardGame.resetCards ();
		//beforeGameStartController.SetPlayerSeatUI ();


		game.state = GameState.BeforeStart;
		foreach (Seat seat in game.seats) {
			seat.UpdateUI (game);
		}

		chooseBankerController.Init ();
		betController.Init ();
		secondDealController.Init ();
		checkCardController.Init ();
		compareController.Init ();
	}

	private void SetGameData() {
		game = new Game ();
		game.seats = setupCardGame.seats;
		for (int i = 0; i < game.seats.Length; i++) {
			game.seats [i].game = game;
		}
		game.deck = setupCardGame.deck;

		game.totalRoundCount = 10;
		game.currentRoundNo = 1;
		game.roomNo = GenerateRoomNo ();
		state = GameState.Ready;
	}


	public void goToNextState() {
		state = state.nextState ();
	}

	public void Reset() {
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

		gameSocket.On (Messages.GoToCheckCard, (string msg) => {
			GoToCheckCardNotify resp = JsonConvert.DeserializeObject<GoToCheckCardNotify>(msg);
			checkCardController.HandleResponse(resp);
		});

		gameSocket.On (Messages.SomePlayerSitDown, (string msg) => {
			Debug.Log("SomePlayerSitDown: " + msg);
			SomePlayerSitDownNotify  resp = JsonConvert.DeserializeObject<SomePlayerSitDownNotify>(msg);
			beforeGameStartController.HandleResponse(resp);
		});

		gameSocket.On (Messages.SomePlayerStandUp, (string msg) => {
			Debug.Log("SomePlayerStandUp: " + msg);
			SomePlayerStandUpNotify resp = JsonConvert.DeserializeObject<SomePlayerStandUpNotify>(msg);
			beforeGameStartController.HandleResponse(resp);
		});

		gameSocket.On (Messages.StartGame, (string msg) => {
			Debug.Log("StartGame: " + msg);
			StartGameNotify resp = JsonConvert.DeserializeObject<StartGameNotify>(msg);
			firstDealerController.HandleResponse(resp);

		});

		gameSocket.On (Messages.SomePlayerRobBanker, (string msg) => {
			Debug.Log("SomePlayerRobBanker: " + msg);
			SomePlayerRobBankerNotify notify = JsonConvert.DeserializeObject<SomePlayerRobBankerNotify>(msg);
			robBankerController.HanldeResponse(notify);
		});

		gameSocket.On (Messages.GoToChooseBanker, (string msg) => {
			Debug.Log("GoToChooseBanker: " + msg);
			GoToChooseBankerNotity resp = JsonConvert.DeserializeObject<GoToChooseBankerNotity>(msg);
			chooseBankerController.HandleResponse(resp);
		});

		gameSocket.On (Messages.SomePlayerBet, (string msg) => {
			Debug.Log("SomePlayerBet: " + msg);
			SomePlayerBetNotify notify = JsonConvert.DeserializeObject<SomePlayerBetNotify>(msg);
			betController.HandleResponse(notify);
		});

		gameSocket.On (Messages.GoToSecondDeal, (string msg) => {
			Debug.Log("GoToSecondDeal: " + msg);
			GoToSecondDealNotify resp = JsonConvert.DeserializeObject<GoToSecondDealNotify>(msg);
			secondDealController.HandleResponse(resp);
		});

		gameSocket.On (Messages.SomePlayerShowCard, (string msg) => {
			Debug.Log("SomePlayerShowCard: " +msg);
			SomePlayerShowCardNotify resp = JsonConvert.DeserializeObject<SomePlayerShowCardNotify>(msg);
			checkCardController.HandleResponse(resp);
		});

		gameSocket.On (Messages.GoToCompareCard, (string msg) => {
			Debug.Log("GoToCompareCard: " + msg);
			GoToCompareCardNotify notify = JsonConvert.DeserializeObject<GoToCompareCardNotify>(msg);
			compareController.HandleResponse(notify);
		});

		gameSocket.On (Messages.SomePlayerReady, (string msg) => {
			Debug.Log("SomePlayerReady: " + msg);
			SomePlayerReadyNotify notify = JsonConvert.DeserializeObject<SomePlayerReadyNotify>(msg);
			beforeGameStartController.HandleResponse(notify);
		});
	}

	public string GenerateRoomNo() {
		return "123456";
	}

}
