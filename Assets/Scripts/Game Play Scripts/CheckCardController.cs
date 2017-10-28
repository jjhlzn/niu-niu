using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckCardController : MonoBehaviour {

	[SerializeField] 
	private GamePlayController gamePlayController;

	[SerializeField]
	private GameObject checkCardPanel;

	private List<Image> deckCards;
	public Sprite[] cardSprites;

	void Update() {
		//Debug.Log ("state = " + gamePlayController.state.value);
		if (gamePlayController.state == GameState.CheckCard) {
			checkCardPanel.SetActive (true);
		} else {
			checkCardPanel.SetActive (false);
		}
	}

	IEnumerator TurnCardUp(Image card) {
		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("TurnUp");
		yield return new WaitForSeconds (.4f);
		card.sprite = cardSprites[0];


		//card.transform.localEulerAngles = new Vector3(0,360,0);
		anim.Play ("TurnBackNow2");
		yield return new WaitForSeconds (.1f);
	}

	IEnumerator TurnUser1Cards() {
		for (int i = 4; i < 10; i++) {
			if (i == 8)
				continue;
			Image card = deckCards [i];
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("TurnUp");
		}

		yield return new WaitForSeconds (.4f);

		for (int i = 4; i < 10; i++) {
			if (i == 8)
				continue;
			Image card = deckCards [i];
			card.sprite = cardSprites[0];
		}


		//card.transform.localEulerAngles = new Vector3(0,360,0);

		for (int i = 4; i < 10; i++) {
			if (i == 8)
				continue;
			Image card = deckCards [i];
			Animator anim = card.GetComponent<Animator> ();
			anim.Play ("TurnBackNow2");
		}
		yield return new WaitForSeconds (.1f); 
	}

	public void CuoCardClick() {
		if (gamePlayController.state == GameState.CheckCard) {

			//user1 亮牌
			StartCoroutine(TurnCardUp(deckCards[8]));
			StartCoroutine (TurnUser1Cards ());

			gamePlayController.goToNextState ();
		}
	}

	public void ShowCardClick() {
		if (gamePlayController.state == GameState.CheckCard) {
			//user1 亮牌
			StartCoroutine(TurnCardUp(deckCards[8]));
			StartCoroutine (TurnUser1Cards ());

			gamePlayController.goToNextState ();
		}
	}

	public void SetDeckCards(List<Image> cards) {
		this.deckCards = cards;
	}

	public void SetCardSprites(Sprite[] cardSprites) {
		this.cardSprites = cardSprites;
	}

}
