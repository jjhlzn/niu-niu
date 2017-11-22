using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using socket.io;
using Newtonsoft.Json;

public class Connect : MonoBehaviour {
	private const float retryTimeInterval = 5000; //服务器中断，重连间隔时间

	[SerializeField]
	private GamePlayController gamePlayController;

	private Socket gameSocket;

	private bool isConnecting = false;
	private float timeLeft = retryTimeInterval; 

	void Start() {
		connect ();
	}

	void Update () {
		if (!gamePlayController.isConnected && timeLeft >= 0) {
			timeLeft -= Time.deltaTime;
		}

		if (!gamePlayController.isConnected && timeLeft < 0 ) {
			timeLeft = retryTimeInterval;
			Debug.Log ("retry connect");
			connect ();
		}

		//Debug.Log ("isConnected = " + gamePlayController.isConnected + ", timeLeft = " + timeLeft);
	}

	void connect() {
		if (isConnecting)
			return;
		Debug.Log ("connecting to server ...");
		isConnecting = true;
		var serverUrl = "http://192.168.31.175:3000";
		gameSocket = Socket.Connect(serverUrl);

		gameSocket.On(SystemEvents.connect, () => {
			Debug.Log("连接成功");
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

	void JoinRoom() {
		var joinReq = new {
			roomNo = gamePlayController.GenerateRoomNo(),
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
	}
}
