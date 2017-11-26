using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class RobBankerController : BaseStateController {

	[SerializeField]
	private GamePlayController gamePlayerController; 

	[SerializeField]
	private GameObject robRankerPanel;


	private Sprite robSprite;
	private Sprite notRobSprite;

	private Seat[] seats;

	private float stateTimeLeft; //这状态停留的时间
	//private bool hasRobBanker = false; 

	public override void Reset() {
		//hasRobBanker = false;
		stateTimeLeft = Constants.MaxStateTimeLeft;
	}

	void Start() {
		robSprite = Resources.Load<Sprite> ("sprites/gameplay/RobImage");
		notRobSprite = Resources.Load<Sprite> ("sprites/gameplay/NotRobImage");
		seats = gamePlayerController.game.seats;
	}

	public void Init() {
		seats = gamePlayerController.game.seats;
		stateTimeLeft = Constants.MaxStateTimeLeft;
	}
		
	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayerController;
	}

	// Update is called once per frame
	public new void Update ()  {
		base.Update ();
		if (gamePlayerController.state == GameState.RobBanker) {
			if (stateTimeLeft >= 0) {
				gamePlayerController.game.ShowStateLabel ("抢庄: " + Mathf.Round (stateTimeLeft));
				stateTimeLeft -= Time.deltaTime;
			}

			if (Player.Me.isPlaying && !Player.Me.hasRobBanker && !robRankerPanel.gameObject.activeInHierarchy) {
				robRankerPanel.gameObject.SetActive (true);
			}
		}
			
	}

	private bool IsMeRobed() {
		var game = gamePlayerController.game;
		foreach (KeyValuePair<string, bool> pair in game.currentRound.robBankerDict) {
			if (pair.Key == Player.Me.userId)
				return true;
		}
		return false;
	}

	public void SetUI() {
		var game = gamePlayerController.game;

		if (IsMeRobed()) {
			Player.Me.hasRobBanker = true;
			robRankerPanel.gameObject.SetActive (false);
		} 

		foreach (KeyValuePair<string, bool> pair in game.currentRound.robBankerDict) {
			int seatIndex = game.GetSeatIndex (pair.Key);
			if (seatIndex == 0) {
				HandleSeat0RobBanker (pair.Value);
			} else {
				HandleOtherSeatRobBanker (seatIndex, pair.Value);
			}
		}
	}

	public void RobClick() {
		SendRobBankerRequest (true);
	}

	public void NotRobClick() {
		SendRobBankerRequest (false);
	}

	private void HandleSeat0RobBanker(bool isRob) {
		robRankerPanel.gameObject.SetActive (false);
		Player.Me.hasRobBanker = true;
		seats[0].isRobImage.gameObject.SetActive(true);
		if (isRob) {
			seats[0].isRobImage.sprite = robSprite;
			MusicController.instance.Play(AudioItem.Rob, seats[0].player.sex);
		} else {
			seats[0].isRobImage.sprite = notRobSprite;
			MusicController.instance.Play(AudioItem.NotRob, seats[0].player.sex);
		}
	}

	private void HandleOtherSeatRobBanker(int seatIndex, bool isRob) {
		seats [seatIndex].player.hasRobBanker = true;
		seats[seatIndex].isRobImage.gameObject.SetActive(true);
		if (isRob) {
			seats[seatIndex].isRobImage.sprite = robSprite;
			MusicController.instance.Play(AudioItem.Rob, seats[seatIndex].player.sex);
		} else {
			seats[seatIndex].isRobImage.sprite = notRobSprite;
			MusicController.instance.Play(AudioItem.NotRob, seats[seatIndex].player.sex);
		}

	}

	private void SendRobBankerRequest(bool isRob) {
		if (gamePlayerController.state != GameState.RobBanker) {
			return;
		}

		var robReq = new {
			roomNo = gamePlayerController.game.roomNo,
			isRob = isRob ? 1 : 0,
			userId = Player.Me.userId
		};

		gamePlayerController.gameSocket.EmitJson (Messages.RobBanker, JsonConvert.SerializeObject (robReq), (string msg) => {
		});
	}
		

	public void HandleResponse(SomePlayerRobBankerNotify notify) {

		if (gamePlayerController.state == GameState.RobBanker) {
			int seatIndex = gamePlayerController.game.GetSeatIndex (notify.userId);
			if (seatIndex == -1) {
				throw new UnityException ("不能找到UserId = " + notify.userId + "的座位");
			}

			if (seatIndex == 0) {
				HandleSeat0RobBanker(notify.isRob);
				return;
			} else {
				HandleOtherSeatRobBanker (seatIndex, notify.isRob);
			}
		}
	}
}
