using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using socket.io;
using Newtonsoft.Json;

public class BetController : MonoBehaviour {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private GameObject betPanel;

	private bool hasBet = false;

	public void Reset() {
		hasBet = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (gamePlayController.state == GameState.Bet && !hasBet) {
			betPanel.SetActive (true);
		} else {
			betPanel.SetActive (false);
		}
	}

	public void BetClick() {
		if (gamePlayController.state == GameState.Bet) {

			Socket socket = gamePlayController.gameSocket; 

			var req = new {
				userId = "",
				bet = 8
			};

			socket.EmitJson (Messages.Bet, JsonConvert.SerializeObject(req), (string msg) => {
				betPanel.gameObject.SetActive (false);
				hasBet = true;
			}); 
		}
	}
}
