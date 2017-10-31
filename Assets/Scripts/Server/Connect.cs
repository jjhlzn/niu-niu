using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using socket.io;

public class Connect : MonoBehaviour {

	public static string GameInstructionEvent = "game instruction";

	[SerializeField]
	private GamePlayController gamePlayController;

	private Socket gameSocket;

	// Use this for initialization
	void Start() {
		var serverUrl = "http://localhost:3000";
		var gameSocket = Socket.Connect(serverUrl);

		gameSocket.On(SystemEvents.connect, () => {
			Debug.Log("Hello, Socket.io~");
			gamePlayController.SetGameSocket(gameSocket);
		});

		gameSocket.On(SystemEvents.reconnect, (int reconnectAttempt) => {
			Debug.Log("Hello, Again! " + reconnectAttempt);
		});

		gameSocket.On(SystemEvents.disconnect, () => {
			Debug.Log("Bye~");
		});


	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
