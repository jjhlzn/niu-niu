using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;

public class CheckCardController : BaseStateController {
	private float user0MoveCardSpeedWhenShowCard = 11f;
	private float moveCardSpeedWhenShowCard = 6f;

	[SerializeField] 
	private GamePlayController gamePlayController;

	[SerializeField]
	private SecondDealController secondDealController;

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
				if (seat.hasPlayer () && seat.player.isPlaying && !playerShowCardCompleted [index]) {
					result = false;
					break;
				}
				index++;
			}
			return result;
		}
	}


	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	// Update is called once per frame
	public new void Update ()  {
		base.Update ();
		if (gamePlayController.state == GameState.CheckCard) {
			if (stateTimeLeft >= 0) {
				gamePlayController.game.ShowStateLabel ("查看手牌: " + Mathf.Round(stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}
		}
			
		if (gamePlayController.state == GameState.CheckCard && Player.Me.isPlaying && !hasShowCard) {
			checkCardPanel.SetActive (true);
		} else {
			checkCardPanel.SetActive (false);
		}

		CheckCardAnimation ();
	}

	public bool isMeShowCard() {
		var game = gamePlayController.game;
		var round = game.currentRound;
		int seatIndex = game.GetSeatIndex (Player.Me.userId);
		if (seatIndex != -1 && round.cardSequenceArray[seatIndex] != null) {
			return true;
		}
		return false;
	}

	public void SetUI() {
		if (isMeShowCard ()) {
			hasShowCard = true;
		}
		
		var game = gamePlayController.game;
		var round = game.currentRound;
		//foreach
		var players = game.PlayingPlayers;
		for (int i = 0; i < players.Count; i++) {
			var player = players [i];
			int seatIndex = player.seat.seatIndex;
			if (round.cardSequenceArray [seatIndex] != null) {
				playerShowCardCompleted [seatIndex] = true;
				Vector3[] showcardPositions = player.seat.showCardPositions;
				int[] sequences = round.cardSequenceArray [seatIndex];
				for(int j = 0;  j < 5; j++) {
					player.cards[j].sprite = deck.GetCardFaceImage(round.playerCardsDict [player.userId][j]); 
					Vector3 targetV = showcardPositions [sequences[j]];
					//有牛的话，第4张牌和第6张牌要有点距离
					if (round.HasNiu (seatIndex) && sequences [j] >= 3) {
						targetV = new Vector3 (targetV.x + 0.3f, targetV.y, targetV.z);
					} 
					player.cards[j].gameObject.transform.position = targetV;
					player.cards[j].transform.SetSiblingIndex (seatIndex * 5 + sequences [j]);
				}

				player.seat.niuImage.sprite = game.getNiuSprite (game.currentRound.niuArray [seatIndex]);
				player.seat.niuImage.gameObject.SetActive (true);
				if (game.currentRound.niuArray [seatIndex] > 6) {
					player.seat.mutipleImage.sprite = game.getMultipleSprite (game.currentRound.multipleArray [seatIndex]);
					player.seat.mutipleImage.gameObject.SetActive (true);
				}

			}
		}
	}

	private void CheckCardAnimation() {
		if (secondDealController.isSecondDealDone) {
			for (int i = 0; i < isMoveCardArray.Length; i++) {
				if (isMoveCardArray [i]) {
					Image[] cards = seats [i].player.cards;
					Vector3[] showcardPositions = seats [i].showCardPositions;
					int[] sequences = gamePlayController.game.currentRound.cardSequenceArray [i];

					float step;
					if (i == 0) {
						step = user0MoveCardSpeedWhenShowCard * Time.deltaTime;
					} else {
						step = moveCardSpeedWhenShowCard * Time.deltaTime;
					}

					bool moveCompleted = true;
					for (int j = 0; j < 5; j++) {
						//Debug.Log ("card " + j + " move to position " + sequences [j]);
						Vector3 targetV = showcardPositions [sequences [j]];
						//有牛的话，第4张牌和第6张牌要有点距离
						if (gamePlayController.game.currentRound.HasNiu (i) && sequences [j] >= 3) {
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
						isMoveCardArray [i] = false;

						var game = gamePlayController.game;
						seats [i].niuImage.sprite = game.getNiuSprite (game.currentRound.niuArray [i]);
						seats [i].niuImage.gameObject.SetActive (true);

						if (game.currentRound.niuArray [i] > 6) {
							seats [i].mutipleImage.sprite = game.getMultipleSprite (game.currentRound.multipleArray [i]);
							seats [i].mutipleImage.gameObject.SetActive (true);
						}
						Debug.Log ("seat " + i + " show card anim completed");
						StartCoroutine (SetPlayerShowCardCompleted (i));
					} 
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

		card.sprite = deck.GetCardFaceImage(gamePlayController.game.currentRound.playerCardsDict[Player.Me.userId][4]);
		anim.Play ("TurnBackNow2");
		yield return new WaitForSeconds (.2f);

		isMoveCardArray [0] = true;
	}

	IEnumerator TurnUserCardsUp(int seatIndex) {
		var game = gamePlayController.game;
		var round = game.currentRound;
		string[] cardPoints = round.playerCardsDict [game.seats [seatIndex].player.userId];
		if (seats [seatIndex].player.userId == Player.Me.userId) {
			Image card = Player.Me.cards[4];
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("TurnUp");

			yield return new WaitForSeconds (FirstDealerController.turnUpTime);
			card.sprite = deck.GetCardFaceImage (cardPoints [4]); 
			anim.Play ("TurnBackNow2");
			yield return new WaitForSeconds (.2f);
			isMoveCardArray [seatIndex] = true;

		} else {

			Image[] cards = seats [seatIndex].player.cards;
			for (int i = 0; i < 5; i++) {
				Image card = cards [i];
				Animator anim = card.GetComponent<Animator> ();
				anim.Play ("TurnUp");
			}

			yield return new WaitForSeconds (FirstDealerController.turnUpTime);


			for (int i = 0; i < 5; i++) {
				Image card = cards [i];
				card.sprite = deck.GetCardFaceImage (cardPoints [i]); 
			}

			for (int i = 0; i < 5; i++) {
				Image card = cards [i];
				Animator anim = card.GetComponent<Animator> ();
				anim.Play ("TurnBackNow2");
			}

			yield return new WaitForSeconds (.2f);
			isMoveCardArray [seatIndex] = true;
		}
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
		StartCoroutine(TurnUserCardsUp(0));
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
		string[] cards = gamePlayController.game.currentRound.playerCardsDict[Player.Me.userId];
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
