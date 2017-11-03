using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;

public class CheckCardController : MonoBehaviour {

	[SerializeField] 
	private GamePlayController gamePlayController;

	[SerializeField]
	private GameObject checkCardPanel;

	private List<Image> deckCards;
	public Sprite[] cardSprites;

	private Vector3[][] showCardPositionsArray;

	private float user1MoveCardSpeedWhenShowCard = 9f;
	private float moveCardSpeedWhenShowNiu = 6f;
	/*
	[SerializeField]
	private Button cuoCardButton;
	[SerializeField]
	private Button showCardButton; */

	public bool[] isMoveCardArray;

	private bool hasShowCard = false;

	public void Awake() {
		isMoveCardArray = new bool[Game.SeatCount];
	}

	public void Reset() {
		hasShowCard = false;
	}


	void Update() {
		//Debug.Log ("state = " + gamePlayController.state.value);
		if (gamePlayController.state == GameState.CheckCard && !hasShowCard) {
			checkCardPanel.SetActive (true);
		} else {
			checkCardPanel.SetActive (false);
		}

		for (int i = 0; i < isMoveCardArray.Length; i++) {
			if (isMoveCardArray [i]) {
				Image[] cards = GetCards (i);
				Vector3[] targetPositions = showCardPositionsArray [i];
				int[] sequences = gamePlayController.game.currentRound.cardSequenceArray [i];

				float step;
				if (i == 0) {
					step = user1MoveCardSpeedWhenShowCard * Time.deltaTime;
				} else {
					step = moveCardSpeedWhenShowNiu * Time.deltaTime;
				}
				for (int j = 0; j < 5; j++) {
					if (gamePlayController.game.currentRound.HasNiu (i)  && sequences [j] >= 3) {
						Vector3 targetV = targetPositions [sequences [j]];
						Vector3 v = new Vector3 (targetV.x + 0.3f, targetV.y, targetV.z);

						cards [j].gameObject.transform.position = Vector3.MoveTowards (cards [j].gameObject.transform.position, v, step);
					} else {
						cards [j].gameObject.transform.position = Vector3.MoveTowards (cards [j].gameObject.transform.position, targetPositions [sequences[j]], step);
					}

					cards [j].gameObject.layer = sequences [j];
					cards [j].transform.SetSiblingIndex (sequences [j]);
				}	

			} 
		}
	}

	IEnumerator TurnCardUp(Image card) {
		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("TurnUp");
		yield return new WaitForSeconds (.4f);

		card.sprite = Utils.findCardSprite(cardSprites, gamePlayController.game.currentRound.myCards[4]);
		anim.Play ("TurnBackNow2");
		yield return new WaitForSeconds (.2f);

		if (gamePlayController.game.currentRound.HasNiu(0)) {
			isMoveCardArray [0] = true;
		}
	}


	private Image[] GetCards(int seatIndex) {
		int playerCount = gamePlayController.game.PlayerCount;
		Seat[] seats = gamePlayController.game.seats;
		int playerIndex = -1;
		for (int i = 0; i <= seatIndex; i++) {
			if (seats [i].hasPlayer ()) {
				//Debug.Log ("seat " + i + " has palyer " + seats [i].player.userId);
				playerIndex++;
			}
		}

		//Debug.Log ("seatIndex = " + seatIndex);
		//Debug.Log ("playerIndex = " + playerIndex);


		Image[] cards = new Image[5];
		cards [0] = deckCards [playerIndex * 4];
		cards [1] = deckCards [playerIndex * 4 + 1];
		cards [2] = deckCards [playerIndex * 4 + 2];
		cards [3] = deckCards [playerIndex * 4 + 3];
		cards [4] = deckCards [playerCount * 4 + playerIndex];
		return cards;
	}

	IEnumerator TurnUserCardsUp(int seatIndex) {

		Image[] cards = GetCards (seatIndex);
		for (int i = 0; i < 5; i++) {
			Image card = cards [i];
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("TurnUp");
		}

		yield return new WaitForSeconds (.4f);

		string[] cardPoints = gamePlayController.game.currentRound.playerCardsDict [gamePlayController.game.seats [seatIndex].player.userId];
		for (int i = 0; i < 5; i++) {
			Image card = cards [i];
			card.sprite = Utils.findCardSprite(cardSprites, cardPoints[i]); 
		}

		for (int i = 0; i < 5; i++) {
			Image card = cards [i];
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("TurnBackNow2");
		}

		yield return new WaitForSeconds (.2f);
		isMoveCardArray [seatIndex] = true;
	}

	public void CuoCardClick() {
		ShowCard ();
	}

	public void ShowCardClick() {
		ShowCard ();
	}

	private void ShowCard() {
		if (gamePlayController.state != GameState.CheckCard) {
			return;
		}
			
		Socket gameSocket = gamePlayController.gameSocket;
		var request = new {
			userId = Player.Me.userId,
			roomNo = gamePlayController.game.roomNo,
			cards = gamePlayController.game.currentRound.myCards
		};

		gameSocket.EmitJson (Messages.ShowCard, JsonConvert.SerializeObject (request), (string msg) => {
			Debug.Log("ShowCardAck: " + msg);

			ShowCardAck notify = JsonConvert.DeserializeObject<ShowCardAck[]>(msg)[0];
			Round round = gamePlayController.game.currentRound;
			round.niuArray[0] = notify.niu;
			round.cardSequenceArray[0] = notify.cardSequences;
			round.multipleArray[0] = notify.multiple;

			hasShowCard = true;
			//user1 亮牌
			StartCoroutine(TurnCardUp(deckCards[4 * gamePlayController.game.PlayerCount]));
			//StartCoroutine (TurnUser1Cards ());
		});
	}

	public void SetDeckCards(List<Image> cards) {
		this.deckCards = cards;
	}

	public void SetCardSprites(Sprite[] cardSprites) {
		this.cardSprites = cardSprites;
	}

	public void SetShowCardPositionsArray(Vector3[][] array) {
		this.showCardPositionsArray = array;
	}

	public void HandleResponse(GoToCheckCardNotify notify) {
		string[] cards = gamePlayController.game.currentRound.myCards;
		cards [4] = notify.card;

		gamePlayController.state = GameState.CheckCard;
	}

	public void HandleResponse(SomePlayerShowCardNotify notify) {
		Game game = gamePlayController.game;
		int seatIndex = game.GetSeatIndex (notify.userId);
		game.currentRound.playerCardsDict [notify.userId] = notify.cards;
		game.currentRound.cardSequenceArray [seatIndex] = notify.cardSequences;
		game.currentRound.multipleArray [seatIndex] = notify.multiple;
		game.currentRound.niuArray [seatIndex] = notify.niu;

		StartCoroutine(TurnUserCardsUp (seatIndex));
	}

}
