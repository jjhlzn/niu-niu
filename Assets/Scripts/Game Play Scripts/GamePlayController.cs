using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;



public class GamePlayController : MonoBehaviour {
	[SerializeField]
	private SetupCardGame setupCardGame;

	[SerializeField]
	private FirstDealerController firstDealerController;

	public GameState state;


	// Use this for initialization
	void Start () {
		Debug.Log ("GamePlayController Start");
		state = GameState.Ready;
	}
		

	public void StartClick() {
		Debug.Log ("start game click");
		setupCardGame.resetCards ();
		state = GameState.FirstDeal;

	}
		

}
