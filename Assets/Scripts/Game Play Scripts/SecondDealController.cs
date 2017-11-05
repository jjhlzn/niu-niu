using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondDealController : BaseStateController {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private BetController betController;

	[SerializeField]
	private GameObject deckCardPosition;

	public static float waitTimeDelta = 0.1f;
	//private GameObject[] user1CardPositions;
	//private GameObject[] user2CardPositions;
	private Vector3[][] userCardPositionsArray;

	private List<Image> deckCards;
	public Sprite[] cardSprites;
	private float secondDealSpeed = 300f;

	private bool dealing;

	public bool canSecondDeal;

	// Use this for initialization
	void Start () {
		dealing = false;
	}

	public override void Reset() {
		dealing = false;
		canSecondDeal = false;
	}

	// Update is called once per frame
	void Update () {
		if (gamePlayController == null) {
			Debug.LogError ("gamePlayController become to null");
		}

		if (gamePlayController.state.Equals (GameState.SecondDeal)) {
			
			if (!dealing) {
				ShowOtherDeckCards ();
				dealing = true;
			}

			/*
			float waitTime = 0;
			FirstDealCards (user1CardPositions, waitTime, 8);
			waitTime += FirstDealerController.waitTimeDelta;
			FirstDealCards (user2CardPositions, waitTime, 9);

			if (Utils.isTwoPositionIsEqual(deckCards [4 * 2 - 1 + 2].transform.position, 
				user2CardPositions [4].transform.position)) {
				dealing = false;
				//Debug.Log("first deal card over");
				HideOtherDeckCards ();

				if (gamePlayController.state == GameState.SecondDeal)
					gamePlayController.goToNextState ();
			} */


			float waitTime = 0;

			Seat[] seats = gamePlayController.game.seats;
			int playerCount = gamePlayController.game.PlayerCount;
			//int index = 0;
			int lastSeatIndex = 0;
			//int playerIndex = 0;
			int playerIndex = 0;
			for (int i = 0; i < Game.SeatCount; i++) {
				Seat seat = seats [i];
				if (seat.hasPlayer ()) {
					lastSeatIndex = i;
					SecondDealCards (userCardPositionsArray[i], waitTime, 4 * playerCount + playerIndex);
					waitTime += 1 * waitTimeDelta;
					playerIndex++;
					//playerIndex++;
				}
			}


			//判断最后一张牌是否已经发好
			Vector3 lastCardPosition = deckCards [5 * playerCount - 1].gameObject.transform.position;
			Vector3 lastUserLastCardPosition = userCardPositionsArray [lastSeatIndex] [4];
			//Debug .Log("(x, y): " + lastCardPosition.x + "," + lastCardPosition.y + "     (x1, y1): " + lastUserLastCardPosition.x + "," + lastUserLastCardPosition.y);
			if (Utils.isTwoPositionIsEqual(deckCards [5 * playerCount - 1].gameObject.transform.position, userCardPositionsArray [lastSeatIndex][4])) {
				HideOtherDeckCards ();

				dealing = false;
			
				gamePlayController.goToNextState (); 
			}
		} 
	}

	private void ShowOtherDeckCards() {
		for (int i = 0; i < deckCards.Count; i++) {
			deckCards[i].gameObject.SetActive (true);
		}
	}

	private void HideOtherDeckCards() {
		//Debug.Log ("hideOtherDeckCard called");
		foreach (Image card in deckCards) {
			//Debug.Log ("deckCardPosition: " +  deckCardPosition.transform.position.x + ", copy  position: " + card.transform.position.x);
			if (Utils.isTwoPositionIsEqual(card.transform.position, deckCardPosition.transform.position))
				card.gameObject.SetActive (false);
		}
	}
		
	private void SecondDealCards(Vector3[] targetCards, float waitTime, int deckCardStartIndex) {
		float step = secondDealSpeed * Time.deltaTime;

		Image card = deckCards [deckCardStartIndex];
		Vector3 targetCard = targetCards [4];

		StartCoroutine(GiveCard(card, targetCard, step, waitTime));
		waitTime += FirstDealerController.waitTimeDelta;

	}

	IEnumerator GiveCard(Image card, Vector3 targetCard, float step, float waitTime) {
		yield return new WaitForSeconds (waitTime);
		card.transform.position = Vector3.MoveTowards(card.gameObject.transform.position, targetCard, step);
	}


	IEnumerator GoToNextState() {

		yield return new WaitForSeconds (.4f);
		if (gamePlayController.state == GameState.SecondDeal)
			gamePlayController.goToNextState ();

	}

	public void SetDeckCards(List<Image> deckCards) {
		this.deckCards = deckCards;
	}	

	public void SetCardSprites(Sprite[] cardSprites) {
		this.cardSprites = cardSprites;
	}

	public void SetUserCardPositionsArray(Vector3[][] userCardPositionsArray) {
		this.userCardPositionsArray = userCardPositionsArray;
	}


	public void HandleResponse(GoToSecondDealNotify notify) {
		string[] cards = gamePlayController.game.currentRound.myCards;
		cards [4] =  notify.cardsDict[Player.Me.userId];

		if (betController.IsAllBetCompleted) {
			gamePlayController.state = GameState.SecondDeal;
		} else {
			canSecondDeal = true;
		}
	}


}
