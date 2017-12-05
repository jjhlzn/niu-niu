using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cn.sharesdk.unity3d;

public class GameOverController : BaseStateController {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private CompareCardController comparedCardController;

	[SerializeField]
	private ShareSDK ssdk;

	[SerializeField]
	private Connect connect;

	[SerializeField]
	private GameOverPanel gameOverPanel;
	private GameOverResponse notify;

	public bool isGetGameOverNotify = false;

	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayController;
	}

	// Use this for initialization
	void Start () {
	}

	public override void Reset() {
		isGetGameOverNotify = false;
		notify = null;
	}

	public void HandleResponse(GameOverResponse notify) {
		var game = gamePlayController.game;
		isGetGameOverNotify = true;
		this.notify = notify;

		if (notify.gameOverAfterRound) {
			var resultDict = notify.resultDict;
			var scoreDict = notify.scores;
			comparedCardController.HandleCurrentRoundOver (resultDict, scoreDict);

		} else {
			HandleGameOverResponse ();
		}
	}
		

	public void HandleGameOverResponse() {
		var game = gamePlayController.game;
		
		if (notify.isPlayed) {
			//展示游戏的成绩单
			gameOverPanel.Show (game, notify);

		} else { //房间没有玩过
			Dictionary<string, string> parameters = new Dictionary<string, string> ();
			parameters ["message"] = "房间已经被解散";
			Scenes.Load ("MainPage", parameters);
		}
	}


	public void BackToMainPageClick() {
		connect.disconnect ();
		Scenes.Load ("MainPage");
	}

	public void ShareClick() {
		Debug.Log ("Share Click");
		ScreenCapture.CaptureScreenshot("zhanji.png");
		var game = gamePlayController.game;
		ShareContent content = new ShareContent();
		//content.SetText("房间【" + game.roomNo + "】");
		content.SetTitle("");

		if (Application.platform == RuntimePlatform.Android) {
			string url = Application.temporaryCachePath.Replace ("cache", "files") + "/zhanji.png";
			content.SetImagePath (url);
		} else {
			string url = Application.temporaryCachePath.Replace ("cache", "files") + "/zhanji.png";
			Debug.Log ("share image url: " + url);
			content.SetImagePath (url);
		}
		content.SetShareType(ContentType.Image);
		ssdk.ShareContent (PlatformType.WeChat, content);
	}


}
