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
	public static float user0CardScale = 1.3f;

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private GameObject deckCardPosition; //发牌位置


	private Deck deck;
	private Seat[] seats;

	private bool isFirstStartDealUpdate;


	void Start () {
		seats = gamePlayController.game.seats;
		deck = gamePlayController.game.deck;
	}

	public void Init() {
		seats = gamePlayController.game.seats;
		deck = gamePlayController.game.deck;
		isFirstStartDealUpdate = true;

	}

	public override void Reset() {
		isFirstStartDealUpdate = true;

	}
		
	void Update () {

		if (gamePlayController.state.Equals (GameState.FirstDeal)) {
			gamePlayController.game.HideStateLabel ();
			
			float waitTime = 0;
			List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;
			for (int i = 0; i < playingPlayers.Count; i++) {
				Player player = playingPlayers [i];
				FirstGiveCardsAnimation (player, waitTime);

				//每个成员之间加入一个延时
				waitTime += dealWaitTimeBetweenPlayer;
			}

			int playerCount = gamePlayController.game.PlayerCount;
			//判断最后一张牌是否已经发好
			if (Utils.isTwoPositionIsEqual(playingPlayers[playingPlayers.Count - 1].seat.cards[3].transform.position, playingPlayers[playingPlayers.Count - 1].seat.cardPositions[3])) {
				StartCoroutine (TurnCardUp (playingPlayers[0].seat.cards[0], gamePlayController.game.currentRound.myCards[0]));
				StartCoroutine (TurnCardUp (playingPlayers[0].seat.cards[1], gamePlayController.game.currentRound.myCards[1]));
				StartCoroutine (TurnCardUp (playingPlayers[0].seat.cards[2], gamePlayController.game.currentRound.myCards[2]));
				StartCoroutine (TurnCardUp (playingPlayers[0].seat.cards[3], gamePlayController.game.currentRound.myCards[3]));

				StartCoroutine (GoToNextState ());
			}
		} 
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
		if (isFirstStartDealUpdate) {
			/*
			float totalWaitTime = dealWaitTimeBetweenPlayer * gamePlayController.game.PlayingPlayers.Count + waitTimeDeltaBetweenCard * gamePlayController.game.PlayerCount * 4;
			Debug.Log ("wait time for give card, waitTime = " + waitTime + ", totalWaitTime = " + totalWaitTime);
			if (waitTime > totalWaitTime)
				isFirstStartDealUpdate = false; */
			yield return new WaitForSeconds (waitTime);
		} else {
			yield return new WaitForSeconds (0);
		}

		card.transform.position = Vector3.MoveTowards(card.gameObject.transform.position, targetCard, step);
		if (player.seat.seatIndex == 0) {
			Vector3 localScale = new Vector3 ();
			localScale.x = user0CardScale;
			localScale.y = user0CardScale;
			card.transform.localScale = localScale;
		}

	
	}



	IEnumerator TurnCardUp(Image card, string cardValue) {
		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("TurnUp");
		yield return new WaitForSeconds (turnUpTime);
		card.sprite = deck.GetCardFaceImage(cardValue);

		anim.Play ("TurnBackNow2");
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

		//更新位置UI
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer ()) {
				seats [i].player.isReady = false;
				seats [i].UpdateUI (gamePlayController.game);
				seats [i].scoreLabel.transform.position = seats [i].originScoreLabelPosition;
			}
		}


		string[] myCards;
		if (cardsDict.ContainsKey (Player.Me.userId)) {
			myCards = cardsDict [Player.Me.userId];
		} else {
			throw new UnityException ("找不到UserId = " + Player.Me.userId + "的牌");
		}

		int[] myBets;
		if (betsDict.ContainsKey (Player.Me.userId)) {
			myBets = betsDict [Player.Me.userId];
		} else {
			throw new UnityException ("找不到UserId = " + Player.Me.userId + "的可下注的筹码量");
		}

		for (int i = 0; i < myCards.Length; i++) {
			gamePlayController.game.currentRound.myCards[i] = myCards[i];
		}

		gamePlayController.game.currentRound.myBets = myBets;

		gamePlayController.game.StartGame ();

		deck.ShowNotDealCardsForFirstDeal (gamePlayController.game.PlayingPlayers.Count);

		//先把数据结构设置好，再在Update()中执行发牌的动画。
		FirstDeal ();

		gamePlayController.state = GameState.FirstDeal;
		gamePlayController.game.UpdateGameInfos ();
	}



}
