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
	private GameObject deckCardPosition;

	public static float speed = 400f; //发牌速度
	public static float waitTimeDelta = 0.1f;

	private GameObject[] user1CardPositions;
	private GameObject[] user2CardPositions;

	private List<Image> deckCards;
	public Sprite[] cardSprites;


	// Use this for initialization
	void Start () {
		hideOtherDeckCard (); 
	}


	public static bool isTwoPositionIsEqual(Vector3 v1, Vector3 v2) {
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
				//Debug.Log("first deal card over");
				hideOtherDeckCard ();

				StartCoroutine (TurnCardUp (deckCards[0]));
				StartCoroutine (TurnCardUp (deckCards[1]));
				StartCoroutine (TurnCardUp (deckCards[2]));
				StartCoroutine (TurnCardUp (deckCards[3]));

				StartCoroutine (GoToNextState ());
			}
		} 

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

	public void SetCardSprites(Sprite[] cardSprites) {
		this.cardSprites = cardSprites;
	}

	public void setUser1CardsPositions(GameObject[] user1Positions) {
		this.user1CardPositions = user1Positions;
	}

	public void setUser2CardsPositions(GameObject[] user2Positions) {
		this.user2CardPositions = user2Positions;
	}

	IEnumerator TurnCardUp(Image card) {
		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("TurnUp");
		yield return new WaitForSeconds (.4f);
		card.sprite = cardSprites[0];


		//card.transform.localEulerAngles = new Vector3(0,360,0);
		anim.Play ("TurnBackNow2");
		yield return new WaitForSeconds (.1f);

	}

	IEnumerator GoToNextState() {
		
		yield return new WaitForSeconds (.4f);
		if (gamePlayController.state == GameState.FirstDeal)
			gamePlayController.goToNextState ();

	}

}
