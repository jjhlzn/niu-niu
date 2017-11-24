using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverController : BaseStateController {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private CompareCardController comparedCardController;

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
		ScreenCapture.CaptureScreenshot ("zhanji.png");
	}
}
