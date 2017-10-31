using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class RobBankerController : MonoBehaviour {

	[SerializeField]
	private GamePlayController gamePlayerController; 

	[SerializeField]
	private GameObject robRankerPanel;

	private bool hasRobBanker = false; 

	public void Reset() {
		hasRobBanker = false;
	}

	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("game state is : " + gamePlayerController.state.value);
		if (gamePlayerController.state == GameState.RobBanker && !hasRobBanker) {
			robRankerPanel.gameObject.SetActive (true);
		} else {
			robRankerPanel.gameObject.SetActive (false);
		}
			
	}

	public void RobClick() {
		if (gamePlayerController.state == GameState.RobBanker) {

			gamePlayerController.gameSocket.EmitJson (Messages.RobBanker, JsonConvert.SerializeObject(new {userId = ""}), (string msg) => {
				robRankerPanel.gameObject.SetActive (false);
				hasRobBanker = true;
			}); 

		}
	}

	public void NotRobClick() {
		
		if (gamePlayerController.state == GameState.RobBanker) {

			gamePlayerController.gameSocket.EmitJson (Messages.RobBanker, JsonConvert.SerializeObject(new {userId = ""}), (string msg) => {
				robRankerPanel.gameObject.SetActive (false);
				hasRobBanker = true;
			});  

		}
	}
}
