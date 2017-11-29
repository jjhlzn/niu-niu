using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseBankerController : BaseStateController {
	public static int ChooseTotalCount = 50;
	private float BankerSignMoveTimeInterval = .002f;
	private float moveBankerSignSpeed = 13f;

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private Image bankerSign;
	//private Vector3 bankerSignOriginPosition;

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
		//bankerSignOriginPosition = bankerSign.gameObject.transform.position;
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

	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	// Update is called once per frame
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

	public void SetUI() {
		var game = gamePlayController.game;
		int seatIndex = game.GetSeatIndex (game.currentRound.banker);
		bankerSign.gameObject.transform.position = seats [seatIndex].bankerSignPosition;
		bankerSign.gameObject.SetActive (true);
	}

	IEnumerator SetRobingBorderImage() {
		yield return new WaitForSeconds (.5f);
		foreach (Seat seat in seats) {
			seat.robingSeatBorderImage.gameObject.SetActive (false);
		}
	}


	private void ChooseBankerAnimation() {
		var game = gamePlayController.game;
		if (isChoosingBanker) {
			timeLeft -= Time.deltaTime;


			if (timeLeft < 0) {
				List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;
				for (int i = 0; i < playingPlayers.Count; i++) {
					Player player = playingPlayers [i];
					if (player.userId == userIds [chooseIndex] && isPlayerRobBanker(game.currentRound.robBankerPlayers, player.userId)) {
						MusicController.instance.Play (AudioItem.RandomSelectBanker);
						player.seat.robingSeatBorderImage.gameObject.SetActive (true);
					} else {
						player.seat.robingSeatBorderImage.gameObject.SetActive (false);
					}
				}

				if (chooseCount > ChooseTotalCount
					&& seats [game.GetSeatIndex (userIds [chooseIndex])].player.userId == game.currentRound.banker) {
					Debug.Log ("choose banker compeleted");
					/*
					foreach (Seat seat in seats) {
						seat.robingSeatBorderImage.gameObject.SetActive (false);
					}*/
					StartCoroutine (SetRobingBorderImage ());
					isChoosingBanker = false;
					chooseCompleted = true;

				} else {
					timeLeft = BankerSignMoveTimeInterval;
					chooseIndex = ++chooseIndex % userIds.Length;
					chooseCount++;
				}
			} 
		}

		if (chooseCompleted) {
			Debug.Log ("show banker choose completed animation");
			StartCoroutine (ChooseBankerCompletedAnimation ());
		}

		if (movingBankerSign) {

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

				//动画执行玩了，看看状态还需要切换，因为动画可能以前落后游戏进度
				if (game.state == GameState.ChooseBanker) {
					game.HideStateLabel ();
					game.state = GameState.Bet;
				}
			}
		} 
	}

	IEnumerator ChooseBankerCompletedAnimation() {
		var game = gamePlayController.game;

		game.ShowStateLabel (game.currentRound.banker + "成为庄家");

		chooseCompleted = false;
		bankerSign.gameObject.transform.position = new Vector3 (game.gameStateLabel.transform.position.x - (game.gameStateLabel.preferredWidth + 20) / SetupCardGame.TransformConstant / 2,
			game.gameStateLabel.transform.position.y / SetupCardGame.TransformConstant, 0);
		bankerSign.gameObject.SetActive (true);

		yield return new WaitForSeconds (.4f);

		movingBankerSign = true;
	}
		
	public void HandleResponse(GoToChooseBankerNotity resp) {
		var game = gamePlayController.game;

		gamePlayController.state = GameState.ChooseBanker;
		game.currentRound.banker = resp.banker;
		game.currentRound.robBankerPlayers = resp.robBankerPlayers;


		userIds = new string[game.PlayerCount];

		int index = 0;
		for (int i = 0; i < Game.SeatCount; i++) {
			if (seats [i].hasPlayer ()) {
				userIds [index++] = seats[i].player.userId;
			}
			seats [i].HideIsRobImage ();
		}
		game.HideStateLabel ();
		chooseIndex = 0;

		if (game.currentRound.robBankerPlayers.Length >= 2 || game.currentRound.robBankerPlayers.Length == 0)
			isChoosingBanker = true;
		else
			chooseCompleted = true;
	}
		
		
}
