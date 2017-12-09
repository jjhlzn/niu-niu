using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class FirstDealerController : BaseStateController {
	public static float dealSpeed = 1000f; //发牌速度
	public static float dealWaitTimeBetweenPlayer = 0.3f;
	public static float dealWaitTimeBetweenPlayerForSecondDeal = 0.1f;
	public static float waitTimeDeltaBetweenCard = 0.07f;
	public static float turnUpTime = 0.5f;
	public static float user0CardScale = 1.32f;

	[SerializeField]
	private GamePlayController gamePlayController;
	[SerializeField]
	private BeforeGameStartController beforeGameStartController;
	[SerializeField]
	private GameObject deckCardPosition; //发牌位置
	[SerializeField]
	private Button shareButton;

	private bool isFirstDealing;
	public bool isFirstDealDone;

	public void Init() {
		isFirstDealing = false;
		isFirstDealDone = false;
	}

	public override void Reset() {
		isFirstDealing = false;
		isFirstDealDone = false;
	}
		
	// Update is called once per frame
	public new void Update ()  {
		base.Update ();

		if (gamePlayController.state == GameState.FirstDeal && gamePlayController.game.IsStateLabelVisible()) {
			gamePlayController.game.HideStateLabel ();
		} 

		FirstDealAnimation ();
	}

	private void FirstDealAnimation() {
		if (isFirstDealing && !beforeGameStartController.isMoveSeat) {
			isFirstDealing = false;
			MusicController.instance.Play (AudioItem.BeforeFirstDeal);
			StartCoroutine (ExecuteFirstDealAnimation ());
		}
	}

	private IEnumerator ExecuteFirstDealAnimation() {
		var game = gamePlayController.game;
		var round = game.currentRound;
		yield return new WaitForSeconds(.6f);

		MusicController.instance.Play (AudioItem.Deal, isLoop: true);

		List<Player> playingPlayers = game.PlayingPlayers;
		for (int i = 0; i < playingPlayers.Count; i++) {
			for (int j = 0; j < 4; j++) {

				Vector3 targetCard = playingPlayers[i].seat.cardPositions [j];
				Image[] cards = playingPlayers [i].cards;

				int index = i * 4 + j;

				Tween t = cards [j].transform.DOLocalMove (targetCard, dealSpeed, false)
					.SetSpeedBased ()
					.SetDelay (index * waitTimeDeltaBetweenCard);

				if (i == playingPlayers.Count - 1 & j == 3) {
					t.OnComplete (() => {
						MusicController.instance.Stop (AudioItem.Deal);
						if (Player.Me.isPlaying) {
							StartCoroutine (TurnCardUp (playingPlayers [0].seat.cards [0], round.playerCardsDict[Player.Me.userId] [0]));
							StartCoroutine (TurnCardUp (playingPlayers [0].seat.cards [1], round.playerCardsDict[Player.Me.userId] [1]));
							StartCoroutine (TurnCardUp (playingPlayers [0].seat.cards [2], round.playerCardsDict[Player.Me.userId] [2]));
							StartCoroutine (TurnCardUp (playingPlayers [0].seat.cards [3], round.playerCardsDict[Player.Me.userId] [3]));
						}
						isFirstDealing = false;
						isFirstDealDone = true;
						StartCoroutine (GoToNextState ());
					});
				}

				if (i == 0)
					cards [j].transform
						.DOScale (1.3f, 0.04f)
						.SetDelay (index * waitTimeDeltaBetweenCard + 0.02f);
			}
		}
	}
		
	IEnumerator TurnCardUp(Image card, string cardValue) {
		if (!string.IsNullOrEmpty (cardValue)) {
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("TurnUp");
			yield return new WaitForSeconds (turnUpTime);
			card.sprite = deck.GetCardFaceImage (cardValue);
			anim.Play ("TurnBackNow2");

		}
	    yield return new WaitForSeconds (turnUpTime);
	}
		
	IEnumerator GoToNextState() {
		yield return new WaitForSeconds (.3f);
		if (gamePlayController.state == GameState.FirstDeal)
			gamePlayController.state = GameState.RobBanker;
	}

	/******* 处理服务器的通知***************/
	public void HandleResponse(FirstDealResponse notify) {
		Dictionary<string, string[]> cardsDict = notify.cardsDict;
		Dictionary<string, int[]> betsDict = notify.betsDict;
		int roundNo = notify.roundNo;
		gamePlayController.game.currentRoundNo = roundNo;
		gamePlayController.PrepareForNewRound();
		HandleResponse (cardsDict, betsDict);
	}

	public void HandleResponse(StartGameNotify notify) {
		Dictionary<string, string[]> cardsDict = notify.cardsDict;
		Dictionary<string, int[]> betsDict = notify.betsDict;
		HandleResponse (cardsDict, betsDict);
	}
		

	private void HandleResponse(Dictionary<string, string[]> cardsDict, Dictionary<string, int[]> betsDict) {
		var game = gamePlayController.game;
		beforeGameStartController.UpdateButtonStatusAfterStart ();

		//更新位置UI
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer ()) {
				seats [i].player.isReady = false;
				seats [i].UpdateUI (gamePlayController.game);
				seats [i].scoreLabel.transform.position = seats [i].originScoreLabelPosition;
			}
		}

		foreach (string uid in cardsDict.Keys) {
			seats [game.GetSeatIndex (uid)].player.isPlaying = true;
		}

		string[] cards;
		string userId = seats [0].player.userId;
		Debug.Log ("userId = " + userId);
		if (cardsDict.ContainsKey (userId)) {
			cards = cardsDict [userId];
		} else {
			throw new UnityException ("找不到UserId = " + userId + "的牌");
		}

		int[] bets;
		if (betsDict.ContainsKey (userId)) {
			bets = betsDict [userId];
		} else {
			throw new UnityException ("找不到UserId = " + userId + "的可下注的筹码量");
		}
		gamePlayController.game.currentRound.playerCardsDict [userId] = new string[5];
		for (int i = 0; i < cards.Length; i++) {
			gamePlayController.game.currentRound.playerCardsDict[userId][i] = cards[i];
		}

		gamePlayController.game.currentRound.myBets = bets;

		gamePlayController.game.StartGame ();

		deck.ShowNotDealCardsForFirstDeal (gamePlayController.game.PlayingPlayers.Count);

		//先把数据结构设置好，再在Update()中执行发牌的动画。
		game.FirstDeal ();

		gamePlayController.state = GameState.FirstDeal;
		gamePlayController.game.UpdateGameInfos ();
		if (beforeGameStartController.IsNeedMoveSeat()) {
			beforeGameStartController.MoveSeats (beforeGameStartController.getMoveSeatIndex ());
		}
		isFirstDealing = true;
	}

	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	public void SetUI() {
		var game = gamePlayController.game;
		game.FirstDeal ();
		List<Player> playingPlayers = game.PlayingPlayers;
		for (int i = 0; i < playingPlayers.Count; i++) {
			Player player = playingPlayers [i];
			Image[] cards = player.seat.cards;
			Vector3[] targetCardPositions = player.seat.cardPositions;
			for (int j = 0; j < 4; j++) {
				Vector3 targetCard = targetCardPositions [j];
				cards [j].transform.position = new Vector3(targetCard.x / SetupCardGame.TransformConstant, targetCard.y/SetupCardGame.TransformConstant);
				if (i == 0) {
					Vector3 localScale = new Vector3 (user0CardScale, user0CardScale);
					cards [j].transform.localScale = localScale;
				}

				if (i == 0 && player.userId == Player.Me.userId) {
					Debug.Log ("game.currentRound.playerCardsDict.ContainsKey ("+player.userId+"): " + game.currentRound.playerCardsDict.ContainsKey (player.userId));
					if (game.currentRound.playerCardsDict.ContainsKey (player.userId)) {
						cards [j].sprite = deck.GetCardFaceImage (game.currentRound.playerCardsDict [player.userId] [j]);
						Debug.Log ("Set My Card " + j + "th sprite, card is " + cards[j]);
					}
				}

				cards [j].gameObject.SetActive (true);
			}
		}
		isFirstDealDone = true;
	}
}
