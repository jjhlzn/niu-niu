using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class Experiment : MonoBehaviour {

	[SerializeField]
	private Image card;
	[SerializeField]
	private Canvas canvas;
	[SerializeField]
	private Text text;
	[SerializeField]
	private Image readyImage;

	private Image[] cards;

	private Vector3[] positions;

	// Use this for initialization
	void Start () {
		
		Setup ();
		//MoveCard ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void MoveCard() {
		text.DOText ("9000", 1, true, ScrambleMode.Numerals);  
		Sequence s = DOTween.Sequence ();
		s.SetDelay (0.1f);
		s.Append(readyImage.transform.DOScale (3f, 0.2f));
		s.SetDelay (0.1f);
		s.Append(readyImage.transform.DOScale (1f, 0.2f));

		for(int i = 0; i < cards.Length / 4; i++) {
			for (int j = 0; j < 4; j++) {
				int index = i * 4 + j;
		
			    cards [index].transform.DOLocalMove (positions [index], 1000f, false)
					.SetSpeedBased ()
					.SetDelay (index * 0.07f);
				if (i == 0)
					cards [index].transform
						.DOScale (1.3f, 0.04f)
						.SetDelay (index * 0.07f + 0.02f);
			}
		}



	}

	public void Reset() {
		text.text = "1000";
		readyImage.transform.localScale = new Vector3 (1f, 1f);
		for(int i = 0; i < cards.Length; i++) {
			cards [i].transform.position = card.transform.position;
			cards [i].transform.localScale = new Vector3 (1.1f, 1.1f, 0);
		}
	}

	private void Setup() {
		cards = new Image[24];

		positions = new Vector3[24];


		for(int i = 0; i < cards.Length; i++) {
			cards[i] = Instantiate (card);
			cards [i].transform.position = card.transform.position;
			cards [i].transform.SetParent (canvas.transform);
			cards [i].transform.localScale = new Vector3 (1.1f, 1.1f, 0);
			cards [i].gameObject.SetActive (true);
		}

		for (int i = 0; i < cards.Length / 4; i++) {
			int x = 0, y = 0;

			for (int j = 0; j < 4; j++) {
				if (i == 0) {
					x = -409 + j * 160;
					y = -333;

				} else if (i == 1) {
					x = -640 + j * 50;
					y = -26;
				} else if (i == 2) {
					x = -685 + j * 50;
					y = 280;
				} else if (i == 3) {
					x = -120 + j * 50;
					y = 290;
				} else if (i == 4) {
					x = 470 + j * 50;
					y = 210;
				}  else if (i == 5) {
					x = 515 + j * 50;
					y = -77;
				}

				positions [i * 4 + j] = new Vector3 (x, y, 0);
			}


		}

	}

}
