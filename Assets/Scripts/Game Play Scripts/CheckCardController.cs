using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BestHTTP.SocketIO;
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
	[SerializeField]
	private GameObject cuoCardPanel;
	[SerializeField]
	private Image fifthCard;

	private bool[] playerShowCardCompleted;
	private bool[] isTurnCardArray;
	private float stateTimeLeft; //这状态停留的时间
	private bool hasPlayCountDown;
	public UIEraserTexture eraser;

	public void Awake() {
		isTurnCardArray = new bool[Game.SeatCount];
		playerShowCardCompleted = new bool[Game.SeatCount];
	}


	public override void Reset() {
	    // hasShowCard = false;
		isTurnCardArray = new bool[Game.SeatCount];
		playerShowCardCompleted = new bool[Game.SeatCount];
		stateTimeLeft = Constants.MaxStateTimeLeft;
		hasPlayCountDown = false;
		checkCardPanel.gameObject.SetActive (false);
	}

	public void Init() {
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
		
	public new void Update ()  {
		base.Update ();
		if (gamePlayController.state == GameState.CheckCard) {
			if (stateTimeLeft <=  3.5f && !hasPlayCountDown) {
				hasPlayCountDown = true;
				MusicController.instance.Play (AudioItem.CountDown);
			}

			if (stateTimeLeft >= 0) {
				gamePlayController.game.ShowStateLabel ("查看手牌: " + Mathf.Round(stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}

			if (Player.Me.isPlaying && !Player.Me.hasShowCard && !checkCardPanel.gameObject.activeInHierarchy) {
				checkCardPanel.gameObject.SetActive (true);
				Debug.Log ("Show checkCardPanel");
			}
		}
	
		CheckCardAnimation ();
	}

	public bool isMeShowCard() {
		var game = gamePlayController.game;
		var round = game.currentRound;
		int seatIndex = game.GetSeatIndex (Player.Me.userId);
		if (seatIndex != -1 && round.cardSequenceArray.ContainsKey(Player.Me.userId)) {
			return true;
		}
		return false;
	}

	public void SetUI() {
		if (isMeShowCard ()) {
			Player.Me.hasShowCard = true;
			checkCardPanel.gameObject.SetActive (false);
		}
		
		var game = gamePlayController.game;
		var round = game.currentRound;
		//foreach
		var players = game.PlayingPlayers;
		for (int i = 0; i < players.Count; i++) {
			var player = players [i];
			int seatIndex = player.seat.seatIndex;
			if (round.cardSequenceArray.ContainsKey(player.userId)) {
				playerShowCardCompleted [seatIndex] = true;
				Vector3[] showcardPositions = player.seat.showCardPositions;
				int[] sequences = round.cardSequenceArray [player.userId];
				for(int j = 0;  j < 5; j++) {
					player.cards[j].sprite = deck.GetCardFaceImage(round.playerCardsDict [player.userId][j]); 
					Vector3 targetV = showcardPositions [sequences[j]];
					//有牛的话，第4张牌和第6张牌要有点距离
					float interval = 0f;
					if (round.HasNiu (player.userId) && sequences [j] >= 3) {
						interval = 0.3f;
						if (seatIndex != 0) {
							interval = 0.22f;
						}
					} 
					targetV = new Vector3 (targetV.x / SetupCardGame.TransformConstant + interval , targetV.y / SetupCardGame.TransformConstant, targetV.z);
					player.cards[j].gameObject.transform.position = targetV;
					player.cards[j].transform.SetSiblingIndex (seatIndex * 5 + sequences [j]);
				}

				player.seat.niuImage.sprite = game.getNiuSprite (game.currentRound.niuArray [player.userId]);
				player.seat.niuImage.gameObject.SetActive (true);
				if (game.currentRound.niuArray [player.userId] > 6) {
					player.seat.mutipleImage.sprite = game.getMultipleSprite (game.currentRound.multipleArray [player.userId]);
					player.seat.mutipleImage.gameObject.SetActive (true);
				}
			}
		}
	}

	private void CheckCardAnimation() {
		var round = gamePlayController.game.currentRound;
		if (secondDealController.isSecondDealDone) {
			for (int i = 0; i < isTurnCardArray.Length; i++) {
				if (isTurnCardArray [i]) {
					isTurnCardArray [i] = false;
					TurnUserCardsUp (i);
				}
			}
		}
	}

	IEnumerator SetPlayerShowCardCompleted(int index) {
		yield return new WaitForSeconds(.3f);
		playerShowCardCompleted [index] = true;
	}


	IEnumerator TurnCardUp2(Image card, string cardValue) {
		if (!string.IsNullOrEmpty (cardValue)) {
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("Turn90");
			yield return new WaitForSeconds (0.3f);
			card.gameObject.SetActive (false);
			card.sprite = deck.GetCardFaceImage (cardValue);
			card.gameObject.SetActive (true);
			anim.Play ("Turn85_2");
			yield return new WaitForSeconds (0.2f);
		}
		yield return new WaitForSeconds (FirstDealerController.turnUpTime);
	}

	void TurnUserCardsUp(int seatIndex) {
		isTurnCardArray [seatIndex] = false;

		var game = gamePlayController.game;
		var round = game.currentRound;
		string[] cardPoints = round.playerCardsDict [game.seats [seatIndex].player.userId];
		if (seats [seatIndex].player.userId == Player.Me.userId) {
			Image card = Player.Me.cards[4];
			StartCoroutine( game.TurnCardUp (card, cardPoints [4], () => {
				MoveCards (seats [seatIndex].player);
			}) );


		} else {
			Image[] cards = seats [seatIndex].player.cards;
			for (int i = 0; i < 5; i++) {
				Image card = cards [i];
				if (i == 4) {
					StartCoroutine( game.TurnCardUp (card, cardPoints [i], () => {
						MoveCards (seats [seatIndex].player);
					}) );
				} else {
					StartCoroutine( game.TurnCardUp (card, cardPoints [i]) );
				}
			}
		}
	}

	private void MoveCards(Player player) {
		var round = gamePlayController.game.currentRound;
		Image[] cards = player.cards;
		Vector3[] showcardPositions = player.seat.showCardPositions;

		int index = player.seat.seatIndex;
		int[] sequences = gamePlayController.game.currentRound.cardSequenceArray [player.userId];
	

		for (int j = 0; j < 5; j++) {
			Vector3 targetV = showcardPositions [sequences [j]];
			//有牛的话，第4张牌和第6张牌要有点距离
			if (game.currentRound.HasNiu (player.userId) && sequences [j] >= 3) {
				int interval = 30;
				if (player.seat.seatIndex != 0) {
					interval = 22;	
				}
				targetV = new Vector3 (targetV.x + interval, targetV.y, targetV.z);
			} 
				
			int cardIndex = j;
			cards [cardIndex].transform.SetSiblingIndex (index * 5 + sequences [cardIndex]);
			Tween t = cards [j].transform.DOLocalMove (targetV, 0.25f);

			if (j == 4) {
				t.OnComplete (() => {
					for(int m = 0; m < 5; m++) {
						cards [m].transform.SetSiblingIndex (index * 5 + sequences [m]);
					}

					MusicController.instance.Play ("niu" + round.niuArray [player.userId], player.sex);

					var game = gamePlayController.game;
					player.seat.niuImage.sprite = game.getNiuSprite (game.currentRound.niuArray [player.userId]);
					player.seat.niuImage.gameObject.SetActive (true);

					if (game.currentRound.niuArray [player.userId] > 6) {
						player.seat.mutipleImage.sprite = game.getMultipleSprite (game.currentRound.multipleArray [player.userId]);
						player.seat.mutipleImage.gameObject.SetActive (true);
					}
					Debug.Log ("seat " + index + " show card anim completed");
					StartCoroutine (SetPlayerShowCardCompleted (index));
				});
			} else {
				t.OnComplete (() => {
					for(int m = 0; m < 5; m++) {
						cards [m].transform.SetSiblingIndex (index * 5 + sequences [m]);
					}
				});
			}
		} 
		
	}

	public void CuoCardClick() {
		//eraser.Reset ();
		//fifthCard.sprite = deck.GetCardFaceImage (game.currentRound.playerCardsDict [Player.Me.userId] [4]);
		cuoCardPanel.SetActive(true);
		if (eraser != null)
			eraser.ReadyForErase = true;
	}

	public void ShowCardClick() {
		ShowCard ();
	}

	private void HandleUser0ShowCardNotify(int niu, int[] cardSequences, int mutiple) {
		cuoCardPanel.gameObject.SetActive (false);
		checkCardPanel.gameObject.SetActive(false);

		Round round = gamePlayController.game.currentRound;
		round.niuArray[seats[0].player.userId] = niu;
		round.cardSequenceArray[seats[0].player.userId] = cardSequences;
		round.multipleArray[seats[0].player.userId] = mutiple;

		Player.Me.hasShowCard = true;
		MusicController.instance.Play (AudioItem.ShowCardTip, Player.Me.sex);
		//user1 亮牌
		TurnUserCardsUp(0);
	}

	public void ShowCardForChuoPai() {
		StartCoroutine( ShowCardForChuoPai2 () );
	}

	public IEnumerator ShowCardForChuoPai2() {
		yield return new WaitForSeconds (.3f);
		ShowCard ();
		eraser.Reset ();
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

		gameSocket.Emit (Messages.ShowCard, JsonConvert.SerializeObject (request));

	}

	public void HandleResponse(GoToCheckCardNotify notify) {
		string[] cards = gamePlayController.game.currentRound.playerCardsDict[Player.Me.userId];
		cards [4] = notify.card;

		gamePlayController.state = GameState.CheckCard;
	}

	public void HandleResponse(SomePlayerShowCardNotify notify) {
		Game game = gamePlayController.game;
		int seatIndex = game.GetSeatIndex (notify.userId);
		game.currentRound.playerCardsDict [notify.userId] = notify.cards;
		game.currentRound.cardSequenceArray [notify.userId] = notify.cardSequences;
		game.currentRound.multipleArray [notify.userId] = notify.multiple;
		game.currentRound.niuArray [notify.userId] = notify.niu;

		if (seatIndex == 0) {
			HandleUser0ShowCardNotify (notify.niu, notify.cardSequences, notify.multiple);
		} else {
			isTurnCardArray[seatIndex] = true;
		}
	}

}
