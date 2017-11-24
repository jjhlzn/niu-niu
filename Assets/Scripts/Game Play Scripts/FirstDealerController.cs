using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstDealerController : BaseStateController {
	public static float dealSpeed = 320f; //发牌速度
	public static float dealWaitTimeBetweenPlayer = 0.3f;
	public static float dealWaitTimeBetweenPlayerForSecondDeal = 0.1f;
	public static float waitTimeDeltaBetweenCard = 0.1f;
	public static float turnUpTime = 0.5f;
	public static float user0CardScale = 1.32f;

	[SerializeField]
	private GamePlayController gamePlayController;
	[SerializeField]
	private BeforeGameStartController beforeGameStartController;

	[SerializeField]
	private GameObject deckCardPosition; //发牌位置


	private Deck deck;
	private Seat[] seats;

	private bool isFirstDealing;
	public bool isFirstDealDone;


	void Start () {
		seats = gamePlayController.game.seats;
		deck = gamePlayController.game.deck;
	}

	public void Init() {
		seats = gamePlayController.game.seats;
		deck = gamePlayController.game.deck;
		isFirstDealing = false;
		isFirstDealDone = false;
	}

	public override void Reset() {
		isFirstDealing = false;
		isFirstDealDone = false;
	}
		
	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	// Update is called once per frame
	public new void Update ()  {
		base.Update ();

		if (gamePlayController.state.Equals (GameState.FirstDeal)) {
			gamePlayController.game.HideStateLabel ();
		} 

		FirstDealAnimation ();
	}



	private void FirstDealAnimation() {
		var game = gamePlayController.game;
		var round = game.currentRound;
		if (isFirstDealing && !beforeGameStartController.isMoveSeat) {
			float waitTime = 0;
			List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;

			for (int i = 0; i < playingPlayers.Count; i++) {
				Player player = playingPlayers [i];
				//Debug.Log ("player: " + player.userId + " sit on seats: " + player.seat.seatIndex);
				FirstGiveCardsAnimation (player, waitTime);

				//每个成员之间加入一个延时
				waitTime += dealWaitTimeBetweenPlayer;
			}

			int playerCount = gamePlayController.game.PlayerCount;
			//判断最后一张牌是否已经发好
			if (Utils.isTwoPositionIsEqual(playingPlayers[playingPlayers.Count - 1].seat.cards[3].transform.position, 
				playingPlayers[playingPlayers.Count - 1].seat.cardPositions[3])) {
				//Debug.Log ("Player.Me.isPlaying = " + Player.Me.isPlaying);
				if (Player.Me.isPlaying) {
					StartCoroutine (TurnCardUp (playingPlayers [0].seat.cards [0], round.playerCardsDict[Player.Me.userId] [0]));
					StartCoroutine (TurnCardUp (playingPlayers [0].seat.cards [1], round.playerCardsDict[Player.Me.userId] [1]));
					StartCoroutine (TurnCardUp (playingPlayers [0].seat.cards [2], round.playerCardsDict[Player.Me.userId] [2]));
					StartCoroutine (TurnCardUp (playingPlayers [0].seat.cards [3], round.playerCardsDict[Player.Me.userId] [3]));
				}

				isFirstDealing = false;
				isFirstDealDone = true;
				StartCoroutine (GoToNextState ());
			}
		}
	}

	public void SetUI() {
		Debug.Log ("FirstDealController.SetUI() called");
		var game = gamePlayController.game;
		FirstDeal ();
		List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;
		//Debug.Log ("playingPlayers.count = " + playingPlayers.Count);
		for (int i = 0; i < playingPlayers.Count; i++) {

			Player player = playingPlayers [i];
			Debug.Log ("player.userId = " + player.userId);
			Image[] cards = player.seat.cards;
			Vector3[] targetCardPositions = player.seat.cardPositions;
			for (int j = 0; j < 4; j++) {
				Vector3 targetCard = targetCardPositions [j];
				cards [j].transform.position = targetCard;
				if (i == 0) {
					Vector3 localScale = new Vector3 ();
					localScale.x = user0CardScale;
					localScale.y = user0CardScale;
					cards [j].transform.localScale = localScale;
					if (game.currentRound.playerCardsDict.ContainsKey(player.userId))
						cards [j].sprite = deck.GetCardFaceImage (game.currentRound.playerCardsDict[player.userId][j]);
				}

				cards [j].gameObject.SetActive (true);
			}


		}
		isFirstDealDone = true;
	}

	/**
	 * 发4张牌给指定的玩家的动画
	 * */

	private void FirstGiveCardsAnimation(Player player, float waitTime) {
		float step = dealSpeed * Time.deltaTime;
		Image[] cards = player.seat.cards;
		Vector3[] targetCardPositions = player.seat.cardPositions;
		for (int i = 0; i < 4; i++) {
			Vector3 targetCard = targetCardPositions [i];
			StartCoroutine(GiveCardAnimation(player, cards[i], targetCard, step, waitTime));
			waitTime += waitTimeDeltaBetweenCard;
		}
	}

	IEnumerator GiveCardAnimation(Player player, Image card, Vector3 targetCard, float step, float waitTime) {

		yield return new WaitForSeconds (waitTime);

		card.transform.position = Vector3.MoveTowards(card.gameObject.transform.position, targetCard, step);
		if (player.seat.seatIndex == 0) {
			Vector3 localScale = new Vector3 ();
			localScale.x = user0CardScale;
			localScale.y = user0CardScale;
			card.transform.localScale = localScale;
		}
	}



	IEnumerator TurnCardUp(Image card, string cardValue) {
		if (!string.IsNullOrEmpty (cardValue)) {
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("TurnUp");
			yield return new WaitForSeconds (turnUpTime);
			card.sprite = deck.GetCardFaceImage (cardValue);

			anim.Play ("TurnBackNow2");
		}
	    yield return new WaitForSeconds (turnUpTime);
	}


	IEnumerator GoToNextState() {
		yield return new WaitForSeconds (.3f);

		if (gamePlayController.state == GameState.FirstDeal)
			gamePlayController.state = GameState.RobBanker;
	}

	private void FirstDeal() {
		List<Player> players = gamePlayController.game.PlayingPlayers;

		for (int i = 0; i < players.Count; i++) {
			FirstDeal (players[i]);
		}
	}

	private void FirstDeal(Player player) {
		player.seat.cards [0] = deck.Deal ();
		player.seat.cards [1] = deck.Deal ();
		player.seat.cards [2] = deck.Deal ();
		player.seat.cards [3] = deck.Deal ();
	}

	/******* 处理服务器的通知***************/
	public void HandleResponse(FirstDealResponse notify) {
		Dictionary<string, string[]> cardsDict = notify.cardsDict;
		Dictionary<string, int[]> betsDict = notify.betsDict;
		int roundNo = notify.roundNo;
		gamePlayController.game.currentRoundNo = roundNo;
		gamePlayController.PrepareForNewRound();
		HandleResponse (cardsDict, betsDict);
	}

	public void HandleResponse(StartGameNotify notify) {
		Dictionary<string, string[]> cardsDict = notify.cardsDict;
		Dictionary<string, int[]> betsDict = notify.betsDict;

		HandleResponse (cardsDict, betsDict);
	}
		

	private void HandleResponse(Dictionary<string, string[]> cardsDict, Dictionary<string, int[]> betsDict) {
		var game = gamePlayController.game;

		//更新位置UI
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer ()) {
				seats [i].player.isReady = false;
				seats [i].UpdateUI (gamePlayController.game);
				seats [i].scoreLabel.transform.position = seats [i].originScoreLabelPosition;
			}
		}

		foreach (string uid in cardsDict.Keys) {
			seats [game.GetSeatIndex (uid)].player.isPlaying = true;
		}

		string[] cards;
		string userId = seats [0].player.userId;
		Debug.Log ("userId = " + userId);
		if (cardsDict.ContainsKey (userId)) {
			cards = cardsDict [userId];
		} else {
			throw new UnityException ("找不到UserId = " + userId + "的牌");
		}

		int[] bets;
		if (betsDict.ContainsKey (userId)) {
			bets = betsDict [userId];
		} else {
			throw new UnityException ("找不到UserId = " + userId + "的可下注的筹码量");
		}
		gamePlayController.game.currentRound.playerCardsDict [userId] = new string[5];
		for (int i = 0; i < cards.Length; i++) {
			gamePlayController.game.currentRound.playerCardsDict[userId][i] = cards[i];
		}

		gamePlayController.game.currentRound.myBets = bets;

		gamePlayController.game.StartGame ();

		deck.ShowNotDealCardsForFirstDeal (gamePlayController.game.PlayingPlayers.Count);

		//先把数据结构设置好，再在Update()中执行发牌的动画。
		FirstDeal ();

		gamePlayController.state = GameState.FirstDeal;
		gamePlayController.game.UpdateGameInfos ();
		if (beforeGameStartController.IsNeedMoveSeat()) {
			beforeGameStartController.MoveSeats (beforeGameStartController.getMoveSeatIndex ());
		}
		isFirstDealing = true;
	}



}
