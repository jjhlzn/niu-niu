using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
	private Sprite[] winOrLoseSigns;

	private UserScorePanel[] panels = new UserScorePanel[Game.SeatCount];
	private GameObject wholePanel;
	private Text descriptionLabel;


	public void Show(Game game, GameOverResponse resp) {
		var players = game.PlayingPlayers;
		for (int i = 0; i < players.Count; i++) {
			UserScorePanel panel = panels [i];
			panel.nickNameLabel.text = players [i].nickname;
			var player = players [i];
			ImageLoader.Instance.Load (player.headimgurl, (Sprite sprite) => {
				panel.userImage.sprite = sprite;
			});
			int score = resp.scores [players [i].userId];
			panel.ScoreLabel.text = score > 0 ? "+" + score :  score + "";
			if (score > 0) {
				panel.ScoreLabel.color = Color.red;
			} else {
				panel.ScoreLabel.color = Color.blue;
			}
			panel.userIdLabel.text = players [i].userId;

			if (game.creater == players [i].userId) {
				panel.createrImageSign.gameObject.SetActive (true);
			} else {
				panel.createrImageSign.gameObject.SetActive (false);
			}

			if (resp.bigWinners.Contains (players [i].userId)) {
				panel.winOrLoseImageSign.gameObject.SetActive (true);
				panel.winOrLoseImageSign.sprite = winOrLoseSigns [0];
			} else if (resp.bigLosers.Contains (players [i].userId)) {
				panel.winOrLoseImageSign.gameObject.SetActive (true);
				panel.winOrLoseImageSign.sprite = winOrLoseSigns [1];
			} else {
				panel.winOrLoseImageSign.gameObject.SetActive (false);
			}
			panel.panel.gameObject.SetActive (true);
		}

		wholePanel.gameObject.SetActive (true);

		descriptionLabel.text = "房号："+game.roomNo+"    名牌抢庄,  "+game.totalRoundCount+"局, 【4，6，8分】,  闲家推注        " + resp.gameOverTime;
	}

	void Start() {
		winOrLoseSigns = new Sprite[2];
		winOrLoseSigns[0] = Resources.Load<Sprite> ("sprites/gameplay/big_winer_sign");
		winOrLoseSigns[1] = Resources.Load<Sprite> ("sprites/gameplay/big_loser_sign");
	}
		
	public void Setup(GameObject panel, GameObject eachPanel, GameObject parent) {
		wholePanel = panel;
		GameObject eachUserPanel = null;

		SetDesciptionLabel (panel);

		//Debug.Log ("score.text = " + descriptionLabel.text);
		for (int i = 0; i < Game.SeatCount; i++) {
			panels [i] = CreateUserScorePanel (eachPanel, i, panel);
		}

		wholePanel.SetActive (false);
	}

	private void SetDesciptionLabel(GameObject panel) {
		Text[] texts = panel.GetComponentsInChildren<Text> ();
		foreach (Text text in texts) {
			if (text.name == "Game Description Label") {
				descriptionLabel = text;
				break;
			}
		}
	}

	private UserScorePanel CreateUserScorePanel(GameObject p, int index, GameObject parent) {
		UserScorePanel panel = new UserScorePanel ();

		GameObject copy = null;

		copy = Instantiate (p);

		copy.name = "score_panel_" + index;
		copy.transform.SetParent (parent.transform);
		Vector3 localScale = new Vector3 (1f, 1f);
		copy.transform.localScale = localScale;

		float x = 0, y = 0;
		if (index < 3) {
			x = 10 + index * 340;
			y = 0;
		} else {
			x = 10 + (index - 3) * 340;
			y = -180;
		}
		copy.transform.position = wholePanel.transform.TransformPoint( new Vector3 ( x , y ));
		//copy.transform.position = new Vector3 ( x / SetupCardGame.TransformConstant, y / SetupCardGame.TransformConstant, 0);



		Text[] childrenTexts = copy.GetComponentsInChildren<Text> ();
		foreach (Text text in childrenTexts) {
			switch (text.name) {
			case "Nick Name Label":
				panel.nickNameLabel = text;
				break;
			case "ID Label":
				panel.userIdLabel = text;
				break;
			case "Score Label":
				panel.ScoreLabel = text;
				break;
			}
		}

		Image[] childImages = copy.GetComponentsInChildren<Image> ();
		foreach (Image image in childImages) {
			switch (image.name) {
			case "Creater Image Sign":
				panel.createrImageSign = image;
				break;
			case "Win Or Lose Image Sign":
				panel.winOrLoseImageSign = image;
				break;
			case "User Image":
				panel.userImage = image;
				break;
			}
		}
		panel.panel = copy;
		copy.gameObject.SetActive (false);
		return panel;
	}
	

}

public class UserScorePanel {
	public GameObject panel;
	public Image userImage;
	public Image createrImageSign;
	public Image winOrLoseImageSign;
	public Text nickNameLabel;
	public Text userIdLabel;
	public Text ScoreLabel;
}


