using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundsPanel : MonoBehaviour {
	
	[SerializeField]
	private Button prevBtn;
	[SerializeField]
	private Button firstBtn;
	[SerializeField]
	private Button nextBtn;
	[SerializeField]
	private Button latestBtn;
	[SerializeField]
	private Text roundNoLabel;
	[SerializeField]
	private Text roomNoLabel;

	[SerializeField]
	private GameObject playerScoreGameObj;
	[SerializeField]
	private GamePlayController gamePlayController;

	private RoundPlayerScorePanel[] playerScores;

	private bool hasInit = false;
	private int roundNo = 0;

	// Use this for initialization
	void Start () {
		//StartCoroutine (ExecInit ());
	}

	public IEnumerator ExecInit() {
		yield return new WaitForSeconds (3f);
		Init ();
	}

	public void Init() {
		if (hasInit)
			return;

		hasInit = true;
		var game = gamePlayController.game;
		roomNoLabel.text = "房间号: " + game.roomNo;
		roundNoLabel.text = game.currentRoundNo + "/" + game.totalRoundCount;

		//设置PlayerScores
		playerScores = new RoundPlayerScorePanel[Game.SeatCount];
		for (int i = 0; i < playerScores.Length; i++) {
			playerScores [i] = new RoundPlayerScorePanel ();
			GameObject playerScore = Instantiate (playerScoreGameObj);
			Text[] texts = playerScore.GetComponentsInChildren<Text> ();
			foreach (Text text in texts) {
				switch (text.name) {
				case "Name Label":
					playerScores [i].nameLabel = text;
					break;
				case "ID Label":
					playerScores [i].idLabel = text;
					break;
				case "Bet Count Label":
					playerScores [i].betCountLabel = text;
					break;
				case "Score Label":
					playerScores [i].scoreLabel = text;
					break;
				}
			}

			Image[] imgs = playerScore.GetComponentsInChildren<Image> ();

			foreach (Image img in imgs) {
				switch (img.name) {
				case "User Image":
					playerScores [i].userImage = img;
					break;
				case "chip":
					playerScores [i].chip = img;
					break;
				case "chip_bg":
					playerScores [i].chipBackground = img;
					break;
				case "Banker Sign":
					playerScores [i].bankerSign = img;
					break;
				case "niu":
					playerScores [i].niuImage = img;
					break;
				case "Card0":
					playerScores [i].cards [0] = img;
					break;
				case "Card1":
					playerScores [i].cards [1] = img;
					break;
				case "Card2":
					playerScores [i].cards [2] = img;
					break;
				case "Card3":
					playerScores [i].cards [3] = img;
					break;
				case "Card4":
					playerScores [i].cards [4] = img;
					break;
				}


			}
			playerScores [i].panel = playerScore;

			Transform parent = playerScoreGameObj.transform.parent;
			playerScore.transform.SetParent (parent);
			playerScore.transform.localScale = new Vector3 (1f, 1f);

			int x = -2, y = i / 3 * -220;
			playerScore.transform.position = new Vector3 ((x + i % 3 * 181) / SetupCardGame.TransformConstant, y / SetupCardGame.TransformConstant);
			//playerScore.SetActive (true);
		}
	}

	private void ShowRound(){
		//Debug.Log ("ShowRound() called");
		if (roundNo == 0 || roundNo >= gamePlayController.game.currentRoundNo)
			return;

		var game = gamePlayController.game;
		roundNoLabel.text = roundNo + "/" + game.totalRoundCount;

		var round = gamePlayController.game.rounds [roundNo - 1];
		//Debug.Log ("round = " + round);
		foreach (RoundPlayerScorePanel playerScore in playerScores) {
			playerScore.panel.gameObject.SetActive (false);
		}

		List<Player> players = round.players;
		for (int i = 0; i < players.Count; i++) {
			Player player = players [i];
			playerScores [i].nameLabel.text = player.nickname;
			playerScores [i].idLabel.text = "ID: " + player.userId;
			Image userImage = playerScores [i].userImage;

			ImageLoader.Instance.Load (player.headimgurl, (Sprite sprite) => {
				userImage.sprite = sprite;

			});
			if (round.banker == player.userId) {
				playerScores [i].bankerSign.gameObject.SetActive (true);

				playerScores [i].chipBackground.gameObject.SetActive (false);
				playerScores [i].chip.gameObject.SetActive (false);
				playerScores [i].betCountLabel.gameObject.SetActive (false);
			} else {
				playerScores [i].bankerSign.gameObject.SetActive (false);

				playerScores [i].chipBackground.gameObject.SetActive (true);
				playerScores [i].chip.gameObject.SetActive (true);
				playerScores [i].betCountLabel.text = round.playerBets [player.userId] + "";
				playerScores [i].betCountLabel.gameObject.SetActive (true);
			}
			string[] cardPoints = round.playerCardsDict [player.userId];
			for (int j = 0; j < cardPoints.Length; j++) {
				playerScores [i].cards [j].sprite = gamePlayController.game.deck.GetCardFaceImage (cardPoints [j]);
			}

			playerScores [i].niuImage.sprite = gamePlayController.game.getNiuSprite (round.niuArray [player.userId]);

			int score = round.resultDict [player.userId];
			playerScores [i].scoreLabel.text = Utils.GetNumberSring (score);
			if (score >= 0) {
				playerScores [i].scoreLabel.color = new Color (187f, 19f, 19f);
			} else {
				playerScores [i].scoreLabel.color = new Color (13f, 111f, 32f);
			}

			playerScores [i].panel.SetActive (true);
		}

	}
	

	public void FirstBtnClick() {
		roundNo = 1;
		ShowRound ();
	}


	public void PrevBtnClick() {
		roundNo -= 1;
		if (roundNo <= 1)
			roundNo = 1;
		ShowRound ();
	}

	public void NextBtnClick() {
		roundNo += 1;
		if (roundNo >= gamePlayController.game.currentRoundNo - 1)
			roundNo = gamePlayController.game.currentRoundNo - 1;
		ShowRound ();
	}

	public void LatestBtnClick() {
		roundNo = gamePlayController.game.currentRoundNo - 1;
		ShowRound ();
	}

	public void CloseBtnClick() {
		this.gameObject.SetActive (false);
	}

	public void OpenBtnClick() {
		var game = gamePlayController.game;
		if (game != null) {
			this.Init ();
			roundNo = game.currentRoundNo - 1;
			if (game.state != GameState.BeforeStart) {
				ShowRound ();
			}
			this.gameObject.SetActive (true);
		}
	}

}

public class RoundPlayerScorePanel {
	public GameObject panel;
	public Image userImage;
	public Text nameLabel;
	public Text idLabel;

	public Text betCountLabel;
	public Image chip;
	public Image chipBackground;
	public Image bankerSign;

	public Image[] cards = new Image[5];
	public Image niuImage;
	public Text scoreLabel;

}
