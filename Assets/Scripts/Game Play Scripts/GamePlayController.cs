using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
//using socket.io;
using BestHTTP.SocketIO;
using System;

using cn.sharesdk.unity3d;


public class GamePlayController : BaseMonoBehaviour {
	[Header("Controller")]
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
	private GameOverController gameOverController;

	[Header("UI")]
	[SerializeField]
	private GameObject messagePanel;
	[SerializeField]
	private GameObject connectFailMessagePanel;
	[SerializeField]
	private Text roomLabel;
	[SerializeField]
	private Text roundLabel;
	[SerializeField]
	private Button menuButton;

	[SerializeField]
	private ShareSDK ssdk;

	public bool isInited; //是否初始化好
	public bool isConnected; //是否已经连接到网络
	public DateTime pauseTime = DateTime.Now;
	public GameState pauseState;
	public Socket gameSocket;
	public Game game;
	public Connect connect;


	// Use this for initialization
	void Start () {
		Debug.Log ("--------------------------------------------------------");
		Debug.Log ("GamePlayController.Start() called");
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		CreateGame ();
	}

	private void CreateGame() {
		game = new Game ();
		game.seats = setupCardGame.seats;
		for (int i = 0; i < game.seats.Length; i++) {
			game.seats [i].game = game;
		}
		game.deck = setupCardGame.deck;

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
		//menuButton.gameObject.transform.SetAsLastSibling ();
	}

	void Update() {
		if (!isConnected && !connectFailMessagePanel.activeInHierarchy) {
			
			messagePanel.SetActive (true);
			messagePanel.transform.SetAsLastSibling ();
		} else {
			messagePanel.SetActive (false);
		}
	}

	public void Reset() {
		PrepareForNewRound ();
	}

	public void PrepareForNewRound() {
		game.GoToNextRound ();
		ResetUI ();
	}

	public void ResetUI() {
		game.deck.Reset ();
		for (int i = 0; i < Game.SeatCount; i++) {
			game.seats [i].Reset ();
		}
		var playingPalyers = game.PlayingPlayers;
		foreach (var player in playingPalyers)
			player.Reset ();

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
		//if (gameSocket != null && gameSocket != socket)
		//	return;
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
		gameSocket.On (Messages.GoToGameOver, new MessageHandler<GameOverResponse, GameOverController> (gameOverController, game).Handle);
		gameSocket.On (Messages.SomePlayerDelegate, new MessageHandler<SomePlayerDeleteNotify, BeforeGameStartController> (beforeGameStartController, game).Handle);
		gameSocket.On (Messages.SomePlayerNotDelegate, new MessageHandler<SomePlayerNotDeleteNotify, BeforeGameStartController> (beforeGameStartController, game).Handle);
		gameSocket.On (Messages.RoomHasDismissed, new MessageHandler<RoomHasDismissedNotify, BeforeGameStartController> (beforeGameStartController, game).Handle);
	}

