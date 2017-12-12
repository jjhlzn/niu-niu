using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using socket.io;
using Newtonsoft.Json;
using System;
using BestHTTP.SocketIO;


public class Connect : BaseMonoBehaviour {
	private const float retryTimeInterval = 5; //服务器中断，重连间隔时间

	[SerializeField]
	private GamePlayController gamePlayController;

	public Socket gameSocket;
	private SocketManager socketManager;
	private SocketOptions options;

	private bool isConnecting = false;
	private bool hasConnected = false;
	private float timeLeft = retryTimeInterval;
	private string roomNo = "";
	private string serverUrl = "";
	private DateTime startConnTime = DateTime.Now; 

	void Start() {
		SetServerUrlAndRoomNo ();

	    options = new SocketOptions ();
		options.ReconnectionAttempts = 1000000000;
		options.AutoConnect = true;
		options.ReconnectionDelay = TimeSpan.FromMilliseconds (1000);

		connect ();
	}

	void Update () {

		/*
		if (!gamePlayController.isConnected && timeLeft >= 0) {
			timeLeft -= Time.deltaTime;
		}

		//Debug.Log ("gamePlayController.isConnected = " + gamePlayController.isConnected + ", timeLeft = " + timeLeft);
		if (!gamePlayController.isConnected && timeLeft < 0 ) {
			timeLeft = retryTimeInterval;
			Debug.Log ("retry connect");
			connect ();

		}*/
		//Debug.Log ("isConnected = " + gamePlayController.isConnected + ", timeLeft = " + timeLeft);
	}
		
	public void connect() {
		if (isConnecting) {
			//Debug.Log ("isConnecting is true, return");
			return;
		}

		if (gamePlayController == null) {
			Debug.Log ("gamePlayController is null, can't connect");
			return;
		}

		Debug.Log ("connecting to server ...");
		startConnTime = DateTime.Now;
		isConnecting = true;
	
		socketManager = new SocketManager (new Uri (ServerUtils.GetSocketIOUrl()), options);
		socketManager.Socket.On(SocketIOEventTypes.Error, 
			(socket, packet, args) => { 
				Debug.LogError(string.Format("Error: {0}", args[0].ToString()));
				if (gamePlayController != null)
					gamePlayController.ShowConnectFailMessage ();
			});
		socketManager.Socket.On (SocketIOEventTypes.Disconnect, (socket, packet, eventArgs) => {
			Debug.Log("lose connection");
			if (gamePlayController != null) {
				gamePlayController.isConnected = false;
			}
			isConnecting = false;
		});
		socketManager.Socket.On(SocketIOEventTypes.Connect, (socket, packet, arg) => {
			
			gameSocket = socketManager.Socket;
			gamePlayController.connect = this;
			gamePlayController.isConnected = true;
			gamePlayController.SetGameSocket (gameSocket);
			JoinRoom ();
			gamePlayController.isConnected = true;

			isConnecting = false;
		});
		socketManager.Open();
	}

	void JoinRoom() {
		var joinReq = new {
			roomNo = roomNo,
			userId = Player.Me.userId
		};

		Debug.Log ("try to join room");
		gameSocket.Emit (Messages.JoinRoom, (socket, packet, args) => {
			string msg = packet.ToString();
			Debug.Log("msg = " + msg);
			JoinRoomResponse resp = JsonConvert.DeserializeObject<JoinRoomResponse[]>(msg)[0];
			if (resp.status != 0) {
				Debug.LogError("ErrorMessage: " + resp.errorMessage);

				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters[Utils.Message_Key] = "该房间不存在";
				Scenes.Load("MainPage",parameters);
				return;
			}

			gamePlayController.HandleResponse(resp);
			timeLeft = retryTimeInterval;
			DateTime endConnTime = DateTime.Now;
			double totalMilliSecs = (endConnTime - startConnTime).TotalMilliseconds;
			Debug.Log("连接和加入房间，使用了" + totalMilliSecs + "ms");
			Debug.Log("Connect");
		}, JsonConvert.SerializeObject (joinReq));
	}

	private void SetServerUrlAndRoomNo() {
		Dictionary<string, string> parameters = Scenes.getSceneParameters ();
		serverUrl = ServerUtils.GetSocketIOUrl();
		roomNo = "123456";
		if (parameters != null) {
			serverUrl = parameters ["serverUrl"];
			roomNo = parameters ["roomNo"];
		}
		Debug.Log ("serverUrl = " + serverUrl);
	}

	void OnDestroy() {
		socketManager.Close ();
	}
}
