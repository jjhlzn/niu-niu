using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseBankerController : MonoBehaviour {
	public static int ChooseTotalCount = 15;
	private float timeInterval = .03f;
	private float moveBankerSignSpeed = 16f;

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private Image bankerSign;
	private Vector3 bankerSignOriginPosition;


	private Image[] robingImages;
	private Image[] bankerSignPositions;
	private Image[] isRobImages;

	private bool chooseCompleted;
	private string[] userIds;
	private bool choosing;
	private int chooseIndex;
	private int chooseCount;
	private float waitTime;
	private float timeLeft;

	private bool movingBankerSign;

	void Awake() {
		bankerSignOriginPosition = bankerSign.gameObject.transform.position;
	}

	void Update() {

		Seat[] seats = gamePlayController.game.seats;
		if (choosing) {
			
				
			timeLeft -= Time.deltaTime;
			if ( timeLeft < 0 )
			{
				for (int i = 0; i < Game.SeatCount; i++) {
					if (seats [i].hasPlayer()) {

						if (seats [i].player.userId == userIds [chooseIndex]) {
							robingImages [i].gameObject.SetActive (true);
						} else {
							robingImages [i].gameObject.SetActive (false);
						}
					} else {
						robingImages [i].gameObject.SetActive (false);
					}
				}

				if (chooseCount > ChooseTotalCount
					&& seats [gamePlayController.game.GetSeatIndex(userIds[chooseIndex])].player.userId == gamePlayController.game.currentRound.banker) {
					choosing = false;
					foreach (Image image in robingImages) {
						image.gameObject.SetActive (false);
					}

					movingBankerSign = true;
					bankerSign.gameObject.SetActive (true);
				} else {
					timeLeft = timeInterval;
					chooseIndex = ++chooseIndex % userIds.Length;
					chooseCount++;
				}
			}
		}

		if (movingBankerSign) {
			int seatIndex = gamePlayController.game.GetSeatIndex (userIds[chooseIndex]);
			Vector3 targetPosition = bankerSignPositions [seatIndex].transform.position;
			float step = moveBankerSignSpeed * Time.deltaTime;
			bankerSign.gameObject.transform.position = Vector3.MoveTowards(bankerSign.transform.position, targetPosition, step);

			if (Utils.isTwoPositionIsEqual(bankerSign.gameObject.transform.position , targetPosition)) {
				Debug.Log ("banker sing move over");
				foreach (Image image in isRobImages) {
					image.gameObject.SetActive (false);
				}
				movingBankerSign = false;

				gamePlayController.goToNextState ();

			}
		} 
	}


	public void Reset() {
		bankerSign.gameObject.SetActive (false);
		choosing = false;
		movingBankerSign = false;
		timeLeft = timeInterval;
		waitTime = 0f;
		chooseIndex = -1;
		foreach (Image image in robingImages) {
			image.gameObject.SetActive (false);
		}
	}

	public void HandleResponse(GoToChooseBankerNotity resp) {
		gamePlayController.state = GameState.ChooseBanker;
		gamePlayController.game.currentRound.banker = resp.banker;


		userIds = new string[gamePlayController.game.PlayerCount];
		Seat[] seats = gamePlayController.game.seats;

		int index = 0;
		for (int i = 0; i < Game.SeatCount; i++) {
			if (seats [i].hasPlayer ()) {
				userIds [index++] = seats[i].player.userId;
			}
		}

		Debug.Log ("userIds = " + userIds);
		for (int i = 0; i < userIds.Length; i++) {
			Debug.Log ("user" + i + " = " + userIds [i]);
		}

		chooseIndex = 0;
		choosing = true;
		//gamePlayController.goToNextState ();

	}

	public void SetRobingImages(Image[] robingImages) {
		this.robingImages = robingImages;
	}

	public void SetIsRobImages(Image[] isRobImages) {
		this.isRobImages = isRobImages;
	}

	public void SetBankerSignPositions(Image[] bankerSignPositions) {
		this.bankerSignPositions = bankerSignPositions;
	}
		
}
