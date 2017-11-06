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
	public Image bankerSignImage;
	public Image readyImage;
	public Text scoreLabel;
	public Image chipImagesForBet;
	public Vector3 chipPositionWhenBet;
	public Text chipCountLabel;


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

	public void UpdateUI(Game game) {
		if (game.state == GameState.BeforeStart) {
			robingSeatBorderImage.gameObject.SetActive (false);
			bankerSignImage.gameObject.SetActive (false);
			scoreLabel.gameObject.SetActive (false);

			if (player != null) {  
				seatBorderImage.gameObject.SetActive (true);
				playerImage.gameObject.SetActive (true);
				playerNameLabel.text = player.userId;
				Debug.Log ("seat " + seatNo + " userid = " + player.userId);
				playerNameLabel.gameObject.SetActive (true);
				playerScoreLabel.text = player.score + "";
				playerScoreLabel.gameObject.SetActive (true);
				seatNumberLabel.gameObject.SetActive (false);

				emptySeatImage.gameObject.SetActive (false);
				sitdownButton.gameObject.SetActive (false);
				readyImage.gameObject.SetActive (true);
			} else {
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
			} 

		} else {
		}
	}

}


