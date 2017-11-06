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

	private Seat[] seats;

	private bool isMoveSeat;
	private int[] fromPositions;
	private int[] toPositions;

	private Vector3[] positions;

	private bool isSeat;

	// Use this for initialization
	void Start () {
		seats = gamePlayerController.game.seats;

		positions = new Vector3[seats.Length];
		for(int i = 0; i < seats.Length; i++) {
			positions [i] = seats [i].playerPanel.transform.position;
		}
		SetSeatClick ();

		isMoveSeat = false;
		isSeat = false;
	}

	/**
	 * 游戏在一局之后，在下一局开始之前，需要重新设置界面或者变量
	 * */
	public override void Reset() {

	}
	
	// Update is called once per frame
	void Update () {
		if (isMoveSeat) {
			
			float step = moveSeatSpeed * Time.deltaTime;
			bool[] moveCompletedArray = new bool[Game.SeatCount]; 
			for (int i = 0; i < fromPositions.Length; i++) {

				int fromSeatIndex = fromPositions [i], toSeatIndex = toPositions [i];
				//Debug.Log ("move seat: " + fromPositions[i] + " --> " + toPositions[i]);
				GameObject fromSeat = seats[fromSeatIndex].playerPanel;
				Vector3 toPosition = positions [toSeatIndex];
				fromSeat.transform.position = Vector3.MoveTowards(fromSeat.gameObject.transform.position, toPosition, step);

				if (Utils.isTwoPositionIsEqual(fromSeat.gameObject.transform.position, toPosition)) {
					moveCompletedArray [i] = true;
				}
			}

			bool isMoveCompleted = true;
			for (int i = 0; i < fromPositions.Length; i++) {
				isMoveCompleted = isMoveCompleted && moveCompletedArray [i];
			}

			if (isMoveCompleted) {
				isMoveSeat = false;
				for (int i = 0; i < Game.SeatCount; i++) {
					seats[i].playerPanel.transform.position = positions [i];
				}

				//循转之后，我总是坐在第一个位置
				Player.Me.seat = seats[0];
				seats [0].player = Player.Me;
				foreach (Seat seat in seats) {
					seat.UpdateUI (gamePlayerController.game);
				}
			}
		} 
	}

	public void StartClick() {
		Debug.Log ("start game click");

		Socket gameSocket = gamePlayerController.gameSocket;
		if (gameSocket == null || !gameSocket.IsConnected) {
			return;
		}

		//make start game request
		var request = new {
			roomNo = gamePlayerController.game.roomNo,
			userId = Player.Me.userId
		};

		gameSocket.EmitJson (Messages.StartGame, JsonConvert.SerializeObject(request), (string msg) => {
			foreach(Seat seat in seats) {
				seat.readyImage.gameObject.SetActive(false);
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
			//setUpGameController.resetCards ();
		

			startButton.gameObject.SetActive(false);
			standUpButton.gameObject.SetActive(false);
		}); 
	}
		

	public void SetSeatClick() {
		foreach (Seat seat in seats) {
			seat.sitdownButton.onClick.AddListener( SitDown );
		}
	}

	public void SitDown() {
		string seatName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

		Debug.Log ("seat: " + seatName);
		int seatIndex = int.Parse(seatName[seatName.Length - 1] + "");


		Socket socket = gamePlayerController.gameSocket;
		var seatReq = new {
			roomNo = gamePlayerController.game.roomNo,
			seat = seats[seatIndex].seatNo,
			userId = Player.Me.userId,
		};
			
		socket.EmitJson (Messages.SitDown, JsonConvert.SerializeObject (seatReq), (msg) => {
			isSeat = true;

			standUpButton.gameObject.SetActive(true);
			if (gamePlayerController.game.PlayerCount < 2) {
				startButton.interactable = false;
			} else {
				startButton.interactable = true;
			} 

			seats[seatIndex].player = Player.Me;
			Player.Me.seat = seats[seatIndex];
			Debug.Log("Player.Me.userId = " + Player.Me.userId);

			foreach(Seat seat in seats) {
				seat.UpdateUI(gamePlayerController.game);
			}

			MoveSeats(seatIndex);
		});
	}

	//移动玩家
	private void MoveSeats(int seatIndex) {
		//如果坐在第一个位置，就不需要移动
		if (seatIndex == 0)
			return;

		int[] originIndexes = new int[6];
		for (int i = 0; i < Game.SeatCount; i++) {
			originIndexes[i] = i;
		}

		int[] destIndexes = new int[6];
		destIndexes [0] = seatIndex;

		for(int i = 1; i < Game.SeatCount; i++) {
			destIndexes[i] = (seatIndex + i) % 6;
		}

		Player[] players = new Player[Game.SeatCount];
		string[] seatNos = new string[Game.SeatCount];
		for (int i = 0; i < Game.SeatCount; i++) {
			players [i] = seats [i].player;
			seatNos [i] = seats [i].seatNo;
		}

		for (int i = 0; i < Game.SeatCount; i++) {
			seats [i].player = players [destIndexes[i]];
			seats [i].seatNo = seatNos [destIndexes[i]];
		}

		this.fromPositions = destIndexes;
		this.toPositions = originIndexes;

		isMoveSeat = true;
	}


	public void StandUp() {
		Socket socket = gamePlayerController.gameSocket;

		var standUpReq = new {
			roomNo = gamePlayerController.game.roomNo,
			seat = Player.Me.seat.seatNo,
			userId = Player.Me.userId,
		};

		socket.EmitJson (Messages.StandUp, JsonConvert.SerializeObject (standUpReq), (msg) => {
			isSeat = false;

			Debug.Log("standup from seat: " + Player.Me.seat );

			Player.Me.Standup();

			foreach(Seat seat in seats) {
				seat.UpdateUI(gamePlayerController.game);
			}
		});
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
				player.seat = seat;
				seat.player = player;
				seat.UpdateUI(gamePlayerController.game);
				break;
			}
			i++;
		}

		if (gamePlayerController.game.PlayerCount < 2) {
			startButton.interactable = false;
		} else {
			startButton.interactable = true;
		} 
	}

	public void HandleResponse(SomePlayerStandUpNotify notify) {
		foreach (Seat seat in gamePlayerController.game.seats) {
			if (seat.player != null && seat.player.userId == notify.userId) {
				seat.player = null;
				foreach(Seat eachSeat in seats) {
					eachSeat.UpdateUI(gamePlayerController.game);
				}
			}
		}

		if (gamePlayerController.game.PlayerCount < 2) {
			startButton.interactable = false;
		} else {
			startButton.interactable = true;
		} 
	}

	public void HandleResponse(SomePlayerReadyNotify notify) {
		int seatIndex = gamePlayerController.game.GetSeatIndex (notify.userId);
		seats [seatIndex].readyImage.gameObject.SetActive (true);
	}
}
