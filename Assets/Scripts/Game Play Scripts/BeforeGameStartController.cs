using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using socket.io;
using Newtonsoft.Json;

public class BeforeGameStartController : BaseStateController {

	private static float moveSeatSpeed = 30f;

	[SerializeField]
	private GamePlayController gamePlayerController;

	[SerializeField]
	private SetupCardGame setUpGameController; 

	[SerializeField]
	private Button standUpButton;

	[SerializeField]
	private Button startButton;

	[SerializeField]
	private Image card;
	[SerializeField]
	private Camera cam;

	private Canvas canvas;

	public GameObject[] userPanels;
	public Image[] seatImages;
	public Image[] playerImages;
	public Text[] playerNames;
	public Text[] playerScores;
	public Button[] seatButtons;
	public Text[] seatDescs;
	public Image[] emptySeatImages;
	public Image[] isRobImages;
	public Image[] readyImages;

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

	/**
	 * 游戏在一局之后，在下一局开始之前，需要重新设置界面或者变量
	 * */
	public override void Reset() {
		foreach(Image isRobImage in isRobImages) {
			isRobImage.gameObject.SetActive (false);
		}
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

				//循转之后，我总是坐在第一个位置
				Player.Me.seat = 0;
				SetPlayerSeatUI ();
			}
		} 
	}
		

	//根据是否有玩家坐在座位上，设置位置的外观
	public void SetPlayerSeatUI() {
		
		//Player[] players = gamePlayerController.game.players;
		Seat[] seats = gamePlayerController.game.seats;

		for (int i = 0; i < seats.Length; i++) {
			Seat seat = seats [i];
			if (seat.hasPlayer()) {
				this.playerImages [i].gameObject.SetActive (true);
				this.playerNames [i].gameObject.SetActive (true);
				this.playerScores [i].gameObject.SetActive (true);
				this.seatButtons [i].gameObject.SetActive (false);
				this.seatImages [i].gameObject.SetActive (true);
				this.emptySeatImages [i].gameObject.SetActive (false);
				this.readyImages [i].gameObject.SetActive (true);
				this.playerNames [i].text = seat.player.userId;
				this.seatDescs [i].text = "座位 [" + seat.seatNo + "]";
				this.playerScores [i].text = seat.player.score + "";
			} else {
				this.playerImages [i].gameObject.SetActive (false);
				this.playerNames [i].gameObject.SetActive (false);
				this.playerScores [i].gameObject.SetActive (false);
				this.readyImages [i].gameObject.SetActive (false);
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

		if (Player.Me.seat == -1) {
			standUpButton.gameObject.SetActive (false);
		} else {
			standUpButton.gameObject.SetActive (true);
		}

		if (gamePlayerController.game.PlayerCount < 2) {
			startButton.interactable = false;
		} else {
			startButton.interactable = true;
		}
	}

	public void StartClick() {
		Socket gameSocket = gamePlayerController.gameSocket;
		if (gameSocket == null || !gameSocket.IsConnected) {
			return;
		}

		setUpGameController.resetCards ();
		Debug.Log ("start game click");


		//make start game request
		var request = new {
			roomNo = gamePlayerController.game.roomNo,
			userId = Player.Me.userId
		};

		gameSocket.EmitJson (Messages.StartGame, JsonConvert.SerializeObject(request), (string msg) => {
			foreach(Image image in readyImages) {
				image.gameObject.SetActive(false);
			}

			startButton.gameObject.SetActive(false);
			standUpButton.gameObject.SetActive(false);
		}); 
	}

	public void ReadyClick() {
		Socket gameSocket = gamePlayerController.gameSocket;
		Debug.Log ("ready  click");
	
		//make start game request
		var request = new {
			roomNo = gamePlayerController.game.roomNo,
			userId = Player.Me.userId
		};

		gameSocket.EmitJson (Messages.StartGame, JsonConvert.SerializeObject(request), (string msg) => {
			setUpGameController.resetCards ();

			foreach(Image image in readyImages) {
				image.gameObject.SetActive(false);
			}

			startButton.gameObject.SetActive(false);
			standUpButton.gameObject.SetActive(false);
		}); 
	}
		

	public void SetSeatClick() {
		foreach (Button button in seatButtons) {
			button.onClick.AddListener( SitDown );
		}
	}

	public void SitDown() {
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

		socket.EmitJson (Messages.SitDown, JsonConvert.SerializeObject (seatReq), (msg) => {
			isSeat = true;
			seats[seq].player = Player.Me;
			seats[seq].player.seat = seq;

			SetPlayerSeatUI();
			MoveSeats(seq);
		});
	}

	public void StandUp() {
		Socket socket = gamePlayerController.gameSocket;

		var standUpReq = new {
			roomNo = gamePlayerController.game.roomNo,
			seat = Player.Me.seat,
			userId = Player.Me.userId,
		};

		socket.EmitJson (Messages.StandUp, JsonConvert.SerializeObject (standUpReq), (msg) => {
			isSeat = false;

			Debug.Log("standup from seat: " + Player.Me.seat );
			gamePlayerController.game.seats[Player.Me.seat].player = null;
			Player.Me.seat = -1;

			SetPlayerSeatUI();

		});
	}

	//移动玩家
	private void MoveSeats(int seatIndex) {
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
		string[] positions = new string[Game.SeatCount];
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
		this.toPositions = originalUsers;

		isMoveSeat = true;
	}

	public void HandleResponse(SomePlayerSitDownNotify notify){
		string seatNo = notify.seat;
		Seat[] seats = gamePlayerController.game.seats;

		//Debug.Log ("seatNo = " + seatNo);
		int i = 0;
		foreach (Seat seat in seats) {
			//Debug.Log ("seat.seatNo =  " + seat.seatNo);
			//Debug.Log ("seat.player = " + seat.player);
			if (seat.seatNo == seatNo && seat.player == null) {
				Player player = new Player ();
				player.userId = notify.userId;
				player.seat = i;
				seat.player = player;
				SetPlayerSeatUI ();
				break;
			}
			i++;
		}
	}

	public void HandleResponse(SomePlayerStandUpNotify notify) {
		foreach (Seat seat in gamePlayerController.game.seats) {
			if (seat.player != null && seat.player.userId == notify.userId) {
				seat.player = null;
				SetPlayerSeatUI ();
			}
		}
		return;
	}

	public void HandleResponse(SomePlayerReadyNotify notify) {
		int seatIndex = gamePlayerController.game.GetSeatIndex (notify.userId);
		readyImages [seatIndex].gameObject.SetActive (true);
	}

	#region 设置UI的元素
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

	public void SetIsRobImages(Image[] isRobImages) {
		this.isRobImages = isRobImages;
	}

	public void SetReadyImages(Image[] readyImages) {
		this.readyImages = readyImages;
	}
	#endregion


}
