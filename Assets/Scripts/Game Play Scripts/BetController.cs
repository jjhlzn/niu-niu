using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;

public class BetController : BaseStateController {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private SecondDealController secondDealController;

	private float user0ChipMoveSpeed = 220f;
	private float chipMoveSpeed = 220f;

	private Seat[] seats {
		get {
			return gamePlayController.game.seats;
		}
	}

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

			if (stateTimeLeft <=  3.5f && !hasPlayCountDown) {
				hasPlayCountDown = true;
				MusicController.instance.Play (AudioItem.CountDown);
			}
				
			if (stateTimeLeft > 0) {
				gamePlayController.game.ShowStateLabel ("请选择下注分数: " + Mathf.Round (stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}

			if (  Player.Me.isPlaying
				&& !Player.Me.hasBet
				&& gamePlayController.game.currentRound.banker != gamePlayController.game.PlayingPlayers [0].userId) {
				gamePlayController.game.ShowBetButtons ();
			} 
		} 
			
		BetAnimation ();
	}

		

	private void BetAnimation() {
		for (int i = 0; i < Game.SeatCount; i++) {
			if (isMoveChipArray [i]) {
				if (!seats[i].chipImageForBet.gameObject.activeInHierarchy)
					seats[i].chipImageForBet.gameObject.SetActive (true);

				Vector3 targetPosition = seats[i].chipPositionWhenBet;
				float step = 0;
				if (i == 0) {
					step = user0ChipMoveSpeed * Time.deltaTime;
				} else {
					step = chipMoveSpeed * Time.deltaTime;
				}

				seats[i].chipImageForBet.gameObject.transform.position = Vector3.MoveTowards (seats[i].chipImageForBet.gameObject.transform.position,
					targetPosition, step);

				if (Utils.isTwoPositionIsEqual (seats[i].chipImageForBet.gameObject.transform.position, targetPosition)) {
					isMoveChipArray [i] = false;
					seats[i].chipCountLabel.text = gamePlayController.game.currentRound.playerBets[i] + "";
					seats[i].chipCountLabel.gameObject.SetActive (true);
					seats [i].chipLabelBackground.gameObject.SetActive (true);
					isBetCompletedArray [i] = true;

					if (gamePlayController.state == GameState.Bet) {
						if (IsAllBetCompleted) {
							Debug.Log ("In BetControoler: change to SecondSeal state");
							//gamePlayController.game.HideStateLabel ();
						}
					}
				}
			}
		}
	}

	private void HandleUser0BetNotify(int bet) {
		gamePlayController.game.HideBetButtons ();
		Player.Me.hasBet = true;
		gamePlayController.game.currentRound.playerBets[0] = bet;
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
			bet = mybet
		};

		socket.EmitJson (Messages.Bet, JsonConvert.SerializeObject(req), (string msg) => {
		}); 

	}

	public void HandleResponse(SomePlayerBetNotify notify) {
		Game game = gamePlayController.game;
		int index = game.GetSeatIndex (notify.userId);

		if (index == 0) {
			HandleUser0BetNotify (notify.bet);
		} else {
			game.currentRound.playerBets [index] = notify.bet;
			isMoveChipArray [index] = true;
			MusicController.instance.Play (AudioItem.Bet, seats [index].player.sex);
		}
	}


	public void SetUI() {
		var game = gamePlayController.game;
		var round = game.currentRound;
		for (int i = 0; i < round.playerBets.Length; i++) {
			if (round.playerBets [i] != -1) {
				seats [i].chipLabelBackground.gameObject.SetActive (true);
				seats[i].chipImageForBet.gameObject.transform.position = seats [i].chipPositionWhenBet;
				seats[i].chipImageForBet.gameObject.SetActive (true);
				seats[i].chipCountLabel.text = gamePlayController.game.currentRound.playerBets[i] + "";
				seats[i].chipCountLabel.gameObject.SetActive (true);
				isBetCompletedArray [i] = true;

				if (Player.Me.isPlaying && Player.Me.seat.seatIndex == i) {
					Player.Me.hasBet = true;
				}
			} 
		}
	}
}
