using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using socket.io;
using BestHTTP.SocketIO;
using Newtonsoft.Json;
using DG.Tweening;

public class BetController : BaseStateController {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private SecondDealController secondDealController;

	private float user0ChipMoveSpeed = 220f;
	private float chipMoveSpeed = 220f;


	private bool[] isMoveChipArray;
	private bool[] isBetCompletedArray;
	//private bool hasBet = false;
	private float stateTimeLeft; //这状态停留的时间
	private float animationTime = 1f;
	private bool hasPlayBetTip;
	private bool hasPlayCountDown;

	public bool IsAllBetCompleted {
		get {
			bool result = true;
			for (var i = 0; i < seats.Length; i++) {
				var player = seats [i].player;
				if (seats [i].hasPlayer () && player.isPlaying && player.userId != gamePlayController.game.currentRound.banker) {
					result = result && isBetCompletedArray [i];
				}
			}
			return result;
		}
	}

	public void Init() {
		stateTimeLeft = Constants.MaxStateTimeLeft - animationTime;
		gamePlayController.game.HideBetButtons ();
	}
		
	public void Awake() {
		isMoveChipArray = new bool[Game.SeatCount];
		isBetCompletedArray = new bool[Game.SeatCount];
	}

	public override void Reset() {
		//hasBet = false;
		isMoveChipArray = new bool[Game.SeatCount];
		isBetCompletedArray = new bool[Game.SeatCount];
		stateTimeLeft = Constants.MaxStateTimeLeft - animationTime;
		hasPlayBetTip = false;
		hasPlayCountDown = false;
		gamePlayController.game.HideBetButtons ();
	}
	
	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	// Update is called once per frame
	public new void Update ()  {
		base.Update ();
		if (gamePlayController.state == GameState.Bet) {
			if (!hasPlayBetTip) {
				hasPlayBetTip = true;
				MusicController.instance.Play (AudioItem.BetTip);
			}

			if (stateTimeLeft <= 3.5f && !hasPlayCountDown) {
				hasPlayCountDown = true;
				MusicController.instance.Play (AudioItem.CountDown);
			}
				
			if (stateTimeLeft > 0) {
				gamePlayController.game.ShowStateLabel ("请选择下注分数: " + Mathf.Round (stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}

			if (Player.Me.isPlaying
			    && !Player.Me.hasBet
			    && gamePlayController.game.currentRound.banker != gamePlayController.game.PlayingPlayers [0].userId) {
				gamePlayController.game.ShowBetButtons ();
			} 
		} else {
			gamePlayController.game.HideBetButtons ();
		}
			
		BetAnimation ();
	}

		

	private void BetAnimation() {
		for (int i = 0; i < Game.SeatCount; i++) {
			if (isMoveChipArray [i]) {
				isMoveChipArray [i] = false;

				Seat seat = seats [i];
				int bet = gamePlayController.game.currentRound.playerBets [seat.player.userId];
				if (!seats[i].chipImageForBet.gameObject.activeInHierarchy)
					seats[i].chipImageForBet.gameObject.SetActive (true);

				Vector3 targetPosition = seats[i].chipPositionWhenBet;

			
				int index = i;
				seats [i].chipImageForBet.transform
					.DOMove (targetPosition, 0.5f)
					.SetDelay(0.3f)
					.OnComplete (() => {
					seat.chipCountLabel.text = bet + "";
					seat.chipCountLabel.gameObject.SetActive (true);
					seat.chipLabelBackground.gameObject.SetActive (true);
					//Debug.Log ("index = " + index);
					isBetCompletedArray [index] = true;

				});
			}
		}
	}

	private void HandleUser0BetNotify(int bet) {
		gamePlayController.game.HideBetButtons ();
		Player.Me.hasBet = true;
		gamePlayController.game.currentRound.playerBets[seats[0].player.userId] = bet;
		isMoveChipArray[0] = true;
		MusicController.instance.Play (AudioItem.Bet, seats [0].player.sex);
	}

	public void SetBetClick(Button[] buttons) {
		for (int i = 0; i < buttons.Length; i++) {
			buttons [i].onClick.AddListener (BetClick);
		}
	}

	public void BetClick() {
		Debug.Log ("bet button click");
		if (gamePlayController.state != GameState.Bet) {
			Debug.LogError ("current state is not bet, the state is " + gamePlayController.state.value);
			return;
		}

		string betName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
		int index = int.Parse(betName.Substring (betName.Length - 1));
		int mybet = gamePlayController.game.currentRound.myBets [index];
			
		Socket socket = gamePlayController.gameSocket; 
		var req = new {
			roomNo = gamePlayController.game.roomNo,
			userId = Player.Me.userId,
			bet = mybet,
			clientInfo = Utils.GetClientInfo(),
			userInfo = Utils.GetUserInfo()
		};

		socket.Emit (Messages.Bet, JsonConvert.SerializeObject(req)); 

	}

	public void HandleResponse(SomePlayerBetNotify notify) {
		Game game = gamePlayController.game;
		int index = game.GetSeatIndex (notify.userId);

		if (index == 0) {
			HandleUser0BetNotify (notify.bet);
		} else {
			game.currentRound.playerBets [notify.userId] = notify.bet;
			isMoveChipArray [index] = true;
			MusicController.instance.Play (AudioItem.Bet, seats [index].player.sex);
		}
	}


	public void SetUI() {
		var game = gamePlayController.game;
		var round = game.currentRound;

		foreach (KeyValuePair<string, int> pair in round.playerBets) {
			string userId = pair.Key;
			int seatIndex = game.GetSeatIndex (userId);
			seats[seatIndex].chipLabelBackground.gameObject.SetActive (true);
			seats[seatIndex].chipImageForBet.gameObject.transform.position = seats [seatIndex].chipPositionWhenBet;
			seats[seatIndex].chipImageForBet.gameObject.SetActive (true);
			seats[seatIndex].chipCountLabel.text = pair.Value + "";
			seats[seatIndex].chipCountLabel.gameObject.SetActive (true);
			isBetCompletedArray [seatIndex] = true;

			if (Player.Me.isPlaying && Player.Me.seat.seatIndex == seatIndex) {
				Player.Me.hasBet = true;
			}
		}
	}
}
