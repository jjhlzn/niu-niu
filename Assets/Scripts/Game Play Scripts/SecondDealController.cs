using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondDealController : MonoBehaviour {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private GameObject deckCardPosition;


	private GameObject[] user1CardPositions;
	private GameObject[] user2CardPositions;

	private List<Image> deckCards;
	public Sprite[] cardSprites;

	private bool dealing;

	// Use this for initialization
	void Start () {
		dealing = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (gamePlayController.state.Equals (GameState.SecondDeal)) {
			if (!dealing) {
				ShowOtherDeckCards ();
				dealing = true;
			}

			float waitTime = 0;
			FirstDealCards (user1CardPositions, waitTime, 8);
			waitTime += FirstDealerController.waitTimeDelta;
			FirstDealCards (user2CardPositions, waitTime, 9);

			if (FirstDealerController.isTwoPositionIsEqual(deckCards [4 * 2 - 1 + 2].transform.position, 
				user2CardPositions [4].transform.position)) {
				dealing = false;
				//Debug.Log("first deal card over");
				HideOtherDeckCards ();

				if (gamePlayController.state == GameState.SecondDeal)
					gamePlayController.goToNextState ();
			}
		} 
	}

	private void ShowOtherDeckCards() {
		for (int i = 8; i < deckCards.Count; i++) {
			deckCards[i].gameObject.SetActive (true);
		}
	}

	private void HideOtherDeckCards() {
		//Debug.Log ("hideOtherDeckCard called");
		foreach (Image card in deckCards) {
			//Debug.Log ("deckCardPosition: " +  deckCardPosition.transform.position.x + ", copy  position: " + card.transform.position.x);
			if (FirstDealerController.isTwoPositionIsEqual(card.transform.position, deckCardPosition.transform.position))
				card.gameObject.SetActive (false);
		}
	}
		
	private void FirstDealCards(GameObject[] targetCards, float waitTime, int deckCardStartIndex) {
		float step = FirstDealerController.speed * Time.deltaTime;

		Image card = deckCards [deckCardStartIndex];
		GameObject targetCard = targetCards [4];

		StartCoroutine(GiveCard(card, targetCard, step, waitTime));
		waitTime += FirstDealerController.waitTimeDelta;

	}

	IEnumerator GiveCard(Image card, GameObject targetCard, float step, float waitTime) {
		yield return new WaitForSeconds (waitTime);
		card.transform.position = Vector3.MoveTowards(card.gameObject.transform.position, targetCard.transform.position, step);
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

	public void SetUser1CardPositions(GameObject[] user1CardPositions) {
		this.user1CardPositions = user1CardPositions;
	}	

	public void SetUser2CardPositions(GameObject[] user2CardPositions) {
		this.user2CardPositions = user2CardPositions;
	}


}
