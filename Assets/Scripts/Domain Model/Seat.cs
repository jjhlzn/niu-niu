using System;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Seat
{
	public Game game;
	public string seatNo;
	public Player player;
	public int seatIndex {
		get {
			for (int i = 0; i < game.seats.Length; i++) {
				if (game.seats [i] == this) {
					return i;
				}
			}
			throw new UnityException ("can't find index for seat: " + seatNo);
		}
	}

	//UI元素
	public GameObject playerPanel;
	public Image[] chipImages;
	public Image seatBorderImage;
	public Image robingSeatBorderImage;
	public Image playerImage;
	public Text playerNameLabel;
	public Text playerScoreLabel;
	public Text seatNumberLabel;
	public Image emptySeatImage;
	public Button sitdownButton;
	public Vector3 bankerSignPosition;
	public Image readyImage;
	public Image leaveImage;
	public Image waitImage;
	public Text scoreLabel;
	public Image chipImageForBet;
	public Vector3 originChipImagePositionForBet;
	public Vector3 chipPositionWhenBet; //下注的时候，筹码移动到的目的位置
	public Text chipCountLabel;
	public Image chipLabelBackground;
	public Image niuImage; //显示牛几的图片
	public Image mutipleImage; //显示牛的倍数的图片


	public Image isRobImage;

	public Vector3[] cardPositions;
	public Vector3[] showCardPositions;
	public Vector3 originScoreLabelPosition;
	public Vector3 targetScoreLabelPosition;

	public Image[] cards;

	public Seat() {
		cards = new Image[5];
	}

	public bool hasPlayer() {
		return player != null;
	}

	public void HideIsRobImage() {
		isRobImage.gameObject.SetActive (false);
	}

	public void UpdateUI(Game game) {
		robingSeatBorderImage.gameObject.SetActive (false);

		//有人的情况
		if (player != null) {
			//Debug.Log ("player userid = " + player.userId);
			//这些不需要根据状态设置
			seatNumberLabel.gameObject.SetActive (false);
			emptySeatImage.gameObject.SetActive (false);
			sitdownButton.gameObject.SetActive (false);
			seatBorderImage.gameObject.SetActive (true);

			playerImage.gameObject.SetActive (true);

			//player.headimgurl = game.PlayingPlayers [0].headimgurl;
			if (string.IsNullOrEmpty(player.headimgurl)) {
				playerImage.gameObject.SetActive (true);
			} else {
				ImageLoader.Instance.Load (player.headimgurl, (Sprite sprite) => {
					playerImage.sprite = sprite;
					playerImage.gameObject.SetActive (true);

				});
			}
				
			playerNameLabel.text = player.nickname;
			playerNameLabel.gameObject.SetActive (true);


			playerScoreLabel.text = Utils.GetNumberSring(player.score);
			playerScoreLabel.gameObject.SetActive (true);

			if (player.isPlaying && player.isReady 
						&& (game.state == GameState.BeforeStart || game.state == GameState.WaitForNextRound)) {
				readyImage.gameObject.SetActive (true);
			} else {
				readyImage.gameObject.SetActive (false);
			}

			if (player.isPlaying && player.isReady && game.state == GameState.WaitForNextRound) {
				if (scoreLabel.transform.position.y == originScoreLabelPosition.y) {
					scoreLabel.gameObject.SetActive (false);
				} else {
					scoreLabel.gameObject.SetActive (true);
				}

			} else {
				scoreLabel.gameObject.SetActive (false);
			}

			leaveImage.gameObject.SetActive (player.isDelegate);
			//什么情况是等待呢，游戏已经开始，刚刚坐下
			if (game.state != GameState.BeforeStart && game.state != GameState.WaitForNextRound) {
				if (!player.isPlaying && !player.isReady) {
					waitImage.gameObject.SetActive (true);
				} else {
					waitImage.gameObject.SetActive (false);
				}
			} else {
				waitImage.gameObject.SetActive (false);
			}

		} else {
			//Debug.Log ("player is null");
			scoreLabel.gameObject.SetActive (false);
			seatBorderImage.gameObject.SetActive (false);
			playerImage.gameObject.SetActive (false);
			playerNameLabel.gameObject.SetActive (false);
			playerScoreLabel.gameObject.SetActive (false);
			seatNumberLabel.text = "座位 [" + seatNo + "]"; 
			seatNumberLabel.gameObject.SetActive (true);

			if (!game.isMeSeated) {
				sitdownButton.gameObject.SetActive (true);
				emptySeatImage.gameObject.SetActive (false);
			} else {
				sitdownButton.gameObject.SetActive (false);
				emptySeatImage.gameObject.SetActive (true);
			}
			readyImage.gameObject.SetActive (false);
			leaveImage.gameObject.SetActive (false);
			waitImage.gameObject.SetActive (false);
		} 


	}

	public void Reset() {
		UpdateUI (game);

		for (int i = 0; i < cards.Length; i++) {
			cards [i] = null;
		}

		chipImageForBet.gameObject.SetActive (false);
		chipImageForBet.transform.position = originChipImagePositionForBet;

		chipCountLabel.gameObject.SetActive (false);
		chipLabelBackground.gameObject.SetActive (false);

		niuImage.gameObject.SetActive (false);
		mutipleImage.gameObject.SetActive (false);

		for (int i = 0; i < chipImages.Length; i++) {
			chipImages [i].transform.position = originChipImagePositionForBet;
			chipImages [i].gameObject.SetActive (false);
		}
	}

}


