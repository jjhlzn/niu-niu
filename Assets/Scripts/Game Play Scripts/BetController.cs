using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetController : MonoBehaviour {

	[SerializeField]
	private GamePlayController gamePlayController;

	[SerializeField]
	private GameObject betPanel;

	
	// Update is called once per frame
	void Update () {
		if (gamePlayController.state == GameState.Bet) {
			betPanel.SetActive (true);
		} else {
			betPanel.SetActive (false);
		}
	}

	public void BetClick() {
		if (gamePlayController.state == GameState.Bet) {
			gamePlayController.goToNextState ();
		}
	}
}
