using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseBankerController : MonoBehaviour {
	[SerializeField]
	private GamePlayController gamePlayController;


	void Update() {
		if (gamePlayController.state == GameState.ChooseBanker) {
			gamePlayController.goToNextState ();
		}
	}
		
}
