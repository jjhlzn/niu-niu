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

	private Image[] isRobImages;

	private bool hasRobBanker = false; 

	public override void Reset() {
		hasRobBanker = false;
		foreach (Image isRobImage in isRobImages) {
			isRobImage.gameObject.SetActive (false);
		}
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
			this.isRobImages[0].gameObject.SetActive(true);
			if (isRob) {
				this.isRobImages[0].sprite = robSprite;
			} else {
				this.isRobImages[0].sprite = notRobSprite;
			}
		});

	}

	public void SetIsRobImages(Image[] isRobImages) {
		this.isRobImages = isRobImages;
	}

	public void HanldeResponse(SomePlayerRobBankerNotify notify) {
		Debug.Log ("game.state = " + gamePlayerController.state.value);
		if (gamePlayerController.state == GameState.RobBanker) {
			int seatIndex = gamePlayerController.game.GetSeatIndex (notify.userId);
			if (seatIndex == -1) {
				throw new UnityException ("不能找到UserId = " + notify.userId + "的座位");
			}

			this.isRobImages[seatIndex].gameObject.SetActive(true);
			if (notify.isRob) {
				this.isRobImages [seatIndex].sprite = robSprite;
			} else {
				this.isRobImages [seatIndex].sprite = notRobSprite;
			}
		}
	}
}
