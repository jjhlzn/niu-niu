using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using socket.io;
using cn.sharesdk.unity3d;
using Newtonsoft.Json;
using DG.Tweening;

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
	private Button readyButton;
	[SerializeField]
	private Button shareButton;
	[SerializeField]
	private Button dismissRoomBtn;
	[SerializeField]
	private Button leaveRoomBtn;

	[SerializeField]
	private ShareSDK ssdk;


	[SerializeField]
	private Image card;

	private Seat[] seats {
		get {
			return gamePlayerController.game.seats;
		}
	}

	public bool isMoveSeat;
	private int[] fromPositions;
	private int[] toPositions;

	private Vector3[] positions;

	//private bool isSeat;

	// Use this for initialization
	void Start () {
		ssdk.shareHandler = ShareResultHandler;

		positions = new Vector3[seats.Length];
		for(int i = 0; i < seats.Length; i++) {
			positions [i] = seats [i].playerPanel.transform.position;
		}
		SetSeatClick ();

		isMoveSeat = false;
		//isSeat = false;

	}

	/**
	 * 游戏在一局之后，在下一局开始之前，需要重新设置界面或者变量
	 * */
	public override void Reset() {
		isMoveSeat = false;
	}

	public override GamePlayController GetGamePlayController ()
	{
		return gamePlayerController;
	}
	
	// Update is called once per frame
	public new void Update ()  {
		base.Update ();

		if (gamePlayerController.state == GameState.BeforeStart) {
			gamePlayerController.game.ShowStateLabel ("等待其他玩家加入...");
		} 
			
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

				foreach (Seat seat in seats) {
					seat.UpdateUI (gamePlayerController.game);
				}

				gamePlayerController.ResetUI ();
			}
		} 
	}


	public int getMoveSeatIndex() {
		var game = gamePlayerController.game;
		int seatIndex = -1;
		Debug.Log ("game.seats = " + seats);
		Debug.Log ("game.seats.length = " + seats.Length);
		if (Player.Me.isPlaying && Player.Me.seat != null && Player.Me.seat.seatIndex != 0) { 
			seatIndex = Player.Me.seat.seatIndex;
		} else if (game.state != GameState.BeforeStart && !seats [0].hasPlayer ()) {
			for (int i = 0; i < Game.SeatCount; i++) {
				Debug.Log ("seats[" + i + "].hasPlayer() = " + seats [i].hasPlayer ());
				if (seats [i].hasPlayer () && seats[i].player.isPlaying) {
					seatIndex = i;
					break;
				}
			}
		}
		return seatIndex;
	}

	//没有做在
	public bool IsNeedMoveSeat() {
		return getMoveSeatIndex () != -1;
	}
		
	public void StartClick() {
		Debug.Log ("start game click");

		Socket gameSocket = gamePlayerController.gameSocket;

		//make start game request
		var request = new {
			roomNo = gamePlayerController.game.roomNo,
			userId = Player.Me.userId
		};

		gameSocket.EmitJson (Messages.StartGame, JsonConvert.SerializeObject(request), (string msg) => {
			foreach(Seat seat in seats) {
				seat.readyImage.gameObject.SetActive(false);
			}

			UpdateButtonStatusAfterStart();
		}); 
	}


	private void HandleMeReady() {
		readyButton.gameObject.SetActive (false);
	}
		
	public void SetSeatClick() {
		foreach (Seat seat in seats) {
			seat.sitdownButton.onClick.AddListener( SitDown );
		}
	}

	public void SitDown() {
		var game = gamePlayerController.game;
		string seatName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

		//Debug.Log ("seat: " + seatName);
		int seatIndex = int.Parse(seatName[seatName.Length - 1] + "");

		Socket socket = gamePlayerController.gameSocket;
		var seatReq = new {
			roomNo = gamePlayerController.game.roomNo,
			seat = seats[seatIndex].seatNo,
			userId = Player.Me.userId,
		};
			
		socket.EmitJson (Messages.SitDown, JsonConvert.SerializeObject (seatReq), (msg) => {
			Debug.Log("msg: " + msg);
			BaseGameResponse resp = JsonConvert.DeserializeObject<BaseGameResponse[]>(msg)[0];

			if (resp.status != 0) {
				Debug.LogError("status = " + resp.status + ", message = " + resp.errorMessage);
				return;
			}
				
			leaveRoomBtn.interactable = false;

			if (Player.Me.userId != game.creater)
				standUpButton.interactable = true;
			if (gamePlayerController.game.PlayerCount < 2) {
				startButton.interactable = false;
			} else {
				startButton.interactable = true;
			} 

			seats[seatIndex].player = Player.Me;
			Player.Me.seat = seats[seatIndex];
			Debug.Log("Player.Me.seat.seatIndex = " + Player.Me.seat.seatIndex);

			foreach(Seat seat in seats) {
				seat.UpdateUI(gamePlayerController.game);
			}

			if (game.state == GameState.BeforeStart || game.state == GameState.WaitForNextRound) {
				MoveSeats(seatIndex);
				Player.Me.isPlaying = true;
			} else {
				Player.Me.isPlaying = false;
			}

			if (game.state == GameState.BeforeStart) {
				if (gamePlayerController.game.PlayerCount >= 2) {
					startButton.interactable = true;
				} else {
					startButton.interactable = false;
				} 
			} 
				
			if (game.state == GameState.WaitForNextRound) {
				readyButton.gameObject.SetActive(true);
			}
		});
	}

	//移动玩家
	public void MoveSeats(int seatIndex) {
		Debug.Log ("MoveSeats is called");

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
			seats [i].seatNo = seatNos [destIndexes[i]];
			seats [i].player = players [destIndexes[i]];
			if (players [destIndexes [i]] != null ) {
				players [destIndexes [i]].seat = seats [i];
			} 
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
			
			BaseGameResponse resp = JsonConvert.DeserializeObject<BaseGameResponse[]>(msg)[0];

			if (resp.status != 0) {
				Debug.LogError("status = " + resp.status + ", message = " + resp.errorMessage);
				return;
			}

			if (Player.Me.userId != gamePlayerController.game.creater) {
				leaveRoomBtn.interactable = true;
			}

			standUpButton.interactable = false;

			Debug.Log("standup from seat: " + Player.Me.seat );

			Player.Me.Standup();

			foreach(Seat seat in seats) {
				seat.UpdateUI(gamePlayerController.game);
			}
		});

		setUpGameController.CloseMenuClick ();
	}

	public void ShareClick() {
		Debug.Log ("Share Click");
		var game = gamePlayerController.game;
		ShareContent content = new ShareContent();
		content.SetTitle("房间【" + game.roomNo + "】");
		content.SetText("【玩法：AA支付，" + game.totalRoundCount + "局，【4，6，8分】，明牌抢庄，闲家推注】");
		content.SetImageUrl("http://is5.mzstatic.com/image/thumb/Purple18/v4/d7/7e/2a/d77e2a15-3898-8fcf-9ea9-7e48a0593af0/source/512x512bb.jpg");
		content.SetUrl("http://niu.yhkamani.com/share?room="+game.roomNo);
		//content.SetUrlDescription("【玩法：AA支付，" + game.totalRoundCount + "局，【4，6，8分】，明牌抢庄，闲家推注】");
		//content.SetDesc ("【玩法：AA支付，" + game.totalRoundCount + "局，【4，6，8分】，明牌抢庄，闲家推注】");

		content.SetShareType (ContentType.Webpage);
		ssdk.ShareContent (PlatformType.WeChat, content);
	}


	void ShareResultHandler (int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		if (state == ResponseState.Success)
		{
			print ("share result :");
			print (MiniJSON.jsonEncode(result));
		}
		else if (state == ResponseState.Fail)
		{
			print ("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
		}
		else if (state == ResponseState.Cancel) 
		{
			print ("cancel !");
		}
	}


	public void HandleResponse(SomePlayerSitDownNotify notify){
		string seatNo = notify.seat;
		int i = 0;
		foreach (Seat seat in seats) {
			if (seat.seatNo == seatNo && seat.player == null) {
				Player player = new Player ();
				if (notify.userId == Player.Me.userId)
					player = Player.Me;
				player.userId = notify.userId;
				player.headimgurl = notify.headimgurl;
				player.nickname = notify.nickname;
				player.sex = notify.sex;
				player.ip = notify.ip;
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
		foreach (Seat seat in seats) {
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

		if (notify.userId == Player.Me.userId) {
			HandleMeReady ();
		} else {
			seats [seatIndex].readyImage.gameObject.SetActive (true);
		}
	}

	public void HandleResponse(SomePlayerDeleteNotify notify) {
		if (notify.roomNo != gamePlayerController.game.roomNo)
			return;

		if (notify.userId == Player.Me.userId)
			return;
		
		HandleDelegate (true, gamePlayerController.game.GetSeatIndex (notify.userId));
	}

	public void HandleResponse(SomePlayerNotDeleteNotify notify) {
		if (notify.roomNo != gamePlayerController.game.roomNo)
			return;

		if (notify.userId == Player.Me.userId)
			return;

		HandleDelegate (false, gamePlayerController.game.GetSeatIndex (notify.userId));
	}

	private void HandleDelegate(bool isDelegte, int seatIndex) {
		if (seatIndex == -1)
			return;
	
		var game = gamePlayerController.game;
		var player = seats [seatIndex].player;
		player.isDelegate = isDelegte;
		player.seat.UpdateUI (gamePlayerController.game);

	}

	public void HandleResponse(RoomHasDismissedNotify notify) {
		if (notify.status != 0)
			return;

		if (gamePlayerController.game.roomNo != notify.roomNo)
			return;

		if (gamePlayerController.game.creater == Player.Me.userId)
			return;

		Dictionary<string, string> parameters = new Dictionary<string, string> ();
		parameters [Utils.Message_Key] = "该房间已被解散";
		Scenes.Load ("MainPage", parameters);
	}

	public void UpdateButtonStatusAfterStart() {
		startButton.gameObject.SetActive(false);
		shareButton.gameObject.SetActive(false);
		readyButton.gameObject.SetActive (false);
		leaveRoomBtn.interactable = false;
		dismissRoomBtn.interactable = false;
		standUpButton.interactable = false;
	}


	public void SetUI() {
		var game = gamePlayerController.game;
		Debug.Log ("game.roomNo = " + game.roomNo);
		//需要循转座位
		if ( IsNeedMoveSeat() ) {
			//first has player seat index
			int seatIndex = getMoveSeatIndex(); 
			MoveSeats (seatIndex);
			isMoveSeat = false;
			for (int i = 0; i < Game.SeatCount; i++) {
				if (seats[i].hasPlayer())
					seats [i].player.seat = seats [i];
			}
		}

		for (int i = 0; i < game.seats.Length; i++) {
			game.seats [i].UpdateUI (game);
		}

		if (gamePlayerController.game.PlayerCount < 2) {
			startButton.interactable = false;
		} else {
			startButton.interactable = true;
		} 

		if (game.state == GameState.BeforeStart) {
			startButton.gameObject.SetActive (true);
			shareButton.gameObject.SetActive (true);
			if (Player.Me.isPlaying && Player.Me.userId != game.creater)
				standUpButton.interactable = true;
			else
				standUpButton.interactable = false;

			//房主不能离开房间
			if (game.creater == Player.Me.userId)
				leaveRoomBtn.interactable = false;
			else
				leaveRoomBtn.interactable = true;

			if (Player.Me.seat != null)
				leaveRoomBtn.interactable = false;
		} else {
			startButton.gameObject.SetActive (false);
			shareButton.gameObject.SetActive (false);
			dismissRoomBtn.interactable = false;
			leaveRoomBtn.interactable = false;
			standUpButton.interactable = false;
		}


		if (game.state == GameState.WaitForNextRound) {
			readyButton.gameObject.SetActive (true);
		} else {
			readyButton.gameObject.SetActive (false);
		}

		if (Player.Me.userId != game.creater) {
			startButton.gameObject.SetActive (false);
			dismissRoomBtn.gameObject.SetActive (false);
		}
	}
}