	public void HandleResponse(JoinRoomResponse resp) {
		//根据resp设置Game的状态，设置Game的状态。
		if (resp.status != 0) {
			Debug.LogError ("status = " + resp.status + ", message = " + resp.errorMessage);
			Dictionary<string, string> parameters = new Dictionary<string, string> ();
			parameters[Utils.Message_Key] = "加入房间失败";
			Scenes.Load ("MainPage", parameters);
			return;
		}

		if (string.IsNullOrEmpty (resp.roomNo)) {
			Dictionary<string, string> parameters = new Dictionary<string, string> ();
			parameters[Utils.Message_Key] = "加入房间失败";
			Scenes.Load ("MainPage", parameters);
			return;
		}

		if (game == null) {
			Debug.Log ("game is null, CreatGame() is called");
			CreateGame ();

		}
		if (System.IO.File.Exists(Utils.GetShareGameResultUrl())) {
			System.IO.File.Delete(Utils.GetShareGameResultUrl());
			Debug.Log ("delete file: " + Utils.GetShareGameResultUrl ());
		}
		game.totalRoundCount = resp.totalRoundCount;
		game.currentRoundNo = resp.currentRoundNo;
		game.roomNo = resp.roomNo;
		game.creater = resp.creater;
		GameState state = GameState.GetGameState (resp.state);
		game.rounds = new Round[game.totalRoundCount];
		Round round = new Round ();
		game.rounds [game.currentRoundNo - 1] = round;
		SetRounds (resp, game);

		Reset ();

	
		game.state = state;
		if (state == GameState.BeforeStart) {
			//加载坐在座位的玩家信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			SetDelegatePlayers (resp, game);
			beforeGameStartController.SetUI ();

		} else if (state == GameState.RobBanker) {
			//加载坐在座位的玩家信息
			//加载我的牌的信息
			//加载抢庄的玩家信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			SetDelegatePlayers (resp, game);

			SetRobBankerPlayers (resp, game);
			SetMyCards (resp, game);

			beforeGameStartController.SetUI ();

			//需要在移动位置之后设置
			SetMyBets (resp, game);

			firstDealerController.SetUI ();
			robBankerController.SetUI ();


		} else if (state == GameState.Bet) {
			//加载坐在座位的玩家信息
			//加载下注的玩家信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			SetDelegatePlayers (resp, game);

			SetBanker (resp, game);
			SetMyBets (resp, game);

			beforeGameStartController.SetUI ();
			SetBetPlayers (resp, game);
			SetMyCards (resp, game);

			firstDealerController.SetUI ();
			robBankerController.SetUI ();
			chooseBankerController.SetUI ();
			betController.SetUI ();

		} else if (state == GameState.CheckCard) {
			//加载坐在座位的玩家信息
			//加载亮牌的玩家信息，以及他的牌的信息
			SetSitdownPlayers (resp, game);
			SetPlayingPlayers (resp, game);
			SetDelegatePlayers (resp, game);

			SetBanker (resp, game);
			SetMyCards (resp, game);
			SetMyBets (resp, game);

			beforeGameStartController.SetUI ();

			SetBetPlayers (resp, game);
			SetShowcardPlayers (resp, game);
			SetBetPlayers (resp, game);


			firstDealerController.SetUI ();
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
			SetDelegatePlayers (resp, game);

			beforeGameStartController.SetUI ();

			SetReadyPlayers (resp, game);

			waitForNextRoundController.SetUI();


		} else if (state == GameState.GameOver) {
		} else {
			throw new UnityException ("客户端无法处理该状态，state = " + state);
		}
		game.UpdateSeatUIs ();

		isInited = true;
		game.isInited = isInited;

		game.UpdateGameInfos ();
	}

	private void SetRounds(JoinRoomResponse resp, Game game) {
		for (int i = 0; i < game.currentRoundNo - 1; i++) {
			game.rounds [i] = resp.rounds [i];
		}
	}

