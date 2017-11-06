using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupCardGame : BaseStateController {
	private int MaxMoveChipCount = 8 * 5;
	private float TransformConstant = 71.98f;
	private int cardCount = 30; //生成多少张牌的图片，6 * 5 = 30

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
	private Text scoreLabel;
	[SerializeField]
	private Image niuImage;
	[SerializeField]
	private Image mutipleImage;

	private Image[] niuImages;  //说明是牛几的图片
	private Image[] multipleImages; //倍数的图片

	public Seat[] seats;
	public Deck deck;

	void Awake() {
		CreateDeck ();
		SeatSeatUIs ();
		SetOtherSeatUIs ();
	}

	public override void Reset() {
	}
		
	private void SetOtherSeatUIs() {

		//仅仅为了隐藏界面上的位置示意
		GameObject[] userPanels = GameObject.FindGameObjectsWithTag ("UserPanel");
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
			x = -479;
			y = -57;
			break;
		case 2:
			x = -453;
			y = 250;
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
				seat.bankerSignImage = image;
				break;
			case "Ready Image":
				seat.readyImage = image;
				break;
			case "Chip Image":
				image.transform.SetParent (userPanel.transform);
				image.gameObject.SetActive (false);
				seat.chipImagesForBet = image;
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
					seat.targetScoreLabelPosition = new Vector3 (seat.originScoreLabelPosition.x, seat.originScoreLabelPosition.y + 1.0f, 0);
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
		//secondDealController.SetUserCardPositionsArray (userCardsPositionsArray);
	}

	private Vector3[] SetPlayerCardPositions(int index) {
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

	/**
	 * 设置展示牛的时候，表示牛几的图片
	 * */
	private void SetNiuImages() {
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

	/**
	 * 当比牌的时候，需要一些筹码从一个座位移动到另一个座位，表示谁输谁赢
	 * 这个函数设置座位上的筹码
	 * */
	private void SetChips() {
		for (int i = 0; i < Game.SeatCount; i++) {
			seats [i].chipImages = GetChips (i);
		}	
	}

	private Image[] GetChips(int index) {
		Image[] images = new Image[MaxMoveChipCount];

		for (int i = 0; i < MaxMoveChipCount; i++) {
			Image image = Instantiate (seats[index].chipImagesForBet);
			image.name = "user" + index + "chip" + i;
			image.gameObject.transform.SetParent (cardPanel.transform);

			Vector3 localScale = new Vector3 ();
			localScale.x = 0.3f;
			localScale.y = 0.3f;
			image.transform.localScale = localScale;

			image.transform.position = seats[index].chipImagesForBet.transform.position;
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
		deck.cardFaceSprites = Resources.LoadAll<Sprite>("sprites/mobile");

		for (int i = 0; i < cardCount; i++) {
			Image temp = Instantiate (card);

			temp.gameObject.transform.SetParent (cardPanel.transform);
			Vector3 localScale = new Vector3 ();
			localScale.x = 1;
			localScale.y = 1;
			temp.transform.localScale = localScale;

			temp.gameObject.transform.position = deckCardPosition.transform.position;

			temp.gameObject.name = "" + i;
			cards.Add (temp);

			temp.gameObject.SetActive (true);
		}
		deck.cards = cards;
		deck.Reset ();
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