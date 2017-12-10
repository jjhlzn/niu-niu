using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cn.sharesdk.unity3d;
using System.IO;

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
		ScreenCapture.CaptureScreenshot(Utils.GetShareGameResultFileName());

		ShareContent content = new ShareContent();
		content.SetTitle("");
		string url = Utils.GetShareGameResultUrl ();
		Debug.Log ("share image url: " + url);

		content.SetImagePath (url);
		content.SetShareType(ContentType.Image);

		StartCoroutine(Share(content));
	}

	private IEnumerator Share(ShareContent content) {
		yield return new WaitForSeconds (.3f);
		if (File.Exists(Utils.GetShareGameResultUrl())) {
			ssdk.ShareContent (PlatformType.WeChat, content);
		}
	}

}
