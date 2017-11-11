using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;

public class CheckCardController : BaseStateController {
	private float user0MoveCardSpeedWhenShowCard = 7f;
	private float moveCardSpeedWhenShowNiu = 6f;

	[SerializeField] 
	private GamePlayController gamePlayController;

	[SerializeField]
	private GameObject checkCardPanel;

	private Deck deck;
	private Seat[] seats;

	private bool[] playerShowCardCompleted;
	private bool[] isMoveCardArray;
	private bool hasShowCard = false;
	private float stateTimeLeft; //这状态停留的时间

	public void Awake() {
		isMoveCardArray = new bool[Game.SeatCount];
		playerShowCardCompleted = new bool[Game.SeatCount];
	}

	public override void Reset() {
		hasShowCard = false;
		isMoveCardArray = new bool[Game.SeatCount];
		playerShowCardCompleted = new bool[Game.SeatCount];
		stateTimeLeft = Constants.MaxStateTimeLeft;
	}

	public void Init() {
		deck = gamePlayController.game.deck;
		seats = gamePlayController.game.seats;
		stateTimeLeft = Constants.MaxStateTimeLeft;
	}

	public bool isAllPlayerShowCardAnimCompleted {
		get {
			bool result = true;
			Seat[] seats = gamePlayController.game.seats;

			int index = 0;
			foreach (Seat seat in seats) {
				if (seat.hasPlayer () && !playerShowCardCompleted [index]) {
					result = false;
					break;
				}
				index++;
			}
			return result;
		}
	}

	void Update() {
		if (gamePlayController.state == GameState.CheckCard) {
			if (stateTimeLeft >= 0) {
				gamePlayController.game.ShowStateLabel ("查看手牌: " + Mathf.Round(stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}
		}
			
		if (gamePlayController.state == GameState.CheckCard && !hasShowCard) {
			checkCardPanel.SetActive (true);
		} else {
			checkCardPanel.SetActive (false);
		}

		for (int i = 0; i < isMoveCardArray.Length; i++) {
			if (isMoveCardArray [i]) {
				Image[] cards = seats[i].player.cards;
				Vector3[] targetPositions = seats [i].showCardPositions;
				int[] sequences = gamePlayController.game.currentRound.cardSequenceArray [i];

				float step;
				if (i == 0) {
					step = user0MoveCardSpeedWhenShowCard * Time.deltaTime;
				} else {
					step = moveCardSpeedWhenShowNiu * Time.deltaTime;
				}

				bool moveCompleted = true;
				for (int j = 0; j < 5; j++) {
					Vector3 targetV = targetPositions [sequences [j]];
					if (gamePlayController.game.currentRound.HasNiu (i)  && sequences [j] >= 3) {
						targetV = new Vector3 (targetV.x + 0.3f, targetV.y, targetV.z);
					} 
					cards [j].gameObject.transform.position = Vector3.MoveTowards (cards [j].gameObject.transform.position, targetV, step);
					cards [j].transform.SetSiblingIndex (i * 5 + sequences [j]);

					if (!Utils.isTwoPositionIsEqual (cards [j].gameObject.transform.position, targetV)) {
						moveCompleted = false;
					}
				}

				if (moveCompleted) {
					gamePlayController.game.HideStateLabel ();
					isMoveCardArray[i] = false;
					Debug.Log ("seat " + i + " show card anim completed");
					seats [i].niuImage.gameObject.SetActive (true);
					seats [i].mutipleImage.gameObject.SetActive (true);
					StartCoroutine(SetPlayerShowCardCompleted(i));
				} 
			} 
		}
	}

	IEnumerator SetPlayerShowCardCompleted(int index) {
		yield return new WaitForSeconds(.3f);
		playerShowCardCompleted [index] = true;
	}

	IEnumerator TurnCardUp(Image card) {
		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("TurnUp");
		yield return new WaitForSeconds (FirstDealerController.turnUpTime);

		card.sprite = deck.GetCardFaceImage(gamePlayController.game.currentRound.myCards[4]);
		anim.Play ("TurnBackNow2");
		yield return new WaitForSeconds (.2f);

		if (gamePlayController.game.currentRound.HasNiu(0)) {
			isMoveCardArray [0] = true;
		}
	}

	IEnumerator TurnUserCardsUp(int seatIndex) {

		Image[] cards = seats[seatIndex].player.cards;
		for (int i = 0; i < 5; i++) {
			Image card = cards [i];
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("TurnUp");
		}

		yield return new WaitForSeconds (FirstDealerController.turnUpTime);

		string[] cardPoints = gamePlayController.game.currentRound.playerCardsDict [gamePlayController.game.seats [seatIndex].player.userId];
		for (int i = 0; i < 5; i++) {
			Image card = cards [i];
			card.sprite = deck.GetCardFaceImage(cardPoints[i]); 
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

	private void HandleUser0ShowCardNotify(int niu, int[] cardSequences, int mutiple) {
		


		Round round = gamePlayController.game.currentRound;
		round.niuArray[0] = niu;
		round.cardSequenceArray[0] = cardSequences;
		round.multipleArray[0] = mutiple;

		hasShowCard = true;
		//user1 亮牌
		StartCoroutine(TurnCardUp(seats[0].player.cards[4]));
	}

	private void ShowCard() {
		if (gamePlayController.state != GameState.CheckCard) {
			return;
		}
			
		Socket gameSocket = gamePlayController.gameSocket;
		var request = new {
			userId = Player.Me.userId,
			roomNo = gamePlayController.game.roomNo
		};

		gameSocket.EmitJson (Messages.ShowCard, JsonConvert.SerializeObject (request), (string msg) => {
			Debug.Log("ShowCardAck: " + msg);

			ShowCardAck notify = JsonConvert.DeserializeObject<ShowCardAck[]>(msg)[0];
			HandleUser0ShowCardNotify(notify.niu, notify.cardSequences, notify.multiple);
		});
	}

	public void HandleResponse(GoToCheckCardNotify notify) {
		string[] cards = gamePlayController.game.currentRound.myCards;
		cards [4] = notify.card;

		gamePlayController.game.HideStateLabel ();
		gamePlayController.state = GameState.CheckCard;
	}

	public void HandleResponse(SomePlayerShowCardNotify notify) {
		Game game = gamePlayController.game;
		int seatIndex = game.GetSeatIndex (notify.userId);
		game.currentRound.playerCardsDict [notify.userId] = notify.cards;
		game.currentRound.cardSequenceArray [seatIndex] = notify.cardSequences;
		game.currentRound.multipleArray [seatIndex] = notify.multiple;
		game.currentRound.niuArray [seatIndex] = notify.niu;

		if (seatIndex == 0) {
			HandleUser0ShowCardNotify (notify.niu, notify.cardSequences, notify.multiple);
		} else {
			StartCoroutine(TurnUserCardsUp (seatIndex));
		}

	}

}
