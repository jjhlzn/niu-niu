using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CompareCardController : BaseStateController {
	private float chipMoveSpeed = 20f;
	private float scoreLabelMoveSpeed = 5f;
	private float MoveTime = 2f;

	[Header("Controller")]
	[SerializeField]
	private GamePlayController gamePlayController;
	[SerializeField]
	private CheckCardController checkCardController;
	[SerializeField]
	private WaitForNextRoundController waitForNextRoundController;
	[SerializeField]
	private GameOverController gameOverController;

	[Header("UI")]
	[SerializeField]
	private Button readyButton;

		
	private bool moveToBanker;
	Dictionary<string, int>  resultDict;
	Dictionary<string, int> scoreDict;

	public void Init() {}

	public override void Reset() {}
		
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
			if (moveToBanker) {
				moveToBanker = false;

				for (int i = 0; i < playingPlayers.Count; i++) {
					playingPlayers [i].score = scoreDict [playingPlayers [i].userId];
					Debug.Log (playingPlayers [i].userId + ": " + scoreDict [playingPlayers [i].userId]);
				}

				List<string> losers = new List<string> ();
				List<string> winners = new List<string> (); 
				foreach (var item in resultDict) {
					if (item.Key != game.currentRound.banker) {
						if (item.Value < 0) {
							losers.Add (item.Key);
						}
						if (item.Value > 0) {
							winners.Add (item.Key);
						}
					}
				}
				MoveChips (losers, winners);
			}
		}
	}
		
	private void MoveChips(List<string> loserIds, List<string> winnerIds) {
		if (loserIds.Count != 0) {
			int to = game.GetSeatIndex (game.currentRound.banker);
			Vector3 targetPosition = seats [to].chipImages [0].transform.position; //TODO: 总是到同一个位置

			for(int i =0; i < loserIds.Count; i++) {
				string id = loserIds [i];
				int from = game.GetSeatIndex (id);
				if (i == loserIds.Count - 1) {
					MoveChips (from, to, () => {
						MoveChipFromBankerToPlayers (winnerIds);
					});
				} else {
					MoveChips (from, to);
				}
			}
			StartCoroutine (PlayMoveChipsAudio ());
		} else {
			MoveChipFromBankerToPlayers (winnerIds);
		}
	}
		
	private void MoveChipFromBankerToPlayers(List<string> ids) {
		StartCoroutine (ExecMoveChipFromBankerToPlayers (ids));
	}

	private IEnumerator ExecMoveChipFromBankerToPlayers(List<string> userIds) {
		Debug.Log ("MoveChipFromBankerToPlayers() called");
		yield return new WaitForSeconds (.3f);

		int from = game.GetSeatIndex (game.currentRound.banker);

		if (userIds.Count != 0) {
			for(int i =0; i < userIds.Count; i++) {
				string id = userIds [i];
				int to = game.GetSeatIndex (id);
				Vector3 targetPosition = seats [to].chipImages [0].transform.position; //TODO: 总是到同一个位置

				if (i == userIds.Count - 1) {
					MoveChips (from, to, () => {
						ShowScoreLabels();
					});
				} else {
					MoveChips (from, to);
				}
			}
			MusicController.instance.Stop(AudioItem.TransmitCoin);
			StartCoroutine (PlayMoveChipsAudio ());
		} else {
			ShowScoreLabels();
		}
	}

	private delegate void Callback();

	private void MoveChips( int from,  int to, Callback calback = null) {
		Vector3 targetPosition = seats [to].chipImages [0].transform.position; //TODO: 总是到同一个位置
		int startIndex = to * SetupCardGame.Chip_Count_When_Transimit;

		for (int i = 0; i < SetupCardGame.Chip_Count_When_Transimit; i++) {
			Image image = seats [from].chipImages [startIndex + i];
			if (!image.gameObject.activeInHierarchy)
				image.gameObject.SetActive (true);

			Tween t = image.transform.DOMove (targetPosition, 0.5f).SetDelay(0.1f + 0.1f * i);
			if (i == SetupCardGame.Chip_Count_When_Transimit - 1) {
				if (calback != null) {
					t.OnComplete (() => {
						HideChips ();
						calback ();
					});
				}
			}
		}
	}

	private IEnumerator PlayMoveChipsAudio() {
		yield return new WaitForSeconds (0.1f);
		MusicController.instance.Play (AudioItem.TransmitCoin);
	}

	private void ShowScoreLabels() {
		Debug.Log ("ShowScoreLabels() called");
		Debug.Log ("playingPlayers.Count = " + playingPlayers.Count);
		for (int i = 0; i < playingPlayers.Count; i++) {
			if (i == playingPlayers.Count - 1) {
				ShowScoreLabel (playingPlayers[i].seat.seatIndex, () => {
					MoveScoreLabels();
				});
			} else {
				ShowScoreLabel (playingPlayers[i].seat.seatIndex);
			}
		}
	}

	private void ShowScoreLabel(int index, Callback callback = null) {
		Text scoreLabel = seats [index].scoreLabel;
		//Debug.Log ("userId: " + seats [index].player.userId);
		int thisRoundScore = gamePlayController.game.currentRound.resultDict [seats [index].player.userId];
		scoreLabel.text = Utils.GetNumberSring (thisRoundScore);

		int score = seats [index].player.score;
		Debug.Log ("score = " + score);
		seats [index].playerScoreLabel.DOText (Utils.GetNumberSring (score), .8f, true, ScrambleMode.Numerals);  

		scoreLabel.gameObject.SetActive (true);
		Animator anim = scoreLabel.GetComponent<Animator> ();
		StartCoroutine(ShowScoreLabel(scoreLabel, anim, callback));
	}


	IEnumerator ShowScoreLabel(Text text, Animator anim, Callback callback = null) {
		anim.Play("ShowUp");
		yield return new WaitForSeconds (.4f);
		if (callback != null)
			callback ();
	}

	private void MoveScoreLabels () {
		Seat[] seats = gamePlayController.game.seats;
		var playingPlayers = gamePlayController.game.PlayingPlayers;
		for (int i = 0; i < playingPlayers.Count; i++) {
			if (i == playingPlayers.Count - 1) {
				MoveScoreLabel (playingPlayers[i].seat.seatIndex, () => {
					readyButton.gameObject.SetActive (true);

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
					Debug.Log ("Go to " + gamePlayController.state.value);
				});
			} else {
				MoveScoreLabel (playingPlayers[i].seat.seatIndex);
			}
		}
	}

	private void MoveScoreLabel(int index, Callback callback = null) {
		Text text = seats [index].scoreLabel;
		Tween t = text.transform.DOMove (seats [index].targetScoreLabelPosition, 0.5f);
		if (callback != null) {
			t.OnComplete (() => {
				callback ();
			});
		}
	}
		
	IEnumerator moveChip(Image image, Vector3 to, float waitTime, float step) {
		yield return new WaitForSeconds (waitTime);
		//Debug.Log ("waitTime = " + waitTime);
		image.transform.position = Vector3.MoveTowards (image.transform.position, to, step);
	} 


	/******************************  Hanle Response   *********************************/
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
		this.resultDict = resultDict;
		this.scoreDict = scoreDict;
			
		//game.HideStateLabel();
		gamePlayController.state = GameState.CompareCard;
		moveToBanker = true;
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

}
