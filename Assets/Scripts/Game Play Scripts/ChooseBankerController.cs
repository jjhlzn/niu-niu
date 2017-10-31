using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseBankerController : MonoBehaviour {
	[SerializeField]
	private GamePlayController gamePlayController;


	void Update() {
		
	}

	public void HandleResponse(GoToChooseBankerNotity resp) {
		gamePlayController.game.currentRound.banker = resp.banker;

		gamePlayController.state = GameState.ChooseBanker;


		gamePlayController.goToNextState ();

	}
		
}
