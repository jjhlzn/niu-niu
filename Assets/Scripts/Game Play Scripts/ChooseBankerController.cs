using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChooseBankerController : BaseStateController {
	public static int ChooseTotalCount = 50;
	private float BankerSignMoveTimeInterval = .002f;
	private static float Move_Banker_Sign_Duration = 0.5f;

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private Image bankerSign;


	private string[] randomSelectBankerUserIds;
	private bool isChoosingBanker;
	private int chooseIndex;
	private int chooseCount;
	private float timeLeft;

	public void Init() {
		bankerSign.gameObject.SetActive (false);
	}

	public override void Reset() {
		randomSelectBankerUserIds = new string[0];
		isChoosingBanker = false;
		chooseIndex = 0;
		chooseCount = 0;
		timeLeft = BankerSignMoveTimeInterval;
	}

	bool isPlayerRobBanker(string[] robBankerPlayers, string playerId) {
		if (robBankerPlayers.Length == 0) {
			return true;
		}
		for (int i = 0; i < robBankerPlayers.Length; i++) {
			if (playerId == robBankerPlayers [i])
				return true;
		}
		return false;
	}
		
	public new void Update ()  {
		base.Update ();
		var game = gamePlayController.game;
		if (game.state == GameState.ChooseBanker && isChoosingBanker) {
			if (game.currentRound.robBankerPlayers.Length >= 2) {
				game.ShowStateLabel ("多人抢庄，现在随机抢庄");
			} else {
				game.ShowStateLabel ("无人抢庄，随机选择一个庄家");
			}
		}
		ChooseBankerAnimation ();
	}

	private void ChooseBankerAnimation() {
		var game = gamePlayController.game;
		if (isChoosingBanker) {
			isChoosingBanker = false;
			MusicController.instance.Play (AudioItem.RandomSelectBanker);
			ShowRobingBorder ();
		}
	}

	private bool IsStopRandomSelect() {
		if (randomSelectBankerUserIds.Length < 2)
			return true;

		int lastChooseIndex = (chooseIndex - 1 + randomSelectBankerUserIds.Length) % randomSelectBankerUserIds.Length;
		if (seats [game.GetSeatIndex (randomSelectBankerUserIds [lastChooseIndex])].player.userId == game.currentRound.banker
		    && chooseCount >= ChooseTotalCount)
			return true;

		return false;
	}

	private void ShowRobingBorder(float delay = 0f) {
		if ( !IsStopRandomSelect() ) {
			int seatIndex = game.GetSeatIndex (randomSelectBankerUserIds [chooseIndex]);
			//Debug.Log ("seatIndex = " + seatIndex);
			Seat seat = seats [seatIndex];
			Sequence s = DOTween.Sequence ();
			s.AppendInterval(delay)
				.OnComplete (() => {
					for (int i = 0; i < playingPlayers.Count; i++) {
						Player player = playingPlayers [i];
						if (player.userId == randomSelectBankerUserIds [chooseIndex]) {
							player.seat.robingSeatBorderImage.gameObject.SetActive (true);
						} else {
							player.seat.robingSeatBorderImage.gameObject.SetActive (false);
						}
					}
					chooseIndex = ++chooseIndex % randomSelectBankerUserIds.Length;
					chooseCount++;
					ShowRobingBorder(BankerSignMoveTimeInterval);
				});
		} else {
			MusicController.instance.Stop (AudioItem.RandomSelectBanker);
			StartCoroutine (ChooseBankerCompletedAnimation ());
		}
	}
		
	private void MoveBankerSign() {
		int bankerSeatIndex = game.GetSeatIndex (gamePlayController.game.currentRound.banker);
		Vector3 targetPosition = seats[bankerSeatIndex].bankerSignPosition;

		bankerSign.gameObject.transform
			.DOMove (targetPosition, Move_Banker_Sign_Duration)
			.OnComplete (() => {
				foreach(Player player in playingPlayers) {
					player.seat.isRobImage.gameObject.SetActive(false);
				}

				Sequence s = DOTween.Sequence();
				s.AppendInterval(0.3f)
					.OnComplete( () => {
						seats [bankerSeatIndex].robingSeatBorderImage.gameObject.SetActive (false);
				});
				
				//动画执行玩了，看看状态还需要切换，因为动画可能以前落后游戏进度
				if (game.state == GameState.ChooseBanker) {
					game.state = GameState.Bet;
				}
			});
	}

	IEnumerator ChooseBankerCompletedAnimation() {

		int bankerSeatIndex = game.GetSeatIndex (game.currentRound.banker);
		game.ShowStateLabel (game.seats[bankerSeatIndex].player.nickname + "成为庄家");

		bankerSign.gameObject.transform.position = new Vector3 (game.gameStateLabel.transform.position.x - (game.gameStateLabel.preferredWidth + 20) / SetupCardGame.TransformConstant / 2,
			game.gameStateLabel.transform.position.y / SetupCardGame.TransformConstant, 0);
		bankerSign.gameObject.SetActive (true);

		yield return new WaitForSeconds (.5f);
		MoveBankerSign ();
	}
		
	public void HandleResponse(GoToChooseBankerNotity resp) {

		gamePlayController.state = GameState.ChooseBanker;
		game.currentRound.banker = resp.banker;
		game.currentRound.robBankerPlayers = resp.robBankerPlayers;


		randomSelectBankerUserIds = game.currentRound.robBankerPlayers;
		if (randomSelectBankerUserIds.Length == 0) {
			randomSelectBankerUserIds = new string[game.PlayingPlayers.Count];
			for (int i = 0; i < playingPlayers.Count; i++) {
				randomSelectBankerUserIds [i] = playingPlayers [i].userId;
			}
		}
			
		Debug.Log ("randomSelectBankerUserIds.Length = " + randomSelectBankerUserIds.Length);
		isChoosingBanker = true;
		game.HideStateLabel ();
	}

	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}
		
	public void SetUI() {
		var game = gamePlayController.game;
		int seatIndex = game.GetSeatIndex (game.currentRound.banker);
		bankerSign.gameObject.transform.position = seats [seatIndex].bankerSignPosition;
		bankerSign.gameObject.SetActive (true);
	}
		
}
