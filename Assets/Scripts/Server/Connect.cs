using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using socket.io;
using Newtonsoft.Json;

public class Connect : MonoBehaviour {
	private const float retryTimeInterval = 5000; //服务器中断，重连间隔时间

	[SerializeField]
	private GamePlayController gamePlayController;

	public Socket gameSocket;

	private bool isConnecting = false;
	private float timeLeft = retryTimeInterval;
	private string roomNo = "";
	private string serverUrl = "";

	void Start() {
		SetServerUrlAndRoomNo ();
		connect (serverUrl, roomNo);
	}

	void Update () {
		if (!gamePlayController.isConnected && timeLeft >= 0) {
			timeLeft -= Time.deltaTime;
		}

		if (!gamePlayController.isConnected && timeLeft < 0 ) {
			timeLeft = retryTimeInterval;
			Debug.Log ("retry connect");
			connect (serverUrl, roomNo);
		}

		//Debug.Log ("isConnected = " + gamePlayController.isConnected + ", timeLeft = " + timeLeft);
	}

	private void SetServerUrlAndRoomNo() {
		Dictionary<string, string> parameters = Scenes.getSceneParameters ();
		serverUrl = ServerUtils.protocol + "://" + ServerUtils.mainServer + ":" + ServerUtils.port;
	    roomNo = "123456";
		if (parameters != null) {
			serverUrl = parameters ["serverUrl"];
			roomNo = parameters ["roomNo"];
		}
		Debug.Log ("serverUrl = " + serverUrl);
	}

	public void connect(string serverUrl, string roomNo) {
		if (isConnecting)
			return;
		Debug.Log ("connecting to server ...");
		isConnecting = true;
		//var serverUrl = "http://localhost:3000";

		gameSocket = SocketManager.Instance.GetSocket (serverUrl);

		if (gameSocket == null) {
			Debug.Log ("SocketManager.Instance.GetSocket return null");
			gameSocket = Socket.Connect (serverUrl);
		} else {
			gameSocket.enabled = true;
			gameSocket.EmitJson ("", "");
			Debug.Log ("socket.isConnected = " + gameSocket.IsConnected);
			Debug.Log ("SocketManager.Instance.GetSocket return not null");
			if (gameSocket.IsConnected) {
				gamePlayController.isConnected = true;
				gamePlayController.SetGameSocket(gameSocket);
				JoinRoom();
				gamePlayController.isConnected = true;
			}
		}
			
		gameSocket.On(SystemEvents.connect, () => {
			Debug.Log("连接成功");
			if (gamePlayController == null)
				return;
			gamePlayController.isConnected = true;
			gamePlayController.SetGameSocket(gameSocket);
			JoinRoom();
			gamePlayController.isConnected = true;
		});

		gameSocket.On(SystemEvents.reconnect, (int reconnectAttempt) => {
			Debug.Log("重连成功! " + reconnectAttempt);
			gamePlayController.isConnected = true;
			JoinRoom();
		});

		gameSocket.On(SystemEvents.disconnect, () => {
			Debug.Log("连接中断~");
			gamePlayController.isConnected = false;
		});

		isConnecting = false;
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
		});
		Debug.Log ("after emitJson");
	}
}
