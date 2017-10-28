using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobBankerController : MonoBehaviour {

	[SerializeField]
	private GamePlayController gamePlayerController; 

	[SerializeField]
	private GameObject robRankerPanel;
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("game state is : " + gamePlayerController.state.value);
		if (gamePlayerController.state == GameState.RobBanker) {
			robRankerPanel.gameObject.SetActive (true);
		} else {
			robRankerPanel.gameObject.SetActive (false);
		}
			
	}

	public void RobClick() {
		gamePlayerController.goToNextState ();
	}

	public void NotRobClick() {
		gamePlayerController.goToNextState ();
	}
}
