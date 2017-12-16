using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CompareCardController : BaseStateController {
	private static float ChipShowInterval = 0.002f;
	public static float Move_Chip_Disapear_Interval = 0.2f;
	public static float Move_Chip_Move_Duration = 0.3f;
	public static float Move_Chip_Move_Interval = 0.05f;
	public static float Move_Score_Label_Duration = 0.8f;
	private delegate void Callback();

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


			//每三个三个传
			float thisDeay = 0;
			if (i % (int)Random.Range(2, 4) == 0) {
				delay += Random.Range (Move_Chip_Move_Interval * 0f, Move_Chip_Move_Interval * 2f);
				thisDeay = delay;
			} else {
				thisDeay = delay + Random.Range (Move_Chip_Move_Interval * 0f, Move_Chip_Move_Interval * 2f);
			}

			Tween t = chip.transform.DOMove (target, Move_Chip_Move_Duration).SetDelay(delay);
			int index = i;
			t.OnComplete (() => {
				Sequence s = DOTween.Sequence();
				s.AppendInterval(Move_Chip_Disapear_Interval)
					.OnComplete( () => {
						chip.gameObject.SetActive(false);
						if (index == 0) {
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
		for (int i = 0; i < playingPlayers.Count; i++) {
			if (i == playingPlayers.Count - 1) {
				ShowScoreLabel (playingPlayers[i].seat.seatIndex, MoveScoreLabels);
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
		for (int i = 0; i < playingPlayers.Count; i++) {
			if (i == playingPlayers.Count - 1) {
				MoveScoreLabel (playingPlayers[i].seat.seatIndex, SetCompareCardCompleted);
			} else {
				MoveScoreLabel (playingPlayers[i].seat.seatIndex);
			}
		}
	}

	private void SetCompareCardCompleted() {
		readyButton.gameObject.SetActive (true);
		if (game.HasNextRound ()) {
			if (game.state == GameState.CompareCard) {
				game.state = GameState.WaitForNextRound;
				waitForNextRoundController.Reset ();
			}
		} else {
			game.state = GameState.GameOver;
			if (gameOverController.isGetGameOverNotify) {
				gameOverController.HandleGameOverResponse ();
			}
		}
	}

	private void MoveScoreLabel(int index, Callback callback = null) {
		Text text = seats [index].scoreLabel;
		Tween t = text.transform.DOMove (seats [index].targetScoreLabelPosition, Move_Score_Label_Duration);
		if (callback != null) {
			t.OnComplete (() => {callback();});
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
		int count = 0;
		game.currentRound.resultDict = resultDict;
		this.resultDict = resultDict;
		this.scoreDict = scoreDict;

		gamePlayController.state = GameState.CompareCard;
		moveToBanker = true;
	}

	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

}
