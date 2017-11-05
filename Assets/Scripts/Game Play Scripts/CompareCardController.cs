using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompareCardController : BaseStateController {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private CheckCardController checkCardController;

	[SerializeField]
	private Button readyButton;

	private float chipMoveSpeed = 18f;
	private float scoreLabelMoveSpeed = 5f;

	private Text[] scoreLabels;
	private Vector3[] targetScoreLabelPositons;

	private Image[][] chipsArray;
	public int[] moveChipFromOtheToBankerArray;
	public int[] moveChipFromBankerToOtherArray;
	public bool moveToBanker;
	public bool moveFromBanker;
	public bool showScoreLabel;
	public bool moveScoreLabel;
	public bool allAnimCompleted;

	private float MoveTime = 2f;

	private float moveTimeLeft; 

	// Use this for initialization
	void Awake () {
		moveTimeLeft = MoveTime;
	}


	private void HideChips() {
		for (int i = 0; i < Game.SeatCount; i++) {
			for (int j = 0; j < chipsArray [i].Length; j++) {
				chipsArray [i] [j].gameObject.SetActive (false);
			}
		}
	}

	public override void Reset() {
	}
	
	// Update is called once per frame
	void Update () {

		if (gamePlayController.state == GameState.CompareCard && checkCardController.isAllPlayerShowCardAnimCompleted) {

			//Debug.Log ("moveToBanker = " + moveToBanker + ", moveFromBanker = " + moveFromBanker + ", showScoreLabel = " + showScoreLabel + ", moveTimeLeft = " + moveTimeLeft);
			if (moveToBanker) {
				if (moveChipFromOtheToBankerArray.Length == 0) {
					moveTimeLeft = MoveTime;
					moveToBanker = false;
					moveFromBanker = true;
					HideChips ();
				}

				for (int i = 0; i < moveChipFromOtheToBankerArray.Length; i++) {
					MoveChipsFromSeat (moveChipFromOtheToBankerArray [i]);
				}

				moveTimeLeft -= Time.deltaTime;

				if (moveTimeLeft <= 0) {
					moveToBanker = false;
					moveFromBanker = true;
					moveTimeLeft = MoveTime;
					HideChips ();
				}
				
			} 

			if (moveFromBanker) {
				if (moveChipFromBankerToOtherArray.Length == 0) {
					moveTimeLeft = MoveTime;
					moveFromBanker = false;
					showScoreLabel = true;
					HideChips ();
				}

				for (int i = 0; i < moveChipFromBankerToOtherArray.Length; i++) {
					MoveChipsToSeat (moveChipFromBankerToOtherArray [i]);
				}

				moveTimeLeft -= Time.deltaTime;

				if (moveTimeLeft <= 0) {
					moveFromBanker = false;
					moveTimeLeft = MoveTime;
					showScoreLabel = true;
					HideChips ();
					//Debug.Log ("Set showScoreLabel to true");
				}
			}


			if (showScoreLabel) {
				HideChips ();
				showScoreLabel = false;
				//Debug.Log ("showScoreLabel = " + showScoreLabel);
				ShowScoreLabels ();
				moveTimeLeft = MoveTime;
			}

			if (moveScoreLabel) {
				//Debug.Log ("moveScoreLabel = " + moveScoreLabel);
				MoveScoreLabels ();
				if (moveTimeLeft < 0) {
					moveScoreLabel = false;
					allAnimCompleted = true;
				}
				moveTimeLeft -= Time.deltaTime;
			}

	
		} 
	}

	private void ShowScoreLabels() {
		Seat[] seats = gamePlayController.game.seats;
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer()) {
				ShowScoreLabel (i);
			}
		}
	}

	private void ShowScoreLabel(int index) {
		scoreLabels [index].gameObject.SetActive (true);
		Animator anim = scoreLabels [index].GetComponent<Animator> ();
		StartCoroutine(ShowScoreLabel(scoreLabels[index], anim));
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
		Text text = scoreLabels [index];
		text.transform.position = Vector3.MoveTowards (text.transform.position, targetScoreLabelPositons [index], scoreLabelMoveSpeed * Time.deltaTime);

		if (Utils.isTwoPositionIsEqual(text.transform.position, targetScoreLabelPositons[index])) {
			text.gameObject.SetActive (false);
			readyButton.gameObject.SetActive (true);
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


	public void SetScoreLabels(Text[] scoreLabels) {
		this.scoreLabels = scoreLabels;
		targetScoreLabelPositons = new Vector3[this.scoreLabels.Length];
		for (int i = 0; i < this.scoreLabels.Length; i++) {
			Vector3 v = this.scoreLabels [i].transform.position;
			targetScoreLabelPositons [i] = new Vector3(v.x, v.y + 1.5f, v.z);
		}
	}

	//private Dictionary<string, bool> moveChipFunctionDict = new Dictionary<string, bool>();
	private void MoveChips( int from,  int to) {
		
		float step = chipMoveSpeed * Time.deltaTime;

		float waitTime = 0;

		Game game = gamePlayController.game;
		Vector3 targetPosition = chipsArray [to] [0].transform.position; //TODO: 总是到同一个位置

		bool moveCompleted = true;

		int startIndex = to * 8;
		for (int i = to * 8; i < to * 8 + 8; i++) {
			Image image = chipsArray [from] [i];
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

		Debug.Log ("Go to compare card state");
		Debug.Log ("moveToBanker = " + moveToBanker);
		gamePlayController.state = GameState.CompareCard;
	}

	public void SetChipsArray(Image[][] chipsArray) {
		this.chipsArray = chipsArray;
	}


}
