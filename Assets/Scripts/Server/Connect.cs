using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using socket.io;
using Newtonsoft.Json;
using System;

public class Connect : MonoBehaviour {
	private const float retryTimeInterval = 5; //服务器中断，重连间隔时间

	[SerializeField]
	private GamePlayController gamePlayController;

	public Socket gameSocket;

	private bool isConnecting = false;
	private bool hasConnected = false;
	private float timeLeft = retryTimeInterval;
	private string roomNo = "";
	private string serverUrl = "";
	private DateTime startConnTime = DateTime.Now; 

	void Start() {
		SetServerUrlAndRoomNo ();
		connect ();
	}

	void Update () {
		if (!gamePlayController.isConnected && timeLeft >= 0) {
			timeLeft -= Time.deltaTime;
		}

		//Debug.Log ("gamePlayController.isConnected = " + gamePlayController.isConnected + ", timeLeft = " + timeLeft);
		if (!gamePlayController.isConnected && timeLeft < 0 ) {
			timeLeft = retryTimeInterval;
			Debug.Log ("retry connect");
			connect ();

		}
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

		gameSocket = SocketManager.Instance.GetSocket (serverUrl);
		if (gameSocket == null) {
			Debug.Log ("SocketManager.Instance.GetSocket return null");
			gameSocket = Socket.Connect (serverUrl);
		} else {
			Debug.Log ("SocketManager.Instance.GetSocket return not null");
			//gameSocket.enabled = true;
			Debug.Log ("socket.isConnected = " + gameSocket.IsConnected);
			if (gameSocket.IsConnected) {
				gamePlayController.isConnected = true;
				gamePlayController.SetGameSocket (gameSocket);
				JoinRoom ();
				gamePlayController.isConnected = true;

				isConnecting = false;
			} else  if (!hasConnected) {
				gameSocket = Socket.Connect (serverUrl);

			}
		}

		gameSocket.On(SystemEvents.connect, () => {
			hasConnected = true;
			Debug.Log("连接成功");
			DateTime endConnTime = DateTime.Now;
			double totalMilliSecs = (endConnTime - startConnTime).TotalMilliseconds;
			Debug.Log("连接使用了" + totalMilliSecs + "ms");
			if (gamePlayController != null) {
				gamePlayController.isConnected = true;
				gamePlayController.SetGameSocket(gameSocket);
				JoinRoom();
				gamePlayController.isConnected = true;
			}
			isConnecting = false;
		});
			
		gameSocket.On(SystemEvents.reconnect, (int reconnectAttempt) => {
			Debug.Log("重连成功! " + reconnectAttempt);
			if (gamePlayController != null) {
				gamePlayController.isConnected = true;
				JoinRoom();
			}
			isConnecting = false;
		});

		gameSocket.On(SystemEvents.disconnect, () => {
			Debug.Log("连接中断~");
			if (gamePlayController != null) {
				gamePlayController.isConnected = false;
			}
			isConnecting = false;
		});
			
		gameSocket.On (SystemEvents.reconnectFailed, ConnectTimeOutHanlder);
		gameSocket.On (SystemEvents.reconnectError, ConnectErrorHandler);
		gameSocket.On(SystemEvents.connectError, ConnectErrorHandler);
		gameSocket.On(SystemEvents.connectTimeOut, ConnectTimeOutHanlder);	
	}

	private void ConnectErrorHandler(Exception ex) {
		Debug.Log ("ConnectErrorHandler called");
		Debug.LogError(ex);
		isConnecting = false;
		if (gamePlayController != null)
			gamePlayController.ShowConnectFailMessage ();
	}

	private void ConnectTimeOutHanlder() {
		Debug.Log ("ConnectTimeOutHanlder called");
		isConnecting = false;
		if (gamePlayController != null)
			gamePlayController.ShowConnectFailMessage ();
	}


	public void disconnect() {
		//Socket.Destroy (gameSocket);
	}

	void JoinRoom() {
		var joinReq = new {
			roomNo = roomNo,
			userId = Player.Me.userId
		};

		Debug.Log ("try to join room");
		gameSocket.EmitJson(Messages.JoinRoom, JsonConvert.SerializeObject(joinReq), (string msg) => {
			Debug.Log("msg = " + msg);
			JoinRoomResponse resp = JsonConvert.DeserializeObject<JoinRoomResponse[]>(msg)[0];
			if (resp.status != 0) {
				Debug.LogError("ErrorMessage: " + resp.errorMessage);
				return;
			}

			gamePlayController.HandleResponse(resp);
			timeLeft = retryTimeInterval;
			DateTime endConnTime = DateTime.Now;
			double totalMilliSecs = (endConnTime - startConnTime).TotalMilliseconds;
			Debug.Log("连接和加入房间，使用了" + totalMilliSecs + "ms");
			Debug.Log("Connect");
		});
		Debug.Log ("after emitJson");
	}

	private void SetServerUrlAndRoomNo() {
		Dictionary<string, string> parameters = Scenes.getSceneParameters ();
		serverUrl = ServerUtils.protocol + "://" + ServerUtils.mainServer + ":" + ServerUtils.socketIOPort;
		roomNo = "123456";
		if (parameters != null) {
			serverUrl = parameters ["serverUrl"];
			roomNo = parameters ["roomNo"];
		}
		Debug.Log ("serverUrl = " + serverUrl);
	}
}
