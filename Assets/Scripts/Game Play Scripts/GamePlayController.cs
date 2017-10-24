using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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

public class GamePlayController : MonoBehaviour {

	private float speed = 10f; //发牌速度
	private float waitTimeDelta = 0.15f;

	public GameObject[] user1CardPositions;
	public GameObject[] user2CardPositions;

	private GameObject[] deckCards;

	// Use this for initialization
	void Start () {
		deckCards = GameObject.FindGameObjectsWithTag ("deckCard");

		setUserCardsPosition ();
		Debug.Log ("deckCards.Length = " + deckCards.Length);
	}
	
	// Update is called once per frame
	void Update () {
		float waitTime = 0;
		firstGiveCards (user1CardPositions, waitTime, 0);
		waitTime += 4 * waitTimeDelta;
		firstGiveCards (user2CardPositions, waitTime, 4);
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
	private void firstGiveCards(GameObject[] targetCards, float waitTime, int deckCardStartIndex) {
		float step = speed * Time.deltaTime;
		for (int i = 0; i < 4; i++) {
			GameObject card = deckCards [deckCardStartIndex + i];
			GameObject targetCard = targetCards [i];

			StartCoroutine(giveCard(card, targetCard, step, waitTime));
			waitTime += waitTimeDelta;
		}
	}

	IEnumerator giveCard(GameObject card, GameObject targetCard, float step, float waitTime) {
		yield return new WaitForSeconds (waitTime);
		Debug.Log("card:" + card.transform.position.x);
		Debug.Log ("targetCard:" + targetCard.transform.position.x);
		card.transform.position = Vector3.MoveTowards(card.transform.position, targetCard.transform.position, step);
	}
}
