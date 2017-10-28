using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupCardGame : MonoBehaviour {
	[SerializeField]
	private FirstDealerController firstDealerController;

	[SerializeField]
	private SecondDealController secondDealController;

	[SerializeField]
	private CheckCardController checkCardController;

	[SerializeField]
	private Image card;
	[SerializeField]
	private GameObject deckCardPosition;
	[SerializeField]
	private GameObject cardPanel;

	public Sprite[] cardSprites;

	private GameObject[] user1CardsPositions;
	private GameObject[] user2CardsPositions;

	private int cardCount = 10;

	private List<Image> cards = new List<Image> ();
	private List<Animator> cardAnims = new List<Animator>();



	void Awake() {

		Debug.Log ("SetupCardGame Awake");


		cardSprites = Resources.LoadAll<Sprite>("sprites/simple");

		SetUserCardsPosition ();
		CreateDeckCards ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void SetUserCardsPosition() {
		user1CardsPositions = SetUserCardsPosition ("user1CardsPosition");
		user2CardsPositions = SetUserCardsPosition ("user2CardsPosition");

		firstDealerController.setUser1CardsPositions (user1CardsPositions);
		firstDealerController.setUser2CardsPositions (user2CardsPositions);

		secondDealController.SetUser1CardPositions (user1CardsPositions);
		secondDealController.SetUser2CardPositions (user2CardsPositions);
	}

	private GameObject[] SetUserCardsPosition(string tag) {
		GameObject[] cards = GameObject.FindGameObjectsWithTag (tag);
		System.Array.Sort (cards, new MyComparer ());
		for (int i = 0; i < cards.Length; i++) {
			cards[i].SetActive(false);
		}
		return cards;

	}

	void CreateDeckCards() {

		cards = new List<Image> ();
		cardAnims = new List<Animator> ();

		for (int i = 0; i < cardCount; i++) {
			Image temp = Instantiate (card);

			temp.gameObject.transform.SetParent (cardPanel.transform);


			Vector3 localScale = new Vector3 ();
			localScale.x = 1;
			localScale.y = 1;
			temp.transform.localScale = localScale;

			temp.gameObject.transform.position = deckCardPosition.transform.position;

			//Debug.Log ("deckCardPosition: " +  deckCardPosition.transform.position.x + ", copy  position: " + temp.transform.position.x);

			temp.gameObject.name = "" + i;
			cards.Add (temp);

			cardAnims.Add (temp.gameObject.GetComponent<Animator> ());

			temp.gameObject.SetActive (true);
		}

		firstDealerController.SetDeckCards (cards);
		secondDealController.SetDeckCards (cards);
		checkCardController.SetDeckCards (cards);

		firstDealerController.SetCardSprites (cardSprites);
		secondDealController.SetCardSprites (cardSprites);
		checkCardController.SetCardSprites (cardSprites);

	}

	public void resetCards() {
		foreach (Image card in cards) {
			card.gameObject.transform.position = deckCardPosition.transform.position;
			card.gameObject.SetActive (true);
		}
	}
}
