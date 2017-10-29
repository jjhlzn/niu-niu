using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using socket.io;


public class GamePlayController : MonoBehaviour {
	[SerializeField]
	private SetupCardGame setupCardGame;

	[SerializeField]
	private FirstDealerController firstDealerController;

	public GameState state;


	private Socket gameSocket;


	// Use this for initialization
	void Start () {
		Debug.Log ("GamePlayController Start");
		state = GameState.Ready;
	}
		

	public void StartClick() {
		Debug.Log ("start game click");

		setupCardGame.resetCards ();
		state = GameState.FirstDeal;

		var request = new {
			type = "startGame",
			data = new {
				roomNo = "123456"
			}
		};

		var json = JsonUtility.ToJson (request);
		Debug.Log ("startGameJson: " + json);
		gameSocket.Emit (Connect.GameInstructionEvent, json);

	}

	public void goToNextState() {
		state = state.nextState ();
	}

	public void SetGameSocket(Socket socket) {
		gameSocket = socket;
	}

}
