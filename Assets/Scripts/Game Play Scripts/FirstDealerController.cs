using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyComparer : IComparer<GameObject> {
	public int Compare(GameObject x1, GameObject y1)  
	{
		if (x1.transform.position.x > y1.transform.position.x) {
			return 1;
		} else if (x1.transform.position.x < y1.transform.position.x) {
			return -1;
		} else {
			return 0;
		} 
	}
}

public class FirstDealerController : MonoBehaviour {
	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private SetupCardGame setupCardGame;

	[SerializeField]
	private GameObject deckCardPosition;

	private float speed = 400f; //发牌速度
	private float waitTimeDelta = 0.1f;

	public GameObject[] user1CardPositions;
	public GameObject[] user2CardPositions;

	private List<Image> deckCards;


	// Use this for initialization
	void Start () {
		setUserCardsPosition ();
		hideOtherDeckCard (); 
	}


	private bool isTwoPositionIsEqual(Vector3 v1, Vector3 v2) {
		float deltaX = Mathf.Abs(v1.x - v2.x);
		float deltaY = Mathf.Abs(v1.y - v2.y);
		//Debug.Log ("deltaX = " + deltaX + ", deltaY = " + deltaY);
		return deltaX < .5f && deltaY < .5f;
	}

	private void hideOtherDeckCard() {
		//Debug.Log ("hideOtherDeckCard called");
		foreach (Image card in deckCards) {
			//Debug.Log ("deckCardPosition: " +  deckCardPosition.transform.position.x + ", copy  position: " + card.transform.position.x);
			if (isTwoPositionIsEqual(card.transform.position, deckCardPosition.transform.position))
				card.gameObject.SetActive (false);
		}
	}

	// Update is called once per frame
	void Update () {

		if (gamePlayController.state.Equals (GameState.FirstDeal)) {
			float waitTime = 0;
			FirstGiveCards (user1CardPositions, waitTime, 0);
			waitTime += 4 * waitTimeDelta;
			FirstGiveCards (user2CardPositions, waitTime, 4);

			if (isTwoPositionIsEqual(deckCards [4 * 2 - 1].transform.position, user2CardPositions [3].transform.position)) {
				Debug.Log("first deal card over");
				hideOtherDeckCard ();
				StartCoroutine (TurnCardUp (deckCards[0]));
				gamePlayController.state = gamePlayController.state.nextState ();
			}
		} 

	}

	private void setUserCardsPosition() {
		user1CardPositions = getUserCardsPosition ("user1CardsPosition");
		user2CardPositions = getUserCardsPosition ("user2CardsPosition");
	}

	private GameObject[] getUserCardsPosition(string tag) {
		GameObject[] cards = GameObject.FindGameObjectsWithTag (tag);
		System.Array.Sort (cards, new MyComparer ());
		for (int i = 0; i < cards.Length; i++) {
			cards[i].SetActive(false);
		}
		return cards;

	}

	/**
	 * 第一次发票给某个玩家
	 * */
	private void FirstGiveCards(GameObject[] targetCards, float waitTime, int deckCardStartIndex) {
		float step = speed * Time.deltaTime;
		for (int i = 0; i < 4; i++) {
			Image card = deckCards [deckCardStartIndex + i];
			GameObject targetCard = targetCards [i];

			StartCoroutine(GiveCard(card, targetCard, step, waitTime));
			waitTime += waitTimeDelta;
		}
	}

	IEnumerator GiveCard(Image card, GameObject targetCard, float step, float waitTime) {
		yield return new WaitForSeconds (waitTime);
		card.transform.position = Vector3.MoveTowards(card.gameObject.transform.position, targetCard.transform.position, step);
	}
		
	public void SetDeckCards(List<Image> cards) {
		this.deckCards = cards;
	}

	IEnumerator TurnCardUp(Image card) {
		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("TurnUp");
		yield return new WaitForSeconds (.4f);
		//anim.sprite = puzzleImage;
	}

}
