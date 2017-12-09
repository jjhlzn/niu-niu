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
	public Vector2[] deltaVectorArray;

    void Start() {
		deltaVectorArray = new Vector2[8];
		deltaVectorArray [0] =	new Vector2 (-17, 0);
		deltaVectorArray [1] = new Vector2 (17, 8);
		deltaVectorArray [2] = new Vector2 (-19, -11);
		deltaVectorArray [3] = new Vector2 (-8, 17);
		deltaVectorArray [4] = new Vector2 (-23, 2);
		deltaVectorArray [5] = new Vector2 (-2, 8);
		deltaVectorArray [6] = new Vector2 (3, -13);
		deltaVectorArray [7] = new Vector2 (18, -3);
	}

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
				ShowChips (losers, winners);
			}
		}
	}
		
	private void ShowChips(List<string> loserIds, List<string> winnerIds) {
		if (loserIds.Count != 0) {
			int toSeatIndex = game.GetSeatIndex (game.currentRound.banker);
			for(int i =0; i < loserIds.Count; i++) {
				string id = loserIds [i];
				int fromSeatIndex = game.GetSeatIndex (id);
				if (i == loserIds.Count - 1) {
					ShowChips (fromSeatIndex, toSeatIndex, () => {
						MoveChipsForWinners (winnerIds);
					});
				} else {
					ShowChips (fromSeatIndex, toSeatIndex);
				}
			}
		} else {
			MoveChipsForWinners (winnerIds);
		}
	}
		
	private void MoveChipsForWinners(List<string> ids) {
		StartCoroutine (ExecMoveChipsForWinners (ids));
	}

	private IEnumerator ExecMoveChipsForWinners(List<string> userIds) {
		yield return new WaitForSeconds (.3f);

		int from = game.GetSeatIndex (game.currentRound.banker);
		if (userIds.Count != 0) {
			for(int i =0; i < userIds.Count; i++) {
				string id = userIds [i];
				int to = game.GetSeatIndex (id);
				Vector3 targetPosition = seats [to].chipImages [0].transform.position; //TODO: 总是到同一个位置

				if (i == userIds.Count - 1) {
					ShowChips (from, to, () => {
						ShowScoreLabels();
					});
				} else {
					ShowChips (from, to);
				}
			}
		} else {
			ShowScoreLabels();
		}
	}

	private delegate void Callback();

	private static float ChipShowInterval = 0.002f;
	public static float Move_Chip_Disapear_Interval = 0.2f;
	private void ShowChips( int fromSeatIndex,  int toSeatIndex, Callback callback = null) {
		Sequence s = DOTween.Sequence ();
		Vector3 targetPosition = seats [toSeatIndex].chipImages [0].transform.position; 
		int startIndex = toSeatIndex * SetupCardGame.Chip_Count_When_Transimit;
		for (int i = 0; i < SetupCardGame.Chip_Count_When_Transimit; i++) {
			Image chip = seats [fromSeatIndex].chipImages [startIndex + i];
			int chipIndex = i;
			s.Append(chip.DOFade(1, 0.01f).OnComplete( () => {
				chip.gameObject.SetActive(true);
	
				float max = 20f;
				chip.transform.position = new Vector3 (
					chip.transform.position.x + Random.Range(-1 * max, max) / SetupCardGame.TransformConstant,
					chip.transform.position.y + Random.Range(-1 * max, max) / SetupCardGame.TransformConstant);
			}));

			s.AppendInterval (ChipShowInterval);
		}

		s.OnComplete (() => {
			MoveChips(fromSeatIndex, toSeatIndex, callback);
		});
	}

	public static float Move_Chip_Move_Duration = 0.3f;
	public static float Move_Chip_Move_Interval = 0.04f;

	private void MoveChips(int fromSeatIndex,  int toSeatIndex, Callback callback = null) {

		int startIndex = toSeatIndex * SetupCardGame.Chip_Count_When_Transimit;
		Vector3 position = seats [toSeatIndex].chipImages [0].transform.position; 

		float delayBeforeMove = 0.15f;
		float delay = delayBeforeMove;

		for (int i = SetupCardGame.Chip_Count_When_Transimit - 1; i >= 0; i--) {
			float max = 18f;
			//随机生成移动的目标位置
			Vector3 target = new Vector3 (position.x + Random.Range (-1 * max, max) / SetupCardGame.TransformConstant,
				position.y + Random.Range (-1 * max, max) / SetupCardGame.TransformConstant);
			
			MusicController.instance.Play (AudioItem.TransmitCoin, allowRepeat: true);

			Image chip = seats [fromSeatIndex].chipImages [startIndex + i];
			delay += Random.Range (0, Move_Chip_Move_Interval * 1.5f);
			Tween t = chip.transform.DOMove (target, Move_Chip_Move_Duration).SetDelay(delay);
			int index = i;
			t.OnComplete (() => {
				chip.DOFade(1, 0.001f).SetDelay(Move_Chip_Disapear_Interval).OnComplete( () => {
					chip.gameObject.SetActive(false);
					if (index == SetupCardGame.Chip_Count_When_Transimit - 1) {
						if (callback != null)
							callback();
					}
				});
			});

		} 
	}


	private IEnumerator PlayMoveChipsAudio() {
		yield return new WaitForSeconds (0f);
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
		int thisRoundScore = gamePlayController.game.currentRound.resultDict [seats [index].player.userId];
		scoreLabel.text = Utils.GetNumberSring (thisRoundScore);
		if (thisRoundScore >= 0) {
			scoreLabel.color = new Color(253f / 255, 207f / 255, 15f / 255);
		} else {
			scoreLabel.color = new Color(63f / 255, 162f / 255, 211f / 255);
		}
		scoreLabel.gameObject.SetActive (true);
		Animator anim = scoreLabel.GetComponent<Animator> ();
		StartCoroutine(ShowScoreLabel(scoreLabel, anim, callback));

		int score = seats [index].player.score;
		seats [index].playerScoreLabel.DOText (Utils.GetNumberSring (score), .8f, true, ScrambleMode.Numerals);  
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
