using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;

using cn.sharesdk.unity3d;




public class GamePlayController : MonoBehaviour {
	[SerializeField]
	private SetupCardGame setupCardGame;

	[SerializeField]
	private BeforeGameStartController beforeGameStartController;

	[SerializeField]
	private WaitForNextRoundController waitForNextRoundController;

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

	[SerializeField]
	private GameObject messagePanel;
	[SerializeField]
	private Text roomLabel;
	[SerializeField]
	private Text roundLabel;


	public GameState state {
		get {
			return game.state;
		}
		set {
			game.state = value;
		}
	}

	public bool isInited; //是否初始化好
	public bool isConnected; //是否已经连接到网络
	public Socket gameSocket;
	public Game game;

	[SerializeField]
	private ShareSDK ssdk;


	public void ShareClick() {
		
		ShareContent content = new ShareContent();
		content.SetText("this is a test string.");
		content.SetImageUrl("https://f1.webshare.mob.com/code/demo/img/1.jpg");
		content.SetTitle("test title");
		content.SetTitleUrl("http://www.mob.com");
		content.SetSite("Mob-ShareSDK");
		content.SetSiteUrl("http://www.mob.com");
		content.SetUrl("http://www.mob.com");
		content.SetComment("test description");
		content.SetMusicUrl("http://mp3.mwap8.com/destdir/Music/2009/20090601/ZuiXuanMinZuFeng20090601119.mp3");
		content.SetShareType(ContentType.Webpage);

	

		ssdk.ShareContent(PlatformType.WeChat, content);  // .ShowPlatformList(null, content, 100, 100);
	
	}

	// Use this for initialization
	void Start () {
		Debug.Log ("GamePlayController Start");
		SetGameData ();

		game.roomLabel = roomLabel;
		game.roundLabel = roundLabel;
		game.state = GameState.BeforeStart;
		game.bankerSignImage = setupCardGame.bankerSignImage;
		game.gameStateLabel = setupCardGame.gameStateLabel;
		game.gameStateLabelBackground = setupCardGame.gameStateLabelBackground;
		game.niuSprites = setupCardGame.niuSprites;
		game.multipleSprites = setupCardGame.multipleSprites;
		game.betButtons = setupCardGame.betButtons;
		game.betLabels = setupCardGame.betLabels;
		game.betButtonPositionsFor3Button = setupCardGame.betButtonPositionsFo3Button;
		game.betLabelPositionsFor3Button = setupCardGame.betLabelPositionsFo3Button;
		game.betButtonPositionsFor4Button = setupCardGame.betButtonPositionsFor4Button;
		game.betLabelPositionsFor4Button = setupCardGame.betLabelPositionsFor4Button;
		betController.SetBetClick (game.betButtons);

		foreach (Seat seat in game.seats) {
			seat.UpdateUI (game);
		}

		//初始化各个控制器
		waitForNextRoundController.Init();
		robBankerController.Init();
		chooseBankerController.Init ();
		betController.Init ();
		secondDealController.Init ();
		checkCardController.Init ();
		compareController.Init ();

		game.UpdateGameInfos ();
	}

	void Update() {
		if (!isConnected) {
			messagePanel.SetActive (true);
		} else {
			messagePanel.SetActive (false);
		}
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
		state = GameState.BeforeStart;

	}

	/*
	public void goToNextState() {
		state = state.nextState ();
	}*/

	public void Reset() {
	}

	public void PrepareForNewRound() {
		game.deck.Reset ();
		game.GoToNextRound ();
		for (int i = 0; i < Game.SeatCount; i++) {
			game.seats [i].Reset ();
		}

		beforeGameStartController.Reset ();
		firstDealerController.Reset ();
		robBankerController.Reset ();
		chooseBankerController.Reset ();
		betController.Reset ();
		secondDealController.Reset ();
		checkCardController.Reset ();
		compareController.Reset ();
	}

