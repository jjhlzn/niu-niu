using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompareCardController : MonoBehaviour {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private CheckCardController checkCardController;

	private float chipMoveSpeed = 14f;

	private Image[][] chipsArray;
	public int[] moveChipFromOtheToBankerArray;
	public int[] moveChipFromBankerToOtherArray;
	public bool moveToBanker;
	public bool moveFromBanker;
	public bool moveCompleted;

	private float MoveTime = 100f;

	private float moveTimeLeft; 

	// Use this for initialization
	void Awake () {
		moveTimeLeft = MoveTime;
	}

	
	// Update is called once per frame
	void Update () {

		if (gamePlayController.state == GameState.CompareCard && checkCardController.isAllPlayerShowCardAnimCompleted) {

			if (moveToBanker) {
				if (moveChipFromOtheToBankerArray.Length == 0) {
					moveTimeLeft = MoveTime;
					moveToBanker = false;
					moveFromBanker = true;
				}

				for (int i = 0; i < moveChipFromOtheToBankerArray.Length; i++) {
					MoveChipsFromSeat (moveChipFromOtheToBankerArray[i]);
				}

				moveTimeLeft -= Time.deltaTime;

				if (moveTimeLeft <= 0) {
					moveToBanker = false;
					moveFromBanker = true;
					moveTimeLeft = MoveTime;
				}
				
			} else if (moveFromBanker) {
				if (moveChipFromBankerToOtherArray.Length == 0) {
					moveFromBanker = false;
					moveCompleted = true;
				}

				for (int i = 0; i < moveChipFromBankerToOtherArray.Length; i++) {
					MoveChipsToSeat (moveChipFromBankerToOtherArray[i]);
				}

				moveTimeLeft -= Time.deltaTime;

				if (moveTimeLeft <= 0) {
					moveToBanker = false;
					moveTimeLeft = MoveTime;
					moveCompleted = true;
				}
			}
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


	//private Dictionary<string, bool> moveChipFunctionDict = new Dictionary<string, bool>();
	private void MoveChips(int from,  int to) {
		
		float step = chipMoveSpeed * Time.deltaTime;

		float waitTime = 0;

		Game game = gamePlayController.game;
		Vector3 targetPosition = chipsArray [to] [0].transform.position; //TODO: 总是到同一个位置

		bool moveCompleted = true;
		for (int i = 0; i < chipsArray [from].Length; i++) {
			Image image = chipsArray [from] [i];
			if (!image.gameObject.activeInHierarchy)
				image.gameObject.SetActive (true);
			//image.transform.position = Vector3.MoveTowards (image.transform.position, targetPosition, step);

	
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
