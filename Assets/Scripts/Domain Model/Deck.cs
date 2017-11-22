using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Deck
{
	//private int DeckCount = 30;
	
	public Deck (){
	}
	public Sprite cardBack;
	public Vector3 originPosition;
	public List<Image> cards;  
	public Sprite[] cardFaceSprites;

	private int validStart = 0;

	public Image Deal() {
		//Debug.Log ("deal card cardIndex  = " + validStart);

		Image card = cards [validStart];
		card.transform.SetSiblingIndex (validStart);
		validStart++;
		return card;
	}

	public void ShowNotDealCardsForFirstDeal(int playerCount) {
		for (int i = 0; i < playerCount * 4; i++) {
			cards [i].gameObject.SetActive (true);
		}
	}

	public void ShowNotDealCardsForSecondDeal(int playerCount) {
		//Debug.Log ("Show NotDeal Cards For SecondDeal");
		for (int i = playerCount * 4; i < playerCount * 5; i++) {
			cards [i].gameObject.SetActive (true);
		}
	}

	private static Dictionary<string, Sprite> cardDict = new Dictionary<string, Sprite>();
	public Sprite GetCardFaceImage(string cardValue) {
		if (cardDict.ContainsKey (cardValue)) {
			return cardDict [cardValue];
		}

		foreach(Sprite sprite in cardFaceSprites) {
			//Debug.Log (sprite.name);
			if (sprite.name.Contains (cardValue)) {
				cardDict [cardValue] = sprite;
				return sprite;
			}
		}

		Debug.LogError ("can't find sprite for cardValue: " + cardValue);
		return null;
	}

	public void Reset() {
		validStart = 0;

		Image card = null;
		for (int i = 0; i < cards.Count; i++) {
			card = cards [i];

			card.transform.position = originPosition;
			card.sprite = cardBack;
			card.transform.SetSiblingIndex (i);
			card.gameObject.SetActive (false);

			Vector3 localScale = new Vector3 ();
			localScale.x = 1f;
			localScale.y = 1f;
			card.transform.localScale = localScale;

		}
	}
}


