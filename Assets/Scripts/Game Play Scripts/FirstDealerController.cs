using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstDealerController : MonoBehaviour {
	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private GameObject deckCardPosition;

	public static float speed = 400f; //发牌速度
	public static float waitTimeDelta = 0.1f;

	private Vector3[][] userCardPositionsArray;

	private List<Image> deckCards;
	public Sprite[] cardSprites;


	// Use this for initialization
	void Start () {
		hideOtherDeckCard (); 
	}

	// Update is called once per frame
	void Update () {

		if (gamePlayController.state.Equals (GameState.FirstDeal)) {
			float waitTime = 0;

			Seat[] seats = gamePlayController.game.seats;
			//int index = 0;
			int lastSeatIndex = 0;
			int playerIndex = 0;
			for (int i = 0; i < Game.SeatCount; i++) {
				Seat seat = seats [i];
				if (seat.hasPlayer ()) {
					lastSeatIndex = i;
					FirstGiveCards (userCardPositionsArray[i], waitTime, 4 * playerIndex);
					waitTime += 4 * waitTimeDelta;
					playerIndex++;
				}
			}

			int playerCount = gamePlayController.game.PlayerCount;
			//判断最后一张牌是否已经发好
			if (Utils.isTwoPositionIsEqual(deckCards [4 * playerCount - 1].transform.position, userCardPositionsArray [lastSeatIndex][3])) {
				hideOtherDeckCard ();

				StartCoroutine (TurnCardUp (deckCards[0], gamePlayController.game.currentRound.myCards[0]));
				StartCoroutine (TurnCardUp (deckCards[1], gamePlayController.game.currentRound.myCards[1]));
				StartCoroutine (TurnCardUp (deckCards[2], gamePlayController.game.currentRound.myCards[2]));
				StartCoroutine (TurnCardUp (deckCards[3], gamePlayController.game.currentRound.myCards[3]));

				StartCoroutine (GoToNextState ());
			}
		} 

	}

	private void hideOtherDeckCard() {
		foreach (Image card in deckCards) {
			if (Utils.isTwoPositionIsEqual(card.transform.position, deckCardPosition.transform.position))
				card.gameObject.SetActive (false);
		}
	}

	/**
	 * 发4张牌给指定的玩家
	 * */
	private void FirstGiveCards(Vector3[] targetCards, float waitTime, int deckCardStartIndex) {
		float step = speed * Time.deltaTime;
		for (int i = 0; i < 4; i++) {
			Image card = deckCards [deckCardStartIndex + i];
			Vector3 targetCard = targetCards [i];
			StartCoroutine(GiveCard(card, targetCard, step, waitTime));
			waitTime += waitTimeDelta;
		}
	}

	IEnumerator GiveCard(Image card, Vector3 targetCard, float step, float waitTime) {
		yield return new WaitForSeconds (waitTime);
		//Debug.Log ("(x, y): (" + targetCard.x + " , " + targetCard.y + ")");
		card.transform.position = Vector3.MoveTowards(card.gameObject.transform.position, targetCard, step);
	}
		
	public void SetDeckCards(List<Image> cards) {
		this.deckCards = cards;
	}

	public void SetCardSprites(Sprite[] cardSprites) {
		this.cardSprites = cardSprites;
	}

	public void setUserCardsPositionsArray(Vector3[][] positionsArray) {
		this.userCardPositionsArray = positionsArray;
	}

	IEnumerator TurnCardUp(Image card, string cardValue) {
		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("TurnUp");
		yield return new WaitForSeconds (.4f);
		card.sprite = Utils.findCardSprite(cardSprites, cardValue);

		//card.transform.localEulerAngles = new Vector3(0,360,0);
		anim.Play ("TurnBackNow2");
		yield return new WaitForSeconds (.1f);
	}


	IEnumerator GoToNextState() {
		yield return new WaitForSeconds (.4f);
		if (gamePlayController.state == GameState.FirstDeal)
			gamePlayController.goToNextState ();
	}


	/******* 处理服务器的通知***************/
	public void HandleResponse(FirstDealResponse notify) {

		string[] cards = notify.cards;
		//Debug.Log ("cards: " + cards);
		int[] bets = notify.bets;
		//Debug.Log ("bets: " + bets);

		for (int i = 0; i < cards.Length; i++) {
			gamePlayController.game.currentRound.myCards[i] = cards[i];
		}

		gamePlayController.game.currentRound.myBets = bets;

		gamePlayController.state = GameState.FirstDeal;
	}

	public void HandleResponse(StartGameNotify notify) {
		Dictionary<string, string[]> cardsDict = notify.cardsDict;
		//Debug.Log ("cards: " + cards);
		Dictionary<string, int[]> betsDict = notify.betsDict;
		//Debug.Log ("bets: " + bets);

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
		gamePlayController.state = GameState.FirstDeal;
	}

}
