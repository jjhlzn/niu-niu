using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
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

	private Deck deck {
		get {
			return gamePlayController.game.deck;
		}
	}
	private Seat[] seats {
		get {
			return gamePlayController.game.seats;
		}
	}

	private bool[] playerShowCardCompleted;
	private bool[] isTurnCardArray;
	//private bool hasShowCard = false;
	private float stateTimeLeft; //这状态停留的时间
	private bool hasPlayCountDown;

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

	// Update is called once per frame
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
	
		CheckCardAnimation2 ();
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
			Player.Me.hasShowCard = true;
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
						targetV = new Vector3 (targetV.x / SetupCardGame.TransformConstant + 0.3f, targetV.y / SetupCardGame.TransformConstant, targetV.z);
					} else {
						targetV = new Vector3 (targetV.x / SetupCardGame.TransformConstant, targetV.y / SetupCardGame.TransformConstant, targetV.z);
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

	private void CheckCardAnimation2() {
		var round = gamePlayController.game.currentRound;
		if (secondDealController.isSecondDealDone) {
			for (int i = 0; i < isTurnCardArray.Length; i++) {
				if (isTurnCardArray [i]) {
					isTurnCardArray [i] = false;
					StartCoroutine (TurnUserCardsUp (i));
				}
			}
		}

	}

	IEnumerator SetPlayerShowCardCompleted(int index) {
		yield return new WaitForSeconds(.3f);
		playerShowCardCompleted [index] = true;
	}



	IEnumerator TurnUserCardsUp(int seatIndex) {
		isTurnCardArray [seatIndex] = false;

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

			MoveCards (seats [seatIndex].player);

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
			MoveCards (seats [seatIndex].player);
		}
	}

	private void MoveCards(Player player) {
		var round = gamePlayController.game.currentRound;
		Image[] cards = player.cards;
		Vector3[] showcardPositions = player.seat.showCardPositions;

		int index = player.seat.seatIndex;
		int[] sequences = gamePlayController.game.currentRound.cardSequenceArray [index];

		float step;
		if (index == 0) {
			step = user0MoveCardSpeedWhenShowCard * Time.deltaTime;
		} else {
			step = moveCardSpeedWhenShowCard * Time.deltaTime;
		}

		for (int j = 0; j < 5; j++) {
			Vector3 targetV = showcardPositions [sequences [j]];
			//有牛的话，第4张牌和第6张牌要有点距离
			if (gamePlayController.game.currentRound.HasNiu (index) && sequences [j] >= 3) {
				targetV = new Vector3 (targetV.x + 30f, targetV.y, targetV.z);
			} 
				
			Tween t = cards [j].transform.DOLocalMove (targetV, 0.3f);
			cards [j].transform.SetSiblingIndex (sequences [j]);

			if (j == 4) {
				t.OnComplete (() => {

					MusicController.instance.Play ("niu" + round.niuArray [index], player.sex);


					var game = gamePlayController.game;
					seats [index].niuImage.sprite = game.getNiuSprite (game.currentRound.niuArray [index]);
					seats [index].niuImage.gameObject.SetActive (true);

					if (game.currentRound.niuArray [index] > 6) {
						seats [index].mutipleImage.sprite = game.getMultipleSprite (game.currentRound.multipleArray [index]);
						seats [index].mutipleImage.gameObject.SetActive (true);
					}
					Debug.Log ("seat " + index + " show card anim completed");
					StartCoroutine (SetPlayerShowCardCompleted (index));
				});
			}
		} 
		
	}

	public void CuoCardClick() {
		ShowCard ();
	}

	public void ShowCardClick() {
		ShowCard ();
	}

	private void HandleUser0ShowCardNotify(int niu, int[] cardSequences, int mutiple) {
		checkCardPanel.gameObject.SetActive(false);

		Round round = gamePlayController.game.currentRound;
		round.niuArray[0] = niu;
		round.cardSequenceArray[0] = cardSequences;
		round.multipleArray[0] = mutiple;

		Player.Me.hasShowCard = true;
		MusicController.instance.Play (AudioItem.ShowCardTip, Player.Me.sex);
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
		});
	}

	public void HandleResponse(GoToCheckCardNotify notify) {
		string[] cards = gamePlayController.game.currentRound.playerCardsDict[Player.Me.userId];
		cards [4] = notify.card;

		//gamePlayController.game.HideStateLabel ();
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
			isTurnCardArray[seatIndex] = true;
		}
	}

}
