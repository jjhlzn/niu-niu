using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompareCardController : BaseStateController {
	private float chipMoveSpeed = 14f;
	private float scoreLabelMoveSpeed = 5f;
	private float MoveTime = 2f;

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private CheckCardController checkCardController;

	[SerializeField]
	private Button readyButton;

	private Seat[] seats;

	public int[] moveChipFromOtheToBankerArray;
	public int[] moveChipFromBankerToOtherArray;
	public bool moveToBanker;
	public bool moveFromBanker;
	public bool showScoreLabel;
	public bool moveScoreLabel;
	public bool allAnimCompleted;
	private float moveTimeLeft; 

	public void Init() {
		seats = gamePlayController.game.seats;
	}

	public override void Reset() {
		moveChipFromOtheToBankerArray = null;
		moveChipFromBankerToOtherArray = null;
		moveToBanker = false;
		moveFromBanker = false;
		showScoreLabel = false;
		moveScoreLabel = false;
		allAnimCompleted = false;
	}
		
	void Awake () {
		moveTimeLeft = MoveTime;
	}
		
	private void HideChips() {
		for (int i = 0; i < Game.SeatCount; i++) {
			for (int j = 0; j < seats[i].chipImages.Length; j++) {
				seats[i].chipImages[j].gameObject.SetActive (false);
			}
		}
	}
		
	void Update () {
		if (gamePlayController.state == GameState.CompareCard ) {
			gamePlayController.game.ShowStateLabel ("比牌中...");
		}

		CompareCardAnimation ();
	}

	private void CompareCardAnimation() {
		if (checkCardController.isAllPlayerShowCardAnimCompleted) {

			if (moveToBanker) {

				if (moveChipFromOtheToBankerArray.Length == 0) {
					//moveToBanker结束
					moveTimeLeft = MoveTime;
					moveToBanker = false;
					moveFromBanker = true;
					HideChips ();
				} else {
					for (int i = 0; i < moveChipFromOtheToBankerArray.Length; i++) {
						MoveChipsFromSeat (moveChipFromOtheToBankerArray [i]);
					}
					moveTimeLeft -= Time.deltaTime;
					if (moveTimeLeft <= 0) {
						//moveToBanker结束
						moveTimeLeft = MoveTime;
						moveToBanker = false;
						moveFromBanker = true;
						HideChips ();
					}
				}

			} 

			if (moveFromBanker) {
				if (moveChipFromBankerToOtherArray.Length == 0) {
					//moveFromBanker结束
					moveTimeLeft = MoveTime;
					moveFromBanker = false;
					showScoreLabel = true;
					HideChips ();
				} else {

					for (int i = 0; i < moveChipFromBankerToOtherArray.Length; i++) {
						MoveChipsToSeat (moveChipFromBankerToOtherArray [i]);
					}
					moveTimeLeft -= Time.deltaTime;
					if (moveTimeLeft <= 0) {
						//moveFromBanker结束
						moveTimeLeft = MoveTime;
						moveFromBanker = false;
						showScoreLabel = true;
						HideChips ();
					}
				}
			}


			if (showScoreLabel) {
				showScoreLabel = false;
				ShowScoreLabels ();
			}

			if (moveScoreLabel) {
				MoveScoreLabels ();
			}
		} 
	}

	private void ShowScoreLabels() {
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer()) {
				ShowScoreLabel (i);
			}
		}
	}

	private void ShowScoreLabel(int index) {
		Text scoreLabel = seats [index].scoreLabel;
		scoreLabel.text = gamePlayController.game.currentRound.resultDict [seats [index].player.userId] + "";
		seats [index].playerScoreLabel.text = seats [index].player.score + "";
		scoreLabel.gameObject.SetActive (true);
		Animator anim = scoreLabel.GetComponent<Animator> ();
		StartCoroutine(ShowScoreLabel(scoreLabel, anim));
	}

	IEnumerator ShowScoreLabel(Text text, Animator anim) {
		anim.Play("ShowUp");
		yield return new WaitForSeconds (.4f);
		showScoreLabel = false;	
		moveScoreLabel = true;
	}

	private void MoveScoreLabels () {
		
		Seat[] seats = gamePlayController.game.seats;
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer()) {
				MoveScoreLabel (i);
			}
		}
	}

	private void MoveScoreLabel(int index) {
		Text text = seats [index].scoreLabel;
		text.transform.position = Vector3.MoveTowards (text.transform.position, seats[index].targetScoreLabelPosition, scoreLabelMoveSpeed * Time.deltaTime);

		if (Utils.isTwoPositionIsEqual(text.transform.position, seats[index].targetScoreLabelPosition)) {
			//text.gameObject.SetActive (false);
			readyButton.gameObject.SetActive (true);
			moveScoreLabel = false;
			allAnimCompleted = true;
			gamePlayController.game.HideStateLabel ();

			if (gamePlayController.game.HasNextRound ()) {
				if (gamePlayController.state == GameState.CompareCard)
					gamePlayController.state = GameState.WaitForNextRound;
			}
			else
				gamePlayController.state = GameState.GameOver;
			Debug.Log ("Go to " + gamePlayController.state.value);
		}
	}

	private void MoveChipsFromSeat(int seat) {
		Game game = gamePlayController.game;
		int toSeat = game.GetSeatIndex (game.currentRound.banker);

		MoveChips (seat, toSeat);
	}

	private void MoveChipsToSeat(int seat) {
		Game game = gamePlayController.game;
		int fromSeat = game.GetSeatIndex (game.currentRound.banker);

		MoveChips (fromSeat, seat);
	}
		
	private void MoveChips( int from,  int to) {
		
		float step = chipMoveSpeed * Time.deltaTime;

		float waitTime = 0;
	
		Vector3 targetPosition = seats [to].chipImages [0].transform.position; //TODO: 总是到同一个位置

		bool moveCompleted = true;
		  
		int startIndex = to * 8;
		for (int i = 0; i < 8; i++) {
			Image image = seats [from].chipImages [startIndex + i];
			if (!image.gameObject.activeInHierarchy)
				image.gameObject.SetActive (true);
	
			StartCoroutine (moveChip (image, targetPosition, waitTime, step));

			waitTime += 0.1f;

			if (!Utils.isTwoPositionIsEqual(image.transform.position, targetPosition)) {
				moveCompleted = false;
			} 
		}
	}


	IEnumerator moveChip(Image image, Vector3 to, float waitTime, float step) {
		yield return new WaitForSeconds (waitTime);
		//Debug.Log ("waitTime = " + waitTime);
		image.transform.position = Vector3.MoveTowards (image.transform.position, to, step);
	} 

	public void HandleResponse(GoToCompareCardNotify notify) {
		Game game = gamePlayController.game;
		game.currentRound.resultDict = notify.resultDict;

		moveToBanker = true;

		int count = 0;
		foreach(var item in notify.resultDict) {
			if (item.Value < 0 && item.Key != game.currentRound.banker) {
				count++;
			}
		}

		moveChipFromOtheToBankerArray = new int[count];
		int index = 0;
		foreach(var item in notify.resultDict) {
			if (item.Value < 0 && item.Key != game.currentRound.banker) {
				moveChipFromOtheToBankerArray[index++] =  game.GetSeatIndex(item.Key);
			}
		}
			
		count = 0;
		foreach(var item in notify.resultDict) {
			if (item.Value > 0 && item.Key != game.currentRound.banker) {
				count++;
			}
		}

		moveChipFromBankerToOtherArray = new int[count];
		index = 0;
		foreach(var item in notify.resultDict) {
			if (item.Value > 0 && item.Key != game.currentRound.banker) {
				moveChipFromBankerToOtherArray[index++] = game.GetSeatIndex(item.Key);
			}
		}

		var playingPlayers = game.PlayingPlayers;
		for (int i = 0; i < playingPlayers.Count; i++) {
			playingPlayers [i].score = notify.scoreDict [playingPlayers [i].userId];
		}

		game.HideStateLabel();
		gamePlayController.state = GameState.CompareCard;
	}

}
