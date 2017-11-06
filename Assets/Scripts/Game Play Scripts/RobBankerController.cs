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

	[SerializeField]
	private Sprite robSprite;
	[SerializeField]
	private Sprite notRobSprite;

	private Seat[] seats;


	private bool hasRobBanker = false; 

	public override void Reset() {
		hasRobBanker = false;
	}

	void Start() {
		seats = gamePlayerController.game.seats;
	}
		
	// Update is called once per frame
	void Update () {
		//Debug.Log ("game state is : " + gamePlayerController.state.value);
		if (gamePlayerController.state == GameState.RobBanker && !hasRobBanker) {
			robRankerPanel.gameObject.SetActive (true);
		} else {
			robRankerPanel.gameObject.SetActive (false);
		}
	}

	public void RobClick() {
		SendRobBankerRequest (true);
	}

	public void NotRobClick() {
		SendRobBankerRequest (false);
	}

	private void SendRobBankerRequest(bool isRob) {
		if (gamePlayerController.state != GameState.RobBanker) {
			return;
		}

		var robReq = new {
			roomNo = gamePlayerController.game.roomNo,
			isRob = isRob,
			userId = Player.Me.userId
		};

		gamePlayerController.gameSocket.EmitJson (Messages.RobBanker, JsonConvert.SerializeObject (robReq), (string msg) => {
			hasRobBanker = true;
			Debug.Log("seats[0].isRobImage.gameObject = " + seats[0].isRobImage.gameObject);
			seats[0].isRobImage.gameObject.SetActive(true);
			if (isRob) {
				seats[0].isRobImage.sprite = robSprite;
			} else {
				seats[0].isRobImage.sprite = notRobSprite;
			}
		});

	}
		

	public void HanldeResponse(SomePlayerRobBankerNotify notify) {
		Debug.Log ("game.state = " + gamePlayerController.state.value);
		if (gamePlayerController.state == GameState.RobBanker) {
			int seatIndex = gamePlayerController.game.GetSeatIndex (notify.userId);
			if (seatIndex == -1) {
				throw new UnityException ("不能找到UserId = " + notify.userId + "的座位");
			}

			seats[seatIndex].isRobImage.gameObject.SetActive(true);
			if (notify.isRob) {
				seats[seatIndex].isRobImage.sprite = robSprite;
			} else {
				seats[seatIndex].isRobImage.sprite = notRobSprite;
			}
		}
	}
}