	public void SetGameSocket(Socket socket) {
		gameSocket = socket;
		gameSocket.On (Messages.GoToFirstDeal, new MessageHandler<FirstDealResponse, FirstDealerController> (firstDealerController, game).Handle);
		gameSocket.On (Messages.GoToCheckCard, new MessageHandler<GoToCheckCardNotify, CheckCardController> (checkCardController, game).Handle);
		gameSocket.On (Messages.SomePlayerSitDown, new MessageHandler<SomePlayerSitDownNotify, BeforeGameStartController> (beforeGameStartController, game).Handle);
		gameSocket.On (Messages.SomePlayerStandUp, new MessageHandler<SomePlayerStandUpNotify, BeforeGameStartController> (beforeGameStartController, game).Handle);
		gameSocket.On (Messages.StartGame, new MessageHandler<StartGameNotify, BeforeGameStartController> (beforeGameStartController, game).Handle);
		gameSocket.On (Messages.SomePlayerRobBanker, new MessageHandler<SomePlayerRobBankerNotify, RobBankerController> (robBankerController, game).Handle);
		gameSocket.On (Messages.GoToChooseBanker, new MessageHandler<GoToChooseBankerNotity, ChooseBankerController> (chooseBankerController, game).Handle);
		gameSocket.On (Messages.SomePlayerBet,  new MessageHandler<SomePlayerBetNotify, BetController> (betController, game).Handle);
		gameSocket.On (Messages.GoToSecondDeal, new MessageHandler<GoToSecondDealNotify, SecondDealController> (secondDealController, game).Handle);
		gameSocket.On (Messages.SomePlayerShowCard, new MessageHandler<SomePlayerShowCardNotify, CheckCardController> (checkCardController, game).Handle);
		gameSocket.On (Messages.GoToCompareCard, new MessageHandler<GoToCompareCardNotify, CompareCardController> (compareController, game).Handle);
		gameSocket.On (Messages.SomePlayerReady, new MessageHandler<SomePlayerReadyNotify, WaitForNextRoundController>(waitForNextRoundController, game).Handle);
	}

	public string GenerateRoomNo() {
		return "123456";
	}


	public void HandleResponse(JoinRoomResponse resp) {
		//根据resp设置Game的状态，设置Game的状态。
		if (resp.status != 0) {
			Debug.LogError ("status = " + resp.status + ", message = " + resp.errorMessage);
			throw new UnityException (resp.errorMessage);
		}

		GameState state = GameState.GetGameState (resp.state);
		game.totalRoundCount = resp.totalRoundCount;
		game.currentRoundNo = resp.currentRoundNo;
		game.state = state;
		if (state == GameState.BeforeStart) {
			//加载坐在座位的玩家信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			beforeGameStartController.SetUI ();

		} else if (state == GameState.RobBanker) {
			//加载坐在座位的玩家信息
			//加载我的牌的信息
			//加载抢庄的玩家信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			beforeGameStartController.SetUI ();
			firstDealerController.SetUI ();
			SetRobBankerPlayers (resp, game);
			SetMyCards (resp, game);
			SetMyBets (resp, game);
			robBankerController.SetUI ();


		} else if (state == GameState.Bet) {
			//加载坐在座位的玩家信息
			//加载下注的玩家信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			beforeGameStartController.SetUI ();
			firstDealerController.SetUI ();
			SetBanker (resp, game);
			SetMyBets (resp, game);
			SetBetPlayers (resp, game);
			SetMyCards (resp, game);
			robBankerController.SetUI ();
			chooseBankerController.SetUI ();
			betController.SetUI ();

		} else if (state == GameState.CheckCard) {
			//加载坐在座位的玩家信息
			//加载亮牌的玩家信息，以及他的牌的信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			beforeGameStartController.SetUI ();
			firstDealerController.SetUI ();

			SetBanker (resp, game);
			SetMyCards (resp, game);
			SetMyBets (resp, game);
			SetBetPlayers (resp, game);
			SetShowcardPlayers (resp, game);
			SetBetPlayers (resp, game);


			robBankerController.SetUI ();
			chooseBankerController.SetUI ();
			betController.SetUI ();
			secondDealController.SetUI ();
			checkCardController.SetUI ();

		} else if (state == GameState.WaitForNextRound) {
			//加载坐在座位的玩家信息
			//加载已经准备好的玩家信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			beforeGameStartController.SetUI ();

			SetReadyPlayers (resp, game);
			waitForNextRoundController.SetUI();


		} else if (state == GameState.GameOver) {
		} else {
			throw new UnityException ("客户端无法处理该状态，state = " + state);
		}
		isInited = true;
		game.isInited = isInited;

	}

