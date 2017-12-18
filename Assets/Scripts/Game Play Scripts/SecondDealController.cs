using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SecondDealController : BaseStateController {
	private float waitTimeBeforeSecondDeal = 0.5f;
	[SerializeField]
	private GamePlayController gamePlayController;
	[SerializeField]
	private BetController betController;
	[SerializeField]
	private FirstDealerController firstDealController;
	[SerializeField]
	private GameObject deckCardPosition;
	[SerializeField]
	private GameObject userPanel;

	private bool isSecondDealing;
	public bool isSecondDealDone;

	public override void Reset() {
		isSecondDealing = false;
		isSecondDealDone = false;
	}

	public void Init() {
		isSecondDealing = false;
		isSecondDealDone = false;
	}
		
	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	// Update is called once per frame
	public new void Update ()  {
		base.Update ();
		SecondDealAnimation ();
	}
		
	private void SecondDealAnimation() {
		if (isSecondDealing && firstDealController.isFirstDealDone && betController.IsAllBetCompleted) {
			isSecondDealing = false;
			StartCoroutine (ExecuteSecondDealAnimation ());
		}
	}

	private IEnumerator ExecuteSecondDealAnimation() {
		yield return new WaitForSeconds (waitTimeBeforeSecondDeal);

		deck.ShowNotDealCardsForSecondDeal(gamePlayController.game.PlayingPlayers.Count);
		MusicController.instance.Play (AudioItem.Deal, isLoop: true);

		List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;
		for (int i = 0; i < playingPlayers.Count; i++) {
			Player player = playingPlayers [i];

			Tween t = player.cards[4].transform
				.DOLocalMove(player.seat.cardPositions[4], FirstDealerController.dealSpeed)
				.SetSpeedBased()
				.SetDelay (i * FirstDealerController.waitTimeDeltaBetweenCard);

			if (i == 0)
				player.cards[4].transform
					.DOScale (1.3f, 0.04f)
					.SetDelay (FirstDealerController.waitTimeDeltaBetweenCard + 0.02f);

			if (i == playingPlayers.Count - 1) {
				t.OnComplete (() => {
					MusicController.instance.Stop (AudioItem.Deal);
					if (gamePlayController.state == GameState.SecondDeal)
						gamePlayController.state = GameState.CheckCard;
					isSecondDealing = false;
					isSecondDealDone = true;
					Debug.Log ("SecondDealAnimation done");
				});
			}
		}
	}
		
	private void SecondDeal() {
		List<Player> players = gamePlayController.game.PlayingPlayers;
		for (int i = 0; i < players.Count; i++) {
			players[i].seat.cards [4] = deck.Deal ();
		}
	}
		
		
	public void HandleResponse(GoToSecondDealNotify notify) {
		var game = gamePlayController.game;
		var round = game.currentRound;
		game.HideStateLabel ();

		if (round.playerCardsDict.ContainsKey (seats [0].player.userId)) {
			string[] cards = round.playerCardsDict [seats [0].player.userId];
			cards [4] = notify.cardsDict [seats [0].player.userId];
		}
			
		SecondDeal ();
		gamePlayController.state = GameState.SecondDeal;
		isSecondDealing = true;
	}


	public void SetUI() {
		SecondDeal ();
		List<Player> playingPlayers = gamePlayController.game.PlayingPlayers;
		for (int i = 0; i < playingPlayers.Count; i++) {
			var player = playingPlayers [i];
			Vector3 position = player.seat.cardPositions [4];
			player.cards[4].transform.position = userPanel.transform.TransformPoint(position) ;
			//player.cards[4].transform.position = new Vector3(position.x / SetupCardGame.TransformConstant, position.y / SetupCardGame.TransformConstant) ;
			if (player.seat.seatIndex == 0) {
				Vector3 localScale = new Vector3 ();
				localScale.x = FirstDealerController.user0CardScale;
				localScale.y = FirstDealerController.user0CardScale;
				player.cards[4].transform.localScale = localScale;
			}
			player.cards [4].gameObject.SetActive (true);
		}
		isSecondDealDone = true;
	}
}
