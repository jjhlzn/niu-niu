using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondDealController : BaseStateController {
	private float waitTimeBeforeSecondDeal = 0.5f;

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private BetController betController;

	[SerializeField]
	private GameObject deckCardPosition;

	private Deck deck;
	private Seat[] seats;

	private float timeLeft;
	private bool dealing;
	public bool canSecondDeal;

	// Use this for initialization
	void Start () {
		dealing = false;
		timeLeft = waitTimeBeforeSecondDeal;
	}

	public override void Reset() {
		dealing = false;
		canSecondDeal = false;
		timeLeft = waitTimeBeforeSecondDeal;
	}

	public void Init() {
		seats = gamePlayController.game.seats;
		deck = gamePlayController.game.deck;
	}

	// Update is called once per frame
	void Update () {

		if (gamePlayController.state.Equals (GameState.SecondDeal)) {
			timeLeft -= Time.deltaTime;

			if (timeLeft > 0) {
				return;
			}
			
			if (!dealing) {
				dealing = true;
				deck.ShowNotDealCardsForSecondDeal(gamePlayController.game.PlayingPlayers.Count);
			}
				
			float waitTime = 0;
			int playerCount = gamePlayController.game.PlayingPlayers.Count;
			List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;
			for (int i = 0; i < playingPlayers.Count; i++) {
				Player player = playingPlayers [i];
				SecondDealCardsAnimation (player, waitTime);

				//每个成员之间加入一个延时
				waitTime += FirstDealerController.dealWaitTimeBetweenPlayer;
			}
				
			//判断最后一张牌是否已经发好
			//Debug.Log("playingPlayers[playingPlayers.Count - 1].seat.cards.lenth = " + playingPlayers[playingPlayers.Count - 1].seat.cards.Length);
			//Debug.Log("playingPlayers[playingPlayers.Count - 1].seat.cardPositions.lenth = " + playingPlayers[playingPlayers.Count - 1].seat.cardPositions.Length);
			if (Utils.isTwoPositionIsEqual(playingPlayers[playingPlayers.Count - 1].seat.cards[4].transform.position, playingPlayers[playingPlayers.Count - 1].seat.cardPositions[4])) {
				StartCoroutine(GoToNextState());
			}
		} 
	}

	private void SecondDealCardsAnimation(Player player,  float waitTime) {
		float step = FirstDealerController.dealSpeed * Time.deltaTime;
		Image[] cards = player.seat.cards;
		Vector3[] targetCardPositions = player.seat.cardPositions;

		Vector3 targetCard = targetCardPositions [4];
		StartCoroutine(GiveCardAnimation(cards[4], targetCard, step, waitTime));
		waitTime += FirstDealerController.waitTimeDeltaBetweenCard;
	}

	IEnumerator GiveCardAnimation(Image card, Vector3 targetCard, float step, float waitTime) {
		yield return new WaitForSeconds (waitTime);
		card.transform.position = Vector3.MoveTowards(card.gameObject.transform.position, targetCard, step);
	}


	IEnumerator GoToNextState() {

		yield return new WaitForSeconds (.4f);
		if (gamePlayController.state == GameState.SecondDeal)
			gamePlayController.goToNextState ();

	}

	private void SecondDeal() {
		List<Player> players = gamePlayController.game.PlayingPlayers;

		for (int i = 0; i < players.Count; i++) {
			//Debug.Log ("Second Deal to player: " + players [i].userId);
			SecondDeal (players[i]);
		}
	}

	private void SecondDeal(Player player) {
		player.seat.cards [4] = deck.Deal ();
	}
		
	public void HandleResponse(GoToSecondDealNotify notify) {
		string[] cards = gamePlayController.game.currentRound.myCards;
		cards [4] =  notify.cardsDict[Player.Me.userId];

		SecondDeal ();

		if (betController.IsAllBetCompleted) {
			gamePlayController.state = GameState.SecondDeal;
		} else {
			canSecondDeal = true;
		}
	}


}