	private void SetSitdownPlayers(JoinRoomResponse resp, Game game) {
		Dictionary<string, string> playerDict = resp.sitdownPlayers;
		foreach (KeyValuePair<string, string> pair in playerDict) 
		{
			string userId = pair.Key;
			string seatNo = pair.Value;
			int seatIndex = game.GetSeatIndexThroughSeatNo (seatNo);
			Seat seat = game.seats [seatIndex];
			Player player = new Player ();
			if (userId == Player.Me.userId) {
				player = Player.Me;
			}
			player.userId = userId;
			seat.player = player;
			player.seat = seat;
			//player.isPlaying = true;
		}

	}

	private void SetPlayingPlayers(JoinRoomResponse resp, Game game) {
		if (game.state != GameState.BeforeStart && game.state != GameState.WaitForNextRound) {
			Dictionary<string, string[]> cardsDict = resp.playerCards;
			foreach (KeyValuePair<string, string[]> pair in cardsDict) {
				string userId = pair.Key;
				int seatIndex = game.GetSeatIndex (userId);
				game.seats [seatIndex].player.isPlaying = true;
			}
		} else {
			for(int i = 0; i < Game.SeatCount; i++) {
				if (game.seats [i].player != null)
					game.seats [i].player.isPlaying = true;
			}
		}
	}


	private void SetMyCards(JoinRoomResponse resp, Game game) {
		var cards = resp.playerCards;
		if (cards.ContainsKey (Player.Me.userId)) {
			game.currentRound.playerCardsDict [Player.Me.userId] = new string[5];
			for (int i = 0; i < cards[Player.Me.userId].Length; i++) {
				game.currentRound.playerCardsDict[Player.Me.userId][i] = cards[Player.Me.userId][i];
			}
		}
	}


	private void SetRobBankerPlayers(JoinRoomResponse resp, Game game) {
		game.currentRound.robBankerDict = resp.robBankerPlayers;
	}

	private void SetBanker(JoinRoomResponse resp, Game game) {
		game.currentRound.banker = resp.banker;
	}

	private void SetMyBets(JoinRoomResponse resp, Game game) {
		Dictionary<string, int[]> playerBetsDict = resp.playerBets;
		foreach (KeyValuePair<string, int[]> pair in playerBetsDict) {
			string userId = pair.Key;
			int[] bets = pair.Value;
			if (userId == Player.Me.userId)
				game.currentRound.myBets = bets;
		}
	}

	private void SetBetPlayers(JoinRoomResponse resp, Game game) {
		Dictionary<string, int> betPlayerDict = resp.betPlayers;
		foreach (KeyValuePair<string, int> pair in betPlayerDict) {
			string userId = pair.Key;
			int bet = pair.Value;
			game.currentRound.playerBets [game.GetSeatIndex (userId)] = bet;
		}
	}

	private void SetShowcardPlayers(JoinRoomResponse resp, Game game) {
		Dictionary<string, string[]> cardsDict = resp.playerCards;
		Dictionary<string, ShowCardResult> showcardPlayersDict = resp.showcardPlayers;
		foreach (KeyValuePair<string, ShowCardResult> pair in showcardPlayersDict) {
			string userId = pair.Key;
			ShowCardResult showcardResult = pair.Value;

			int index = game.GetSeatIndex (userId);
			game.currentRound.cardSequenceArray [index] = showcardResult.cardSequences;
			game.currentRound.niuArray [index] = showcardResult.niu;
			game.currentRound.multipleArray [index] = showcardResult.multiple;

			foreach (KeyValuePair<string, string[]> kv in cardsDict) {
				string uid = kv.Key;
				string[] cards = kv.Value;
				if (userId == uid) {
					game.currentRound.playerCardsDict[uid] = cards;
					break;
				}
			}
		}
	}

	private void SetReadyPlayers(JoinRoomResponse resp, Game game) {
		Dictionary<string, bool> readyPlayerDict = resp.readyPlayers;
		foreach (KeyValuePair<string, bool> pair in readyPlayerDict) {
			string userId = pair.Key;
			int index = game.GetSeatIndex (userId);
			game.seats [index].player.isReady = true;
		}
	}
}
