using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using socket.io;

public class Connect : MonoBehaviour {

	// Use this for initialization
	void Start() {
		var serverUrl = "http://localhost:3000";
		var socket = Socket.Connect(serverUrl);

		socket.On(SystemEvents.connect, () => {
			Debug.Log("Hello, Socket.io~");
		});

		socket.On(SystemEvents.reconnect, (int reconnectAttempt) => {
			Debug.Log("Hello, Again! " + reconnectAttempt);
		});

		socket.On(SystemEvents.disconnect, () => {
			Debug.Log("Bye~");
		});
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
