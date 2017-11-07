﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseBankerController : BaseStateController {
	public static int ChooseTotalCount = 15;
	private float BankerSignMoveTimeInterval = .03f;
	private float moveBankerSignSpeed = 10f;

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private Image bankerSign;
	private Vector3 bankerSignOriginPosition;

	private Seat[] seats;

	private bool chooseCompleted;
	private string[] userIds;
	private bool isChoosingBanker;
	private int chooseIndex;
	private int chooseCount;
	private float waitTime;
	private float timeLeft;
	private bool movingBankerSign;
	private bool isShowStateLabel;

	void Awake() {
		bankerSignOriginPosition = bankerSign.gameObject.transform.position;
	}

	void Start() {
	}

	public void Init() {
		seats = gamePlayController.game.seats;
		bankerSign.gameObject.SetActive (false);
	}

	public override void Reset() {
		chooseCompleted = false;
		userIds = null;
		isChoosingBanker = false;
		chooseIndex = -1;
		chooseCount = -1;
		timeLeft = BankerSignMoveTimeInterval;
		waitTime = 0f;
		movingBankerSign = false;
	}

	void Update() {

		if (isChoosingBanker) {
			
			timeLeft -= Time.deltaTime;
			if (timeLeft < 0) {
				List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;
				for (int i = 0; i < playingPlayers.Count; i++) {
					Player player = playingPlayers [i];
					if (player.userId == userIds [chooseIndex]) {
						player.seat.robingSeatBorderImage.gameObject.SetActive (true);
					} else {
						player.seat.robingSeatBorderImage.gameObject.SetActive (false);
					}
				}
					

				if (chooseCount > ChooseTotalCount
				    && seats [gamePlayController.game.GetSeatIndex (userIds [chooseIndex])].player.userId == gamePlayController.game.currentRound.banker) {

					isChoosingBanker = false;
					foreach (Seat seat in seats) {
						seat.robingSeatBorderImage.gameObject.SetActive (false);
					}
					Game game = gamePlayController.game;
					game.ShowStateLabel (game.currentRound.banker + "成为庄家");
					movingBankerSign = true;
					bankerSign.gameObject.transform.position = new Vector3 (game.gameStateLabel.transform.position.x - game.gameStateLabel.preferredWidth / SetupCardGame.TransformConstant / 2,
						game.gameStateLabel.transform.position.y, 0);
					bankerSign.gameObject.SetActive (true);
				} else {
					timeLeft = BankerSignMoveTimeInterval;
					chooseIndex = ++chooseIndex % userIds.Length;
					chooseCount++;
				}
			} else {
				gamePlayController.game.ShowStateLabel ("多人抢庄，现在随机抢庄");
			}
		}

		if (movingBankerSign) {
			//int seatIndex = gamePlayController.game.GetSeatIndex (userIds[chooseIndex]);

			int bankerSeatIndex = gamePlayController.game.GetSeatIndex (gamePlayController.game.currentRound.banker);
			Vector3 targetPosition = seats[bankerSeatIndex].bankerSignPosition;
			float step = moveBankerSignSpeed * Time.deltaTime;
			bankerSign.gameObject.transform.position = Vector3.MoveTowards(bankerSign.transform.position, targetPosition, step);

			if (Utils.isTwoPositionIsEqual(bankerSign.gameObject.transform.position , targetPosition)) {
				Debug.Log ("banker sing move over");
				foreach (Seat seat in seats) {
					seat.isRobImage.gameObject.SetActive (false);
				}
				movingBankerSign = false;
				gamePlayController.game.HideStateLabel ();
				gamePlayController.goToNextState ();
			}
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
		gamePlayController.game.HideStateLabel ();
		chooseIndex = 0;
		isChoosingBanker = true;

	}


		
}
