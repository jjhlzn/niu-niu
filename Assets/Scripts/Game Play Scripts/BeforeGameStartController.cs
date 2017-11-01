using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;

public class BeforeGameStartController : MonoBehaviour {

	private static float moveSeatSpeed = 30f;

	[SerializeField]
	private GamePlayController gamePlayerController;

	public GameObject[] userPanels;
	public Image[] seatImages;
	public Image[] playerImages;
	public Text[] playerNames;
	public Text[] playerScores;
	public Button[] seatButtons;
	public Text[] seatDescs;
	public Image[] emptySeatImages;

	private bool isMoveSeat;
	private int[] fromPositions;
	private int[] toPositions;

	private Vector3[] positions;

	private bool isSeat;

	// Use this for initialization
	void Start () {
		isMoveSeat = false;
		isSeat = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (isMoveSeat) {
			float step = moveSeatSpeed * Time.deltaTime;
			bool[] bools = new bool[Game.SeatCount]; 
			for (int i = 0; i < fromPositions.Length; i++) {
				//Debug.Log ("move seat: " + fromPositions[i] + " --> " + toPositions[i]);
				GameObject fromP = userPanels [fromPositions[i]];
				Vector3 to = positions [toPositions[i]];
				fromP.transform.position = Vector3.MoveTowards(fromP.transform.position, to, step);

				if (fromP.transform.position == to) {
					bools [i] = true;
				}
			}

			bool isMoveOver = true;
			for (int i = 0; i < fromPositions.Length; i++) {
				isMoveOver = isMoveOver && bools [i];
			}

			if (isMoveOver) {
				isMoveSeat = false;
				for (int i = 0; i < Game.SeatCount; i++) {
					userPanels [i].transform.position = positions [i];
				}
				SetPlayerSeatUI ();
			}

		} else {
			
		}
	}
		

	public void SetPlayerImages(Image[] playerImages) {
		this.playerImages = playerImages;
	}

	public void SetPlayerNames(Text[] playerNames) {
		this.playerNames = playerNames;
	}

	public void SetPlayerScores(Text[] scores) {
		this.playerScores = scores;
	}

	public void SetSeatButtons(Button[] seatButtons) {
		this.seatButtons = seatButtons;


	}

	public void SetUserPanels(GameObject[] userPanels) {
		this.userPanels = userPanels;

		positions = new Vector3[Game.SeatCount];
		for (int i = 0; i < positions.Length; i++) {
			positions [i] = userPanels [i].gameObject.transform.position;
		}
	}

	public void SetSeatDescs(Text[] seatDescs) {
		this.seatDescs = seatDescs;
	}

	public void SetSeatImages(Image[] seatImages) {
		this.seatImages = seatImages;
	}

	public void SetEmptySeatImages(Image[] emptySeatImages) {
		this.emptySeatImages = emptySeatImages;
	}

	//根据是否有玩家坐在座位上，设置位置的外观
	public void SetPlayerSeatUI() {
		
		//Player[] players = gamePlayerController.game.players;
		Seat[] seats = gamePlayerController.game.seats;

		for (int i = 0; i < seats.Length; i++) {
			Seat seat = seats [i];
			if (seat.player != null) {
				this.playerImages [i].gameObject.SetActive (true);
				this.playerNames [i].gameObject.SetActive (true);
				this.playerScores [i].gameObject.SetActive (true);
				this.seatButtons [i].gameObject.SetActive (false);
				this.seatImages [i].gameObject.SetActive (true);
				this.emptySeatImages [i].gameObject.SetActive (false);

				this.playerNames [i].text = seat.player.userId;
				this.seatDescs [i].text = "座位 [" + seat.seatNo + "]";
			} else {
				this.playerImages [i].gameObject.SetActive (false);
				this.playerNames [i].gameObject.SetActive (false);
				this.playerScores [i].gameObject.SetActive (false);

				this.seatImages [i].gameObject.SetActive (false);
				this.seatDescs [i].text = "座位 [" + seat.seatNo + "]";

				if (isSeat) {
					this.emptySeatImages [i].gameObject.SetActive (true);
					this.seatButtons [i].gameObject.SetActive (false);
				} else {
					this.emptySeatImages [i].gameObject.SetActive (false);
					this.seatButtons [i].gameObject.SetActive (true);
				}
			}
		}
	}

	public void SetSeatClick() {
		//Seat[] seats = gamePlayerController.game.seats;

		foreach (Button button in seatButtons) {
			
			button.onClick.AddListener( Seat );
		}
	}

	public void Seat() {
		string seatName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
		Debug.Log ("seat: " + seatName);

		int seq = int.Parse(seatName[seatName.Length - 1] + "");

		Seat[] seats = gamePlayerController.game.seats;

		Socket socket = gamePlayerController.gameSocket;

		var seatReq = new {
			roomNo = gamePlayerController.game.roomNo,
			seat = seats[seq].seatNo,
			userId = Player.Me.userId,
		};

		socket.EmitJson (Messages.Seat, JsonConvert.SerializeObject (seatReq), (msg) => {
			isSeat = true;
			seats[seq].player = Player.Me;
			SetPlayerSeatUI();

			moveSeats(seq);

		});
	}

	//移动玩家
	private void moveSeats(int seatIndex) {
		int[] originalUsers = new int[6];
		for (int i = 0; i < Game.SeatCount; i++) {
			originalUsers[i] = i;
		}

		int[] destUsers = new int[6];
		destUsers [0] = seatIndex;

		for(int i = 1; i < Game.SeatCount; i++) {
			destUsers[i] = (seatIndex + i) % 6;
		}

		Player[] players = new Player[Game.SeatCount];
		int[] positions = new int[Game.SeatCount];
		Seat[] seats = gamePlayerController.game.seats;
		for (int i = 0; i < Game.SeatCount; i++) {
			players [i] = seats [i].player;
			positions [i] = seats [i].seatNo;
		}

		for (int i = 0; i < Game.SeatCount; i++) {
			seats [i].player = players [destUsers[i]];
			seats [i].seatNo = positions [destUsers[i]];
		}

		this.fromPositions = destUsers;
		this.toPositions = positions;
		isMoveSeat = true;
	}




}
