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
	private FirstDealerController firstDealController;

	[SerializeField]
	private GameObject deckCardPosition;

	private Deck deck;
	private Seat[] seats;

	private float timeLeft;
	private bool isSecondDealing;
	private bool isShowCardBeforeDeal;
	public bool isSecondDealDone;
	//public bool canSecondDeal;

	public override void Reset() {
		isSecondDealing = false;
		//canSecondDeal = false;
		isShowCardBeforeDeal = false;
		isSecondDealDone = false;
		timeLeft = waitTimeBeforeSecondDeal;
	}

	public void Init() {
		seats = gamePlayController.game.seats;
		deck = gamePlayController.game.deck;

		isSecondDealing = false;
		//canSecondDeal = false;
		isShowCardBeforeDeal = false;
		isSecondDealDone = false;
		timeLeft = waitTimeBeforeSecondDeal;
	}
		
	void Update () {
		
		SecondDealAnimation ();
	}

	private void SecondDealAnimation() {
		if (isSecondDealing && firstDealController.isFirstDealDone && betController.IsAllBetCompleted) {
			timeLeft -= Time.deltaTime;

			if (timeLeft > 0) {
				return;
			} 

			if (!isShowCardBeforeDeal) {
				isShowCardBeforeDeal = true;
				deck.ShowNotDealCardsForSecondDeal(gamePlayController.game.PlayingPlayers.Count);
			}

			float waitTime = 0;
			int playerCount = gamePlayController.game.PlayingPlayers.Count;
			List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;
			for (int i = 0; i < playingPlayers.Count; i++) {
				Player player = playingPlayers [i];
				SecondDealCardsAnimation (player, waitTime);

				//每个成员之间加入一个延时
				waitTime += FirstDealerController.dealWaitTimeBetweenPlayerForSecondDeal;
			}

			//判断最后一张牌是否已经发好
			if (Utils.isTwoPositionIsEqual (playingPlayers [playingPlayers.Count - 1].seat.cards [4].transform.position, playingPlayers [playingPlayers.Count - 1].seat.cardPositions [4])) {
				StartCoroutine (GoToNextState ());
				isSecondDealing = false;
				isSecondDealDone = true;
			}
		}
	}

	private void SecondDealCardsAnimation(Player player,  float waitTime) {
		float step = FirstDealerController.dealSpeed * Time.deltaTime;
		Image[] cards = player.seat.cards;
		Vector3[] targetCardPositions = player.seat.cardPositions;

		Vector3 targetCard = targetCardPositions [4];
		StartCoroutine(GiveCardAnimation(player, cards[4], targetCard, step, waitTime));
		waitTime += FirstDealerController.waitTimeDeltaBetweenCard;
	}

	IEnumerator GiveCardAnimation(Player player, Image card, Vector3 targetCard, float step, float waitTime) {
		yield return new WaitForSeconds (waitTime);
		card.transform.position = Vector3.MoveTowards(card.gameObject.transform.position, targetCard, step);
		if (player.seat.seatIndex == 0) {
			Vector3 localScale = new Vector3 ();
			localScale.x = FirstDealerController.user0CardScale;
			localScale.y = FirstDealerController.user0CardScale;
			card.transform.localScale = localScale;
		}
	}


	IEnumerator GoToNextState() {
		yield return new WaitForSeconds (.4f);
		if (gamePlayController.state == GameState.SecondDeal)
			gamePlayController.state = GameState.CheckCard;

	}

	private void SecondDeal() {
		List<Player> players = gamePlayController.game.PlayingPlayers;
		for (int i = 0; i < players.Count; i++) {
			SecondDeal (players[i]);
		}
	}

	private void SecondDeal(Player player) {
		player.seat.cards [4] = deck.Deal ();
	}
		
	public void HandleResponse(GoToSecondDealNotify notify) {
		gamePlayController.game.HideStateLabel ();
		string[] cards = gamePlayController.game.currentRound.myCards;
		cards [4] =  notify.cardsDict[Player.Me.userId];

		SecondDeal ();
		gamePlayController.state = GameState.SecondDeal;

		/*
		if (betController.IsAllBetCompleted) {
			
		} else {
			canSecondDeal = true;
		}*/
		isSecondDealing = true;
	}


}
