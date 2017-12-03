using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompareCardController : BaseStateController {
	private float chipMoveSpeed = 20f;
	private float scoreLabelMoveSpeed = 5f;
	private float MoveTime = 2f;

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private CheckCardController checkCardController;

	[SerializeField]
	private WaitForNextRoundController waitForNextRoundController;

	[SerializeField]
	private GameOverController gameOverController;

	[SerializeField]
	private Button readyButton;

	private Seat[] seats {
		get {
			return gamePlayController.game.seats;
		}
	}

	public int[] moveChipFromOtheToBankerArray;
	public int[] moveChipFromBankerToOtherArray;
	private bool moveToBanker;
	private bool moveFromBanker;
	private bool showScoreLabel;
	private bool moveScoreLabel;
	private bool hasSetAfterAllAnimCompleted;
	public bool allAnimCompleted;
	private bool[] hasPlayedTransmitCoin = new bool[Game.SeatCount];
	private float moveTimeLeft;

	public void Init() {
	}

	public override void Reset() {
		moveChipFromOtheToBankerArray = null;
		moveChipFromBankerToOtherArray = null;
		moveToBanker = false;
		moveFromBanker = false;
		showScoreLabel = false;
		moveScoreLabel = false;
		allAnimCompleted = false;
		hasSetAfterAllAnimCompleted = false;
		for (int i = 0; i < Game.SeatCount; i++) {
			hasPlayedTransmitCoin [i] = false;
		}
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
		
	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	// Update is called once per frame
	public new void Update ()  {
		
		base.Update ();
		if (gamePlayController.state == GameState.CompareCard) {
			gamePlayController.game.ShowStateLabel ("比牌中...");
		}
		CompareCardAnimation ();
	}

	private void CompareCardAnimation() {
		if (checkCardController.isAllPlayerShowCardAnimCompleted) {
			//Debug.Log ("CompareCardAnimation Start");
			if (moveToBanker) {
				
				if (moveChipFromOtheToBankerArray.Length == 0) {
					//moveToBanker结束
					moveTimeLeft = MoveTime;
					moveToBanker = false;
					moveFromBanker = true;
					HideChips ();
				} else {
					for (int i = 0; i < moveChipFromOtheToBankerArray.Length; i++) {
						int seatIndex = moveChipFromOtheToBankerArray [i];
						MoveChipsFromSeat (seatIndex);
						if (!hasPlayedTransmitCoin[seatIndex]) {
							hasPlayedTransmitCoin [seatIndex] = true;
							MusicController.instance.Play (AudioItem.TransmitCoin, seats [seatIndex].player.sex);
						}
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
						int seatIndex = moveChipFromBankerToOtherArray [i];
						MoveChipsToSeat (seatIndex);
						if (!hasPlayedTransmitCoin[seatIndex]) {
							hasPlayedTransmitCoin [seatIndex] = true;
							MusicController.instance.Play (AudioItem.TransmitCoin, seats [seatIndex].player.sex);
						}
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

			if (allAnimCompleted && !hasSetAfterAllAnimCompleted) {
				hasSetAfterAllAnimCompleted = true;
				if (gamePlayController.game.HasNextRound ()) {
					if (gamePlayController.state == GameState.CompareCard) {
						gamePlayController.state = GameState.WaitForNextRound;
						waitForNextRoundController.Reset ();
					}
				} else {
					gamePlayController.state = GameState.GameOver;
					if (gameOverController.isGetGameOverNotify) {
						gameOverController.HandleGameOverResponse ();
					}
				}
				gamePlayController.game.HideStateLabel ();
				Debug.Log ("Go to " + gamePlayController.state.value);
			}
			//Debug.Log ("CompareCardAnimation End");
		} 
	}

	private void ShowScoreLabels() {
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer() && seats[i].player.isPlaying) {
				ShowScoreLabel (i);
			}
		}
	}

	private void ShowScoreLabel(int index) {
		Text scoreLabel = seats [index].scoreLabel;
		//Debug.Log ("userId: " + seats [index].player.userId);
		int thisRoundScore = gamePlayController.game.currentRound.resultDict [seats [index].player.userId];
		scoreLabel.text = Utils.GetNumberSring (thisRoundScore);
		int score = seats [index].player.score;
		seats [index].playerScoreLabel.text = Utils.GetNumberSring (score);
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

		//bool moveCompleted = true;
		  
		int startIndex = to * 8;
		for (int i = 0; i < 8; i++) {
			Image image = seats [from].chipImages [startIndex + i];
			if (!image.gameObject.activeInHierarchy)
				image.gameObject.SetActive (true);
	
			StartCoroutine (moveChip (image, targetPosition, waitTime, step));

			waitTime += 0.1f;

			if (!Utils.isTwoPositionIsEqual(image.transform.position, targetPosition)) {
				//moveCompleted = false;
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


		Dictionary<string, int> scoreDict =	notify.scoreDict;
		Dictionary<string, int> resultDict = notify.resultDict;

		HandleCurrentRoundOver (resultDict, scoreDict); 
	}

	public void HandleCurrentRoundOver(Dictionary<string, int>  resultDict, Dictionary<string, int> scoreDict) {
		var game = gamePlayController.game;
		int count = 0;
		game.currentRound.resultDict = resultDict;
		foreach(var item in resultDict) {
			if (item.Value < 0 && item.Key != game.currentRound.banker) {
				count++;
			}
		}

		moveChipFromOtheToBankerArray = new int[count];
		int index = 0;
		foreach(var item in resultDict) {
			if (item.Value < 0 && item.Key != game.currentRound.banker) {
				moveChipFromOtheToBankerArray[index++] =  game.GetSeatIndex(item.Key);
			}
		}

		count = 0;
		foreach(var item in resultDict) {
			if (item.Value > 0 && item.Key != game.currentRound.banker) {
				count++;
			}
		}

		moveChipFromBankerToOtherArray = new int[count];
		index = 0;
		foreach(var item in resultDict) {
			if (item.Value > 0 && item.Key != game.currentRound.banker) {
				moveChipFromBankerToOtherArray[index++] = game.GetSeatIndex(item.Key);
			}
		}

		var playingPlayers = game.PlayingPlayers;
		for (int i = 0; i < playingPlayers.Count; i++) {
			playingPlayers [i].score = scoreDict [playingPlayers [i].userId];
		}

		//game.HideStateLabel();
		gamePlayController.state = GameState.CompareCard;
		moveToBanker = true;
	}

}
