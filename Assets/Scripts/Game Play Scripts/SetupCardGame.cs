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
	private ChooseBankerController chooseBankerController;

	[SerializeField]
	private CheckCardController checkCardController;

	[SerializeField]
	private BeforeGameStartController beforeGameStartController;

	[SerializeField]
	private RobBankerController robBankerController;

	[SerializeField]
	private BetController betController;

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
	public Image[] emptySeatImages;
	public Image[] isRobImages;
	public Image[] robingImages;
	public Image[] bankerSignPositions;
	public Image[] chipImages;
	public Image[] chipPositionImages;
	public Text[] chipCountLabels;

	private GameObject[][] userCardsPositionsArray;
	private GameObject[][] showCardPositionsArray;

	private int cardCount = 30;

	private List<Image> cards = new List<Image> ();
	private List<Animator> cardAnims = new List<Animator>();



	void Awake() {
		Debug.Log ("SetupCardGame Awake");
		userCardsPositionsArray = new GameObject[6][];
		showCardPositionsArray = new GameObject[6][];

		cardSprites = Resources.LoadAll<Sprite>("sprites/simple");

		GetUserCardsPosition ();
		GetShowCardsPosition ();
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
			seatButtons [i] = buttons [i].GetComponent<Button> ();
		}
		beforeGameStartController.SetSeatButtons (seatButtons);

		GameObject[] descs = GameObject.FindGameObjectsWithTag ("SeatDesc");
		descs = SortUserSeatUIObjects (descs);
		seatDescs = new Text[descs.Length];
		for (int i = 0; i < descs.Length; i++) {
			seatDescs [i] = descs [i].GetComponent<Text> ();
		}
		beforeGameStartController.SetSeatDescs (seatDescs);

		GameObject[] emptyImages = GameObject.FindGameObjectsWithTag ("EmptySeatImage");
		emptyImages = SortUserSeatUIObjects (emptyImages);
		emptySeatImages = new Image[emptyImages.Length];
		for (int i = 0; i < emptyImages.Length; i++) {
			emptySeatImages [i] = emptyImages [i].GetComponent<Image> ();
		}
		beforeGameStartController.SetEmptySeatImages (emptySeatImages);

		GameObject[] isRobObjs = GameObject.FindGameObjectsWithTag ("isRobImage");
		isRobObjs = SortUserSeatUIObjects (isRobObjs);
		isRobImages = new Image[isRobObjs.Length];
		for (int i = 0; i < isRobObjs.Length; i++) {
			isRobImages [i] = isRobObjs [i].GetComponent<Image> ();
		}
		beforeGameStartController.SetIsRobImages (isRobImages);
		robBankerController.SetIsRobImages (isRobImages);
		robBankerController.Reset ();
		chooseBankerController.SetIsRobImages (isRobImages);

		GameObject[] robingObjs = GameObject.FindGameObjectsWithTag ("RobingImage");
		robingObjs = SortUserSeatUIObjects (robingObjs);
		robingImages = new Image[robingObjs.Length];
		for (int i = 0; i < robingObjs.Length; i++) {
			robingImages [i] = robingObjs [i].GetComponent<Image> ();
		}
		chooseBankerController.SetRobingImages (robingImages);
		chooseBankerController.Reset ();

		GameObject[] bankerSignObjs = GameObject.FindGameObjectsWithTag ("BankerSign");
		bankerSignObjs = SortUserSeatUIObjects (bankerSignObjs);
		bankerSignPositions = new Image[bankerSignObjs.Length];
		for (int i = 0; i < bankerSignObjs.Length; i++) {
			bankerSignPositions [i] = bankerSignObjs [i].GetComponent<Image> ();
			bankerSignPositions [i].gameObject.SetActive (false);
		}
		chooseBankerController.SetBankerSignPositions (bankerSignPositions);
		chooseBankerController.Reset ();


		GameObject[] chipObjs = GameObject.FindGameObjectsWithTag ("chip");
		chipObjs = SortUserSeatUIObjects (chipObjs);
		chipImages = new Image[chipObjs.Length];
		for (int i = 0; i < chipObjs.Length; i++) {
			chipImages [i] = chipObjs [i].GetComponent<Image> ();
		}
		betController.SetChipImages (chipImages);


		GameObject[] chipPositionObjs = GameObject.FindGameObjectsWithTag ("chipPosition");
		chipPositionObjs = SortUserSeatUIObjects (chipPositionObjs);
		chipPositionImages = new Image[chipPositionObjs.Length];
		for (int i = 0; i < chipPositionObjs.Length; i++) {
			chipPositionImages [i] = chipPositionObjs [i].GetComponent<Image> ();
		}
		betController.SetChipPositionImages (chipPositionImages);

		GameObject[] chipCountObjs = GameObject.FindGameObjectsWithTag ("chipCountLabel");
		chipCountObjs = SortUserSeatUIObjects (chipCountObjs);
		chipCountLabels = new Text[chipCountObjs.Length];
		for (int i = 0; i < chipCountObjs.Length; i++) {
			chipCountLabels [i] = chipCountObjs [i].GetComponent<Text> ();
		}
		betController.SetChipCountLabels (chipCountLabels);
		betController.Reset ();
	}


	private GameObject[] SortUserSeatUIObjects(GameObject[] objs)  {
		GameObject[] result = new GameObject[objs.Length];
		for (int i = 0; i < objs.Length; i++) {
			string name = objs [i].name;
			//Debug.Log (name);
		}
		for (int i = 0; i < objs.Length; i++) {
			string name = objs [i].name;
			//Debug.Log (name);
			int index = int.Parse (name [name.Length - 1] + "");
			result [index] = objs [i];
		}
		return result;
	}

	private void GetUserCardsPosition() {
		for (int i = 0; i < Game.SeatCount; i++) {
			userCardsPositionsArray[i] = GetUserCardsPosition ("user"+(i + 1)+"CardsPosition");
		}
	
		firstDealerController.setUserCardsPositionsArray (userCardsPositionsArray);
		secondDealController.SetUserCardPositionsArray (userCardsPositionsArray);
	}

	private GameObject[] GetUserCardsPosition(string tag) {
		GameObject[] cards = GameObject.FindGameObjectsWithTag (tag);
		System.Array.Sort (cards, new MyComparer ());
		for (int i = 0; i < cards.Length; i++) {
			cards[i].SetActive(false);
		}
		return cards;

	}

	private void GetShowCardsPosition() {
		for (int i = 0; i < Game.SeatCount; i++) {
			showCardPositionsArray[i] = GetShowCardsPosition ("showCardPosition"+i);
		}

		checkCardController.SetShowCardPositionsArray (showCardPositionsArray);
	}

	private GameObject[] GetShowCardsPosition(string tag) {
		GameObject[] cardObjs = GameObject.FindGameObjectsWithTag (tag);

		Debug.Log (tag + " has " + cardObjs.Length);
		System.Array.Sort (cardObjs, new MyComparer ());
		for (int i = 0; i < cardObjs.Length; i++) {
			cardObjs[i].gameObject.SetActive(false);
		}
		return cardObjs;
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