	private void SetSitdownPlayers(JoinRoomResponse resp, Game game) {
		Dictionary<string, JoinRoomResponsePlayerInfo> playerDict = resp.sitdownPlayers;
		foreach (KeyValuePair<string, JoinRoomResponsePlayerInfo> pair in playerDict) 
		{
			string userId = pair.Key;
			JoinRoomResponsePlayerInfo userInfo = pair.Value;
			int seatIndex = game.GetSeatIndexThroughSeatNo (userInfo.seat);
			Seat seat = game.seats [seatIndex];
			Player player = new Player ();
			if (userId == Player.Me.userId) {
				player = Player.Me;
			} else {
				player.userId = userId;
				player.sex = userInfo.sex;
				player.nickname = userInfo.nickname;
				player.headimgurl = userInfo.headimgurl;
				player.ip = userInfo.ip;

			}
			if (resp.scores.ContainsKey (userId))
				player.score = resp.scores [userId];
			else
				player.score = 0;
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

	private void SetDelegatePlayers(JoinRoomResponse resp, Game game) {
		foreach (string userId in resp.delegatePlayers) {
			var seatIndex = game.GetSeatIndex (userId);
			if (seatIndex != -1) {
				if (game.seats [seatIndex].player.userId != Player.Me.userId)
					game.seats [seatIndex].player.isDelegate = true;
			}
		}
	}


	private void SetMyCards(JoinRoomResponse resp, Game game) {
		var cards = resp.playerCards;
		//Debug.Log ("cards.keys: " + cards.Keys);
		//Debug.Log ("Me.userId: " + Player.Me.userId);
		//Debug.Log ("cards.ContainsKey (Player.Me.userId): " + cards.ContainsKey (Player.Me.userId));
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

	//TODO 需要在寻转座位之后进行，进行重构
	private void SetBetPlayers(JoinRoomResponse resp, Game game) {
		Dictionary<string, int> betPlayerDict = resp.betPlayers;
		foreach (KeyValuePair<string, int> pair in betPlayerDict) {
			string userId = pair.Key;
			int bet = pair.Value;
			//有问题，因为现在的位置不是最终的位置。
			game.currentRound.playerBets [userId] = bet;
			//Debug.Log ("Set " + userId + " index = " + game.GetSeatIndex(userId) + " bet = " + bet);
		}
	}

	//TODO 需要在寻转座位之后进行，进行重构
	private void SetShowcardPlayers(JoinRoomResponse resp, Game game) {
		Dictionary<string, string[]> cardsDict = resp.playerCards;
		Dictionary<string, ShowCardResult> showcardPlayersDict = resp.showcardPlayers;
		foreach (KeyValuePair<string, ShowCardResult> pair in showcardPlayersDict) {
			string userId = pair.Key;
			ShowCardResult showcardResult = pair.Value;

			//这是的位置不是用户最终的位置。
			int index = game.GetSeatIndex (userId);
			game.currentRound.cardSequenceArray [userId] = showcardResult.cardSequences;
			game.currentRound.niuArray [userId] = showcardResult.niu;
			game.currentRound.multipleArray [userId] = showcardResult.multiple;

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
		

	void OnApplicationPause(bool pauseStatus)
	{
		//isPaused = pauseStatus;
		Debug.Log("OnApplicationPause: pauseStatus = " + pauseStatus);



		if (pauseStatus) {
			pauseTime = DateTime.Now;
			if (isConnected) {
				gameSocket.Emit(Messages.Delegate, JsonConvert.SerializeObject(new {userId = Player.Me.userId, 
					roomNo = game.roomNo,
					clientInfo = Utils.GetClientInfo(),
					userInfo = Utils.GetUserInfo()}));
			}
		} else {
			if (gameSocket != null && gameSocket.IsOpen)
				gameSocket.Emit(Messages.NotDelegate, JsonConvert.SerializeObject(new {userId = Player.Me.userId, 
					roomNo = game.roomNo,
					clientInfo = Utils.GetClientInfo(),
					userInfo = Utils.GetUserInfo()}));

			if (game != null  &&  (game.state == GameState.GameOver || game.state == GameState.BeforeStart ))
				return;

			DateTime now = DateTime.Now;
			var differ = now - pauseTime;
			double seconds = differ.TotalSeconds;

			int pauseSecs = 15;

			if (pauseState == GameState.CompareCard || pauseState == GameState.CheckCard) {
				pauseSecs = 10;
			} else if (pauseState == GameState.BeforeStart || pauseState == GameState.GameOver) {
				pauseSecs = 60 * 50;
			} else if (pauseState == GameState.FirstDeal || pauseState == GameState.RobBanker 
				|| pauseState == GameState.ChooseBanker) {
				pauseSecs = 30;
			} 

			if (seconds < pauseSecs) {
				//什么都不需要坐
			} else {
				isConnected = false;
				isInited = false;
				game.isInited = false;
				gameSocket.Manager.Close ();
				if (connect != null) {
					connect.connect ();
				}
			}
		}
	}

	public void ConnectFailConfirmClick() {
		connectFailMessagePanel.gameObject.SetActive (false);
	}

	public void ShowConnectFailMessage() {
		if (!connectFailMessagePanel.gameObject.activeInHierarchy) {
			connectFailMessagePanel.gameObject.SetActive (true);
		}
	}

	void OnEnable()
	{
		Application.logMessageReceived += LogCallback;
	}

	void OnDisable() {

		Application.logMessageReceived -= LogCallback;
	}


	public GameState state {
		get {
			return game.state;
		}
		set {
			game.state = value;
		}
	}
}
