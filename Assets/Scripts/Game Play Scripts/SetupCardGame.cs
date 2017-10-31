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
	private BeforeGameStartController beforeGameStartController;

	[SerializeField]
	private Image card;
	[SerializeField]
	private GameObject deckCardPosition;
	[SerializeField]
	private GameObject cardPanel;

	public Sprite[] cardSprites;

	public GameObject[] userPanels;
	public Image[] seatImages;
	public Image[] playerImages;
	public Text[] playerNames;
	public Text[] playerScores;
	public Button[] seatButtons;
	public Text[] seatDescs;

	private GameObject[] user1CardsPositions;
	private GameObject[] user2CardsPositions;

	private int cardCount = 10;

	private List<Image> cards = new List<Image> ();
	private List<Animator> cardAnims = new List<Animator>();



	void Awake() {
		Debug.Log ("SetupCardGame Awake");


		cardSprites = Resources.LoadAll<Sprite>("sprites/simple");

		GetUserCardsPosition ();
		CreateDeckCards ();
		GetUserSeatUI ();
	}

	// Update is called once per frame
	void Update () {
		
	}
		
	private void GetUserSeatUI() {
		userPanels = GameObject.FindGameObjectsWithTag ("UserPanel");
		userPanels = SortUserSeatUIObjects (userPanels);
		beforeGameStartController.SetUserPanels (userPanels);

		GameObject[] seats = GameObject.FindGameObjectsWithTag ("UserSeat");
		seats = SortUserSeatUIObjects (seats);
		Debug.Log ("seats = " + seats);
		seatImages = new Image[seats.Length];
		for (int i = 0; i < seats.Length; i++) {
			seatImages [i] = seats [i].GetComponent<Image> ();
		}
		beforeGameStartController.SetSeatImages (seatImages);

		GameObject[] images = GameObject.FindGameObjectsWithTag ("UserImage");
		images = SortUserSeatUIObjects (images);
		playerImages = new Image[images.Length];
		for (int i = 0; i < images.Length; i++) {
			playerImages [i] = images [i].GetComponent<Image> ();
		}
		beforeGameStartController.SetPlayerImages (playerImages);

		GameObject[] names = GameObject.FindGameObjectsWithTag ("UserName");
		names = SortUserSeatUIObjects (names);
		playerNames = new Text[names.Length];
		for (int i = 0; i < names.Length; i++) {
			playerNames [i] = names [i].GetComponent<Text> ();
		}
		beforeGameStartController.SetPlayerNames (playerNames);

		GameObject[] scores = GameObject.FindGameObjectsWithTag ("UserScore");
		scores = SortUserSeatUIObjects (scores);
		playerScores = new Text[scores.Length];
		for (int i = 0; i < scores.Length; i++) {
			playerScores [i] = scores [i].GetComponent<Text> ();
		}
		beforeGameStartController.SetPlayerScores (playerScores);

		GameObject[] buttons = GameObject.FindGameObjectsWithTag ("SeatButton");
		buttons = SortUserSeatUIObjects (buttons);
		seatButtons = new Button[buttons.Length];
		for (int i = 0; i < buttons.Length; i++) {
			seatButtons[i] = buttons[i].GetComponent<Button>();
		}
		beforeGameStartController.SetSeatButtons (seatButtons);

		GameObject[] descs = GameObject.FindGameObjectsWithTag ("SeatDesc");
		descs = SortUserSeatUIObjects (descs);
		seatDescs = new Text[descs.Length];
		for (int i = 0; i < descs.Length; i++) {
			seatDescs [i] = descs [i].GetComponent<Text> ();
		}
		beforeGameStartController.SetSeatDescs (seatDescs);
	}



	private GameObject[] SortUserSeatUIObjects(GameObject[] objs)  {
		GameObject[] result = new GameObject[objs.Length];
		for (int i = 0; i < objs.Length; i++) {
			string name = objs [i].name;
			//Debug.Log (name);
		}
		for (int i = 0; i < objs.Length; i++) {
			string name = objs [i].name;

			int index = int.Parse (name [name.Length - 1] + "");
			result [index] = objs [i];
		}
		return result;
	}

	private void GetUserCardsPosition() {
		user1CardsPositions = GetUserCardsPosition ("user1CardsPosition");
		user2CardsPositions = GetUserCardsPosition ("user2CardsPosition");

		firstDealerController.setUser1CardsPositions (user1CardsPositions);
		firstDealerController.setUser2CardsPositions (user2CardsPositions);

		secondDealController.SetUser1CardPositions (user1CardsPositions);
		secondDealController.SetUser2CardPositions (user2CardsPositions);
	}

	private GameObject[] GetUserCardsPosition(string tag) {
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
