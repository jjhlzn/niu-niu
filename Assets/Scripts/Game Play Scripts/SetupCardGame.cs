using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupCardGame : BaseStateController {
	private int MaxMoveChipCount = 8 * 5;

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
	private CompareCardController compareController;

	[SerializeField]
	private Image card;
	[SerializeField]
	private GameObject deckCardPosition;
	[SerializeField]
	private GameObject cardPanel;
	[SerializeField]
	private GameObject userPanel;
	[SerializeField]
	private Text scoreLabel;

	[SerializeField]
	private Image niuImage;
	[SerializeField]
	private Image mutipleImage;


	public Sprite[] cardSprites;

	private float TransformConstant = 71.98f;

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
	public Image[] chipImages;   //下注的筹码
	public Image[] chipPositionImages;
	public Text[] chipCountLabels;
	public Image[] readyImages;
	public Image[] niuImages;  //说明是牛几的图片
	public Image[] multipleImages; //倍数的图片
	public Image[][] chipsArray; //计算结果，所移动的筹码
	public Text[] scoreLabels; //在展示结果时，跳出来的本局输赢分数的展示
	private Vector3[][] userCardsPositionsArray;
	private Vector3[][] showCardPositionsArray;

	private int cardCount = 30;

	private List<Image> cards = new List<Image> ();
	private List<Animator> cardAnims = new List<Animator>();



	void Awake() {
		 //Debug.Log ("SetupCardGame Awake");
		userCardsPositionsArray = new Vector3[6][];
		showCardPositionsArray = new Vector3[6][];

		cardSprites = Resources.LoadAll<Sprite>("sprites/mobile");

		GetUserCardsPosition ();
		GetShowCardsPosition ();
		GetNiuImages ();

		CreateDeckCards ();
		GetUserSeatUI ();
		GetChipsArray ();
		GetScoreLabels ();
	}

	public override void Reset() {
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
		//Debug.Log ("seats = " + seats);
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

		GameObject[] readyObjs = GameObject.FindGameObjectsWithTag ("ReadyImage");
		readyObjs = SortUserSeatUIObjects (readyObjs);
		readyImages = new Image[readyObjs.Length];
		for (int i = 0; i < readyObjs.Length; i++) {
			readyImages [i] = readyObjs [i].GetComponent<Image> ();
			readyImages [i].gameObject.SetActive (false);
		}
		beforeGameStartController.SetReadyImages (readyImages);
		firstDealerController.SetReadyImages (readyImages);

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
			userCardsPositionsArray [i] = GetUserCardsPosition (i);
		}
		firstDealerController.setUserCardsPositionsArray (userCardsPositionsArray);
		secondDealController.SetUserCardPositionsArray (userCardsPositionsArray);
	}

	private Vector3[] GetUserCardsPosition(int index) {
		Vector3[] result = new Vector3[5];
		int initialX = 0, initialY = 0;
		int stepX = 25;
		switch (index) {
		case 0:
			initialX = -240;
			initialY = -277;
			stepX = 100;
			break;
		case 1:
			initialX = -448;
			initialY = -65;
			break;
		case 2:
			initialX = -340;
			initialY = 121;
			break;
		case 3:
			initialX = -20;
			initialY = 170;
			break;
		case 4:
			initialX = 220;
			initialY = 95;
			break;
		case 5:
			initialX = 320;
			initialY = -70;
			break;
		}
		for (int i = 0; i < 5; i++) {
			result[i] = new Vector3 ((initialX + i * stepX) / TransformConstant, initialY / TransformConstant,  0);
		}
		return result;
	}

	private void GetShowCardsPosition() {
		for (int i = 0; i < Game.SeatCount; i++) {
			showCardPositionsArray[i] = GetShowCardsPosition (i);
		}

		checkCardController.SetShowCardPositionsArray (showCardPositionsArray);
	}

	private Vector3[] GetShowCardsPosition(int index) {
		Vector3[] result = new Vector3[5];
		int initialX = 0, initialY = 0;
		int stepX = 30;
		switch (index) {
		case 0:
			initialX = -100;
			initialY = -277;
			break;
		case 1:
			initialX = -448;
			initialY = -65;
			break;
		case 2:
			initialX = -340;
			initialY = 121;
			break;
		case 3:
			initialX = -20;
			initialY = 170;
			break;
		case 4:
			initialX = 220;
			initialY = 95;
			break;
		case 5:
			initialX = 320;
			initialY = -70;
			break;
		}
		for (int i = 0; i < 5; i++) {
			result[i] = new Vector3 ((initialX + i * stepX) / TransformConstant, initialY / TransformConstant,  0);
		}
		return result;
	}

	private void GetNiuImages() {
		this.niuImages = new Image[Game.SeatCount];
		this.multipleImages = new Image[Game.SeatCount];
		for (int i = 0; i < Game.SeatCount; i++) {
			niuImages [i] = GetNiuImage (i);
			niuImages [i].gameObject.SetActive (false);
			multipleImages [i] = GetMutipleImage (i);
			multipleImages [i].gameObject.SetActive (false);
		}
		checkCardController.SetNiuImages (niuImages);
		checkCardController.SetMutipleImages (multipleImages);
	}

	private Image GetNiuImage(int index) {
		int x = 0, y = 0;
		Image image = Instantiate (niuImage);
		switch (index) {
		case 0:
			x = -100;
			y = -277;
			break;
		case 1:
			x = -448;
			y = -65;
			break;
		case 2:
			x = -340;
			y = 121;
			break;
		case 3:
			x = -20;
			y = 170;
			break;
		case 4:
			x = 220;
			y = 95;
			break;
		case 5:
			x = 320;
			y = -70;
			break;
		}
			
		image.gameObject.transform.SetParent (cardPanel.transform);


		Vector3 localScale = new Vector3 ();
		localScale.x = 0.5f;
		localScale.y = 0.5f;
		image.transform.localScale = localScale;

		image.transform.position = new Vector3 ( (x + 20) / TransformConstant, (y - 30) / TransformConstant, 0);
		return image;
	}

	private Image GetMutipleImage(int index) {
		int x = 0, y = 0;
		Image image = Instantiate (mutipleImage);
		switch (index) {
		case 0:
			x = -100;
			y = -277;
			break;
		case 1:
			x = -448;
			y = -65;
			break;
		case 2:
			x = -340;
			y = 121;
			break;
		case 3:
			x = -20;
			y = 170;
			break;
		case 4:
			x = 220;
			y = 95;
			break;
		case 5:
			x = 320;
			y = -70;
			break;
		}

		image.gameObject.transform.SetParent (cardPanel.transform);


		Vector3 localScale = new Vector3 ();
		localScale.x = 0.4f;
		localScale.y = 0.4f;
		image.transform.localScale = localScale;

		image.transform.position = new Vector3 ( (x + 130) / TransformConstant, (y - 30) / TransformConstant, 0);
		return image;
	}


	private void GetChipsArray() {
		chipsArray = new Image[Game.SeatCount][];
		for (int i = 0; i < Game.SeatCount; i++) {
			chipsArray [i] = GetChips (i);
		}	
		compareController.SetChipsArray (chipsArray);
	}

	private Image[] GetChips(int index) {
		Image[] images = new Image[MaxMoveChipCount];
		for (int i = 0; i < MaxMoveChipCount; i++) {
			Image image = Instantiate (chipImages[index]);
			image.name = "user" + index + "chip" + i;
			image.gameObject.transform.SetParent (cardPanel.transform);

			Vector3 localScale = new Vector3 ();
			localScale.x = 0.3f;
			localScale.y = 0.3f;
			image.transform.localScale = localScale;

			image.transform.position = chipImages [index].transform.position;
			images [i] = image;
			image.gameObject.SetActive (false);
		}
		return images;
	}

	private void GetScoreLabels() {
		this.scoreLabels = new Text[Game.SeatCount];
		for (int i = 0; i < scoreLabels.Length; i++) {
			scoreLabels [i] = GetScoreLabel (i);
		}
		compareController.SetScoreLabels (scoreLabels);
	}

	private Text GetScoreLabel(int index) {
		int x = 0, y = 0;
		switch (index) {
		case 0:
			x = -515;
			y = -265;
			break;
		case 1:
			x = -555;
			y = -37;
			break;
		case 2:
			x = -420;
			y = 234;
			break;
		case 3:
			x = 30;
			y = 273;
			break;
		case 4:
			x = 375;
			y = 210;
			break;
		case 5:
			x = 555;
			y = -33;
			break;
		}

		Text text = Instantiate (scoreLabel);
		text.name = "userScoreLabel" + index;
		text.transform.SetParent (userPanel.transform);

		Vector3 localScale = new Vector3 ();
		localScale.x = 1f;
		localScale.y = 1f;
		text.transform.localScale = localScale;

		text.gameObject.SetActive (false);

		text.transform.position = new Vector3 ( x / TransformConstant, y / TransformConstant, 0);
		return text;

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