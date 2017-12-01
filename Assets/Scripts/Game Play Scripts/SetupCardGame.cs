using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupCardGame : BaseStateController {
	private int MaxMoveChipCount = 8 * 6;
	public static float TransformConstant = 71.98f;
	private int cardCount = 30; //生成多少张牌的图片，6 * 5 = 30
	public static float ChipScale = 0.6f;

	[SerializeField]
	private CheckCardController checkCardController;

	[SerializeField]
	private GameObject seatUI;
	[SerializeField]
	private GameObject seatUI1;
	[SerializeField]
	private GameObject seatUI2;
	[SerializeField]
	private Image card;
	[SerializeField]
	private GameObject deckCardPosition;
	[SerializeField]
	private GameObject cardPanel;
	[SerializeField]
	private GameObject userPanel;
	[SerializeField]
	private GameObject cardsGameObject;
	[SerializeField]
	private Text scoreLabel;
	[SerializeField]
	private Image niuImage;
	[SerializeField]
	private Image mutipleImage;

	[SerializeField]
	public Image bankerSignImage;
	[SerializeField]
	public Text gameStateLabel;
	[SerializeField]
	public Image gameStateLabelBackground;

	[SerializeField]
	public Image bankerSign;

	[SerializeField]
	private Button betButton;
	[SerializeField]
	private Text betLabel;

	[SerializeField]
	private Button menuButton;
	[SerializeField]
	private GameObject menuPanel;

	[SerializeField]
	private GameObject connectingPanel;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GameObject scorePanel;
	[SerializeField]
	private GameObject eachScorePanel;


	public Seat[] seats;
	public Deck deck;
	public Sprite[] niuSprites;
	public Sprite[] multipleSprites;
	public Button[] betButtons;
	public Text[] betLabels;
	public Vector3[] betButtonPositionsFo3Button;
	public Vector3[] betButtonPositionsFor4Button;
	public Vector3[] betLabelPositionsFo3Button;
	public Vector3[] betLabelPositionsFor4Button;

	[SerializeField]
	private GameOverPanel gameOverPanel;

	void Awake() {
		CreateDeck ();
		SeatSeatUIs ();
		SetOtherSeatUIs ();
		LoadNiuAndMultipleImages ();
		CreateBetButtons ();
		Create4BetsButtonPositions ();
		Create3BetsButtonPositions ();



		SetMenu ();
		SetGameOverPanel ();
		//把移动的庄家放在最上面一层
		bankerSign.transform.SetParent(userPanel.transform);
		bankerSign.transform.SetAsLastSibling ();

		menuButton.transform.SetParent (canvas.transform);
		menuButton.transform.SetAsLastSibling ();


	}

	private void SetGameOverPanel() {
		//gameOverPanel = new GameOverPanel ();
		gameOverPanel.Setup (scorePanel, eachScorePanel, userPanel);
	}

	public override GamePlayController GetGamePlayController ()
	{
		return null;
	}


	void Start() {
	}

	public override void Reset() {
	}

	private void SetMenu() {
		menuPanel.gameObject.SetActive (false);
	}

	public void MenuClick() {
		menuButton.gameObject.SetActive (false);
		menuPanel.gameObject.SetActive (true);
	}

	public void CloseMenuClick() {
		menuButton.gameObject.SetActive (true);
		menuPanel.gameObject.SetActive (false);
	}
		
	private void SetOtherSeatUIs() {

		//仅仅为了隐藏界面上的位置示意
		GameObject[] userPanels = GameObject.FindGameObjectsWithTag ("RobingImage");
		foreach (GameObject obj in userPanels) {
			obj.SetActive (false);
		}

		//展示玩家抢庄、不抢庄的图片
		GameObject[] isRobObjs = GameObject.FindGameObjectsWithTag ("isRobImage");
		isRobObjs = SortUserSeatUIObjects (isRobObjs);
		for (int i = 0; i < isRobObjs.Length; i++) {
			seats [i].isRobImage = isRobObjs [i].GetComponent<Image> ();
			seats [i].isRobImage.gameObject.SetActive (false);
		} 
			
		//玩家下注的时候，筹码移动到的最终位置
		GameObject[] chipPositionObjs = GameObject.FindGameObjectsWithTag ("chipPosition");
		chipPositionObjs = SortUserSeatUIObjects (chipPositionObjs);
		for (int i = 0; i < chipPositionObjs.Length; i++) {
			Image positionImage = chipPositionObjs [i].GetComponent<Image> ();
			positionImage.gameObject.SetActive (false);
			seats [i].chipPositionWhenBet = positionImage.transform.position;
		}

		//展示玩家下注的时候的文本，例如4
		GameObject[] chipCountObjs = GameObject.FindGameObjectsWithTag ("chipCountLabel");
		chipCountObjs = SortUserSeatUIObjects (chipCountObjs);
		for (int i = 0; i < chipCountObjs.Length; i++) {
			seats [i].chipCountLabel = chipCountObjs [i].GetComponent<Text> ();
			seats [i].chipCountLabel.gameObject.SetActive (false);
		}

		//展示文本的背景图片
		GameObject[] chipLabelBackgroundObjs = GameObject.FindGameObjectsWithTag ("chipLabelBackground");
		chipLabelBackgroundObjs = SortUserSeatUIObjects (chipLabelBackgroundObjs);
		for (int i = 0; i < chipLabelBackgroundObjs.Length; i++) {
			seats [i].chipLabelBackground = chipLabelBackgroundObjs [i].GetComponent<Image> ();
			seats [i].chipLabelBackground.gameObject.SetActive (false);
		}

		SetChips ();
		SetNiuImages ();
		SetPlayerCardPositions ();
		SetShowCardPositions ();
	}

	/**
	 * 设置座位上各种UI元素，例如玩家的图片，玩家的ID等
	 * */
	private void SeatSeatUIs() {
		seats = new Seat[Game.SeatCount];
		string seatNos = "ABCDEF";
		for(int i = 0 ; i < Game.SeatCount; i++) {
			seats [i] = new Seat ();
			seats [i].seatNo = seatNos[i] + "";
			SetSeatUI (i, seats[i]);
		}
	}

	private void SetSeatUI(int index, Seat seat) {
		GameObject seatUICopy = null;
		if (index == 1) {
			seatUICopy = Instantiate (seatUI1);
		} else if (index == 5) {
			seatUICopy = Instantiate (seatUI2);
		} else {
			seatUICopy = Instantiate (seatUI);
		}
			
		seatUICopy.name = "PlayerSeat" + index;
		seatUICopy.transform.SetParent (userPanel.transform);

	
		int x = 0, y = 0;
		switch (index) {
		case 0:
			x = -470;
			y = -258;
			break;
		case 1:
			x = -482;
			y = -24;
			break;
		case 2:
			x = -380;
			y = 230;
			break;
		case 3:
			x = -16;
			y = 290;
			break;
		case 4:
			x = 396;
			y = 219;
			break;
		case 5:
			x = 493;
			y = -34;
			break;
		}

		Vector3 localScale = new Vector3 ();
		localScale.x = 0.7f;
		localScale.y = 0.7f;
		seatUICopy.transform.localScale = localScale;
		seatUICopy.transform.position = new Vector3 ( x / TransformConstant, y / TransformConstant, 0);

		Image[] childrenImages = seatUICopy.GetComponentsInChildren<Image>();
		foreach (Image image in childrenImages) {
			switch (image.name) {
			case "Seat Border Image":
				seat.seatBorderImage = image;
				break;
			case "Robing Seat Border Image":
				seat.robingSeatBorderImage = image;
				break;
			case "Player Image":
				seat.playerImage = image;
				break;
			case "Empty Seat Image":
				seat.emptySeatImage = image;
				break;
			case "Banker Sign Image":
				image.gameObject.SetActive (false);
				seat.bankerSignPosition = image.transform.position;
				break;
			case "Ready Image":
				seat.readyImage = image;
				break;
			case "Leave Image Sign":
				seat.leaveImage = image;
				break;
			case "Chip Image":
				image.transform.SetParent (userPanel.transform);
				image.transform.localScale = new Vector3 (ChipScale, ChipScale);
				image.gameObject.SetActive (false);
				seat.chipImageForBet = image;
				seat.originChipImagePositionForBet = image.transform.position;
				break;
			}
		}

		Text[] childrenTexts = seatUICopy.GetComponentsInChildren<Text> ();
		foreach (Text text in childrenTexts) {
			switch (text.name) {
			case "Player Name Label":
				seat.playerNameLabel = text;
				text.fontSize = 32;
				break;
			case "Player Score Label":
				seat.playerScoreLabel = text;
				text.fontSize = 32;
				break;
			case "Seat Number Label":
				seat.seatNumberLabel = text;
				break;
			case "Score Label":
				seat.scoreLabel = text;
				seat.originScoreLabelPosition = seat.scoreLabel.transform.position;
				if (index == 3) {
					seat.targetScoreLabelPosition = new Vector3 (seat.originScoreLabelPosition.x, seat.originScoreLabelPosition.y + 0.8f, 0);
				} else {
					seat.targetScoreLabelPosition = new Vector3 (seat.originScoreLabelPosition.x, seat.originScoreLabelPosition.y + 1.1f, 0);
				}
				break;
			}
		}

		Button button = seatUICopy.GetComponentInChildren<Button> ();
		button.name = button.name + index;
		seat.sitdownButton = button;

		seat.playerPanel = seatUICopy;
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


	/**
	 * 设置座位上发牌发到的位置
	 * */
	private void SetPlayerCardPositions() {
		for (int i = 0; i < Game.SeatCount; i++) {
			seats [i].cardPositions = SetPlayerCardPositions (i);
		}
	}

	private Vector3[] SetPlayerCardPositions(int index) {
		Vector3[] result = new Vector3[5];
		int initialX = 0, initialY = 0;
		int stepX = 25;
		switch (index) {
		case 0:
			initialX = -230;
			initialY = -260;
			stepX = 112;
			break;
		case 1:
			initialX = -408;
			initialY = -55;
			break;
		case 2:
			initialX = -290;
			initialY = 110;
			break;
		case 3:
			initialX = -50;
			initialY = 176;
			break;
		case 4:
			initialX = 210;
			initialY = 95;
			break;
		case 5:
			initialX = 330;
			initialY = -70;
			break;
		}
		for (int i = 0; i < 5; i++) {
			result[i] = new Vector3 ((initialX + i * stepX) / TransformConstant, initialY / TransformConstant,  0);
		}
		return result;
	}

	/**
	 * 设置座位上，最后展示牌的位置 （正面朝上的时候）
	 * */
	private void SetShowCardPositions() {
		for (int i = 0; i < Game.SeatCount; i++) {
			seats [i].showCardPositions = SetShowCardPositions (i);
		}
	}

	private Vector3[] SetShowCardPositions(int index) {
		Vector3[] result = new Vector3[5];
		int initialX = 0, initialY = 0;
		int stepX = 30;
		switch (index) {
		case 0:
			initialX = -100;
			initialY = -260;
			stepX = 40;
			break;
		case 1:
			initialX = -408;
			initialY = -55;
			break;
		case 2:
			initialX = -290;
			initialY = 110;
			break;
		case 3:
			initialX = -50;
			initialY = 176;
			break;
		case 4:
			initialX = 210;
			initialY = 95;
			break;
		case 5:
			initialX = 300;
			initialY = -70;
			break;
		}
		for (int i = 0; i < 5; i++) {
			result[i] = new Vector3 ((initialX + i * stepX) / TransformConstant, initialY / TransformConstant,  0);
		}
		return result;
	}

	/**
	 * 设置展示牛的时候，表示牛几的图片
	 * */
	private void SetNiuImages() {
		for (int i = 0; i < Game.SeatCount; i++) {
			seats[i].niuImage = GetNiuImage (i);
			seats[i].niuImage.gameObject.SetActive (false);
			seats[i].mutipleImage = GetMutipleImage (i);
			seats[i].mutipleImage.gameObject.SetActive (false);
		}
	}

	private Image GetNiuImage(int index) {
		int initialX = 0, initialY = 0;
		Image image = Instantiate (niuImage);
		switch (index) {
		case 0:
			initialX = -100;
			initialY = -260;

			break;
		case 1:
			initialX = -408;
			initialY = -55;
			break;
		case 2:
			initialX = -290;
			initialY = 110;
			break;
		case 3:
			initialX = -50;
			initialY = 176;
			break;
		case 4:
			initialX = 210;
			initialY = 95;
			break;
		case 5:
			initialX = 300;
			initialY = -70;
			break;
		}
			
		image.gameObject.transform.SetParent (cardPanel.transform);


		Vector3 localScale = new Vector3 ();
		localScale.x = 0.64f;
		localScale.y = 0.64f;
		image.transform.localScale = localScale;

		image.transform.position = new Vector3 ( (initialX + 20) / TransformConstant, (initialY - 40) / TransformConstant, 0);
		return image;
	}

	private Image GetMutipleImage(int index) {
		int initialX = 0, initialY = 0;
		Image image = Instantiate (mutipleImage);
		switch (index) {
		case 0:
			initialX = -100;
			initialY = -260;

			break;
		case 1:
			initialX = -408;
			initialY = -55;
			break;
		case 2:
			initialX = -290;
			initialY = 110;
			break;
		case 3:
			initialX = -50;
			initialY = 176;
			break;
		case 4:
			initialX = 210;
			initialY = 95;
			break;
		case 5:
			initialX = 300;
			initialY = -70;
			break;
		}

		image.gameObject.transform.SetParent (cardPanel.transform);


		Vector3 localScale = new Vector3 ();
		localScale.x = 0.4f;
		localScale.y = 0.4f;
		image.transform.localScale = localScale;

		image.transform.position = new Vector3 ( (initialX + 140) / TransformConstant, (initialY - 45) / TransformConstant, 0);
		return image;
	}

	/**
	 * 当比牌的时候，需要一些筹码从一个座位移动到另一个座位，表示谁输谁赢
	 * 这个函数设置座位上的筹码
	 * */
	private void SetChips() {
		for (int i = 0; i < Game.SeatCount; i++) {
			seats [i].chipImages = CreateChips (i);
		}	
	}

	private Image[] CreateChips(int index) {
		Image[] images = new Image[MaxMoveChipCount];
		int startSiblings = 100;

		for (int i = 0; i < images.Length; i++) {
			Image image = Instantiate (seats[index].chipImageForBet);
			image.name = "user" + index + "chip" + i;
			image.gameObject.transform.SetParent (userPanel.transform);
			image.transform.SetSiblingIndex (startSiblings + i);

			Vector3 localScale =  new Vector3 (ChipScale, ChipScale);
			image.transform.localScale = localScale;

			image.transform.position = seats[index].chipImageForBet.transform.position;
			images [i] = image;
			image.gameObject.SetActive (false);
		}
		return images;
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

	/**
	 * 生成一副牌的图片
	 * */
	void CreateDeck() {
		deck = new Deck ();
		List<Image> cards = new List<Image> ();
		//deck.cardFaceSprites = Resources.LoadAll<Sprite>("sprites/mobile");
		deck.cardFaceSprites = Resources.LoadAll<Sprite>("sprites/cards1");

		for (int i = 0; i < cardCount; i++) {
			Image temp = Instantiate (card);

			temp.gameObject.transform.SetParent (cardPanel.transform);
			Vector3 localScale = new Vector3 (1f, 1f);
			temp.transform.localScale = localScale;

			temp.gameObject.transform.position = deckCardPosition.transform.position;

			temp.gameObject.name = "" + i;
			cards.Add (temp);

			temp.gameObject.SetActive (true);
		}
		deck.cards = cards;
		deck.originPosition = cards [0].transform.position;
		deck.cardBack = cards [0].sprite;
		deck.Reset ();
	}

	void LoadNiuAndMultipleImages() {
		niuSprites = Resources.LoadAll<Sprite>("sprites/niu");
		multipleSprites = Resources.LoadAll<Sprite>("sprites/mutiple");
	}

	void CreateBetButtons() {
		betButtons = new Button[4];
		betLabels = new Text[4];
		for (int i = 0; i < 4; i++) {
			betButtons[i] = Instantiate (betButton);
			betButtons [i].gameObject.SetActive (false);
			betButtons [i].transform.SetParent (userPanel.transform);
			betButtons [i].name = "betButton" +i;
			Vector3 localScale = new Vector3 (1f, 1f);
			betButtons[i].gameObject.transform.localScale = localScale;


			betLabels [i] = Instantiate (betLabel);
			betLabels [i].gameObject.SetActive (false);
			betLabels [i].transform.SetParent (userPanel.transform);
			betLabels [i].name = "betLabel" + i;
		    localScale = new Vector3 (1f, 1f);
			betLabels[i].gameObject.transform.localScale = localScale;

		}
	}

	void Create4BetsButtonPositions() {
		betButtonPositionsFor4Button = new Vector3[4];
		betLabelPositionsFor4Button = new Vector3[4];
		int X = -132, Y = -125, YForLabel = -168;
		for (int i = 0; i < betButtonPositionsFor4Button.Length; i++) {
			
			betButtonPositionsFor4Button [i] = new Vector3 (X / TransformConstant, Y / TransformConstant, 0);
			betButtons [i].transform.position = betButtonPositionsFor4Button [i];
			betLabelPositionsFor4Button [i] = new Vector3 (X / TransformConstant, YForLabel / TransformConstant, 0);
			betLabels [i].transform.position = betLabelPositionsFor4Button [i];
			X += 88;
		}
	}

	void Create3BetsButtonPositions() {
		betButtonPositionsFo3Button = new Vector3[3];
		betLabelPositionsFo3Button = new Vector3[3];
		int X = -84, Y = -125, YForLabel = -170;
		for (int i = 0; i < betButtonPositionsFo3Button.Length; i++) {
			
			betButtonPositionsFo3Button [i] = new Vector3 (X / TransformConstant, Y / TransformConstant, 0);
			betLabelPositionsFo3Button [i] = new Vector3 (X / TransformConstant, YForLabel / TransformConstant, 0);
			X += 88;
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