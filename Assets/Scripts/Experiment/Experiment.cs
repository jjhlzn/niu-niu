using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BestHTTP;
using BestHTTP.SocketIO;
using System;
using BestHTTP.JSON;
using Newtonsoft.Json;
using cn.sharesdk.unity3d;


public class Experiment : MonoBehaviour {

	[SerializeField]
	private Image card;
	[SerializeField]
	private Canvas canvas;
	[SerializeField]
	private Text text;
	[SerializeField]
	private Image readyImage;
	[SerializeField]
	private Image[] chips;

	private Image[] cards;

	private Vector3[] positions;
	private SocketManager socketManager;

	private DateTime startTime = DateTime.Now;

	[SerializeField]
	private Image card2;
	private Image[] turnCards;


	private Sprite[] cardSprites;

	// Use this for initialization
	void Start () {
		cardSprites = Resources.LoadAll<Sprite>("sprites/cards1");
		Setup ();
		//MoveCard ();
		startTime = DateTime.Now;
		ConnectSocket();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void ShowChips() {
		Sequence s = DOTween.Sequence ();
		for (int i = 0; i < chips.Length; i++) {
			Image chip = chips [i];
			s.Append(chips[i].DOFade(1, 0.01f).OnComplete( () => {
				chip.gameObject.SetActive(true);
			}));
			s.AppendInterval (0.05f);
		}
	}



	public void ConnectSocket ()
	{
		TimeSpan e = TimeSpan.FromMilliseconds (1000);

		var options = new SocketOptions ();
		options.ReconnectionAttempts = 1000000000;
		options.AutoConnect = true;
		options.ReconnectionDelay = e;

		//Server URI
		socketManager = new SocketManager (new Uri (ServerUtils.GetSocketIOUrl()), options);

		//socketManager.GetSocket("/").On ("connect", OnConnect);
		socketManager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));
		socketManager.Socket.On (SocketIOEventTypes.Disconnect, (socket, packet, eventArgs) => {
			Debug.Log("lose connection");
		});
		socketManager.GetSocket("/").On(SocketIOEventTypes.Connect, (socket, packet, arg) => {
			
			Debug.Log ("Connected");  
			var joinRoomReq =  new {
				roomNo = "123456",
				userId = "1313123"
			};

			//BestHTTP.SocketIO.Events.SocketIOAckCallback a = null;
			string json = JsonConvert.SerializeObject(joinRoomReq);
			Debug.Log ("jsons = " + json);
			socket.Emit ("JoinRoom", JoinRoomCallback,  json);
		});
		socketManager.Open();

	}

	private void OnConnect (Socket socket, Packet packet, params object[] args) {
		
	}

	private void JoinRoomCallback(Socket socket, Packet packet, params object[] args) {
		string msg = packet.ToString ();
		JoinRoomResponse resp = JsonConvert.DeserializeObject<JoinRoomResponse[]> (msg) [0];
		Debug.Log("msg: " + resp.status );

		DateTime end = DateTime.Now;

		double millisecs = (end - startTime).TotalMilliseconds;
		Debug.Log ("TotalMilliseconds = " + millisecs);


	}

	public void TurnCards() {
		for (int i = 0; i < turnCards.Length; i++) {
			StartCoroutine(TurnCard(turnCards[i]));
		}
	}

	public IEnumerator TurnCard(Image card) {
		
		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("Turn90_2");
		yield return new WaitForSeconds (0.15f);
		card.gameObject.SetActive (false);
		card.sprite = cardSprites[UnityEngine.Random.Range(1, 40)];
		card.gameObject.SetActive (true);
		anim.Play ("Turn85_3");
		yield return new WaitForSeconds (0.1f);

	}

	/*
	public IEnumerator TurnCard2(Image card) {

		Animator anim = card.GetComponent<Animator> ();
		anim.Play ("Turn90_2");
		yield return new WaitForSeconds (0.5f);
		card.gameObject.SetActive (false);
		card.sprite = cardSprites[UnityEngine.Random.Range(1, 40)];
		card.gameObject.SetActive (true);
		anim.Play ("Turn85_3");
		yield return new WaitForSeconds (0.2f);

	}
*/


	public void MoveCard() {

		TurnCards ();
		
		//ShowChips ();

		/*
		text.DOText ("9000", 1, true, ScrambleMode.Numerals);  
		Sequence s = DOTween.Sequence ();
		s.SetDelay (0.1f);
		s.Append(readyImage.transform.DOScale (3f, 0.2f));
		s.SetDelay (0.1f);
		s.Append(readyImage.transform.DOScale (1f, 0.2f)); */


		/*
		for(int i = 0; i < cards.Length / 4; i++) {
			for (int j = 0; j < 4; j++) {
				int index = i * 4 + j;
		
			    cards [index].transform.DOLocalMove (positions [index], 13000f, false)
					.SetSpeedBased ()
					.SetDelay (index * 0.07f);
				if (i == 0)
					cards [index].transform
						.DOScale (1.7f, 0.04f)
						.SetDelay (index * 0.07f + 0.02f)
						.OnComplete (() => {


					});
			}
		} */

	}

	public void Reset() {
		text.text = "1000";
		readyImage.transform.localScale = new Vector3 (1f, 1f);
		for(int i = 0; i < cards.Length; i++) {
			cards [i].transform.position = card.transform.position;
			cards [i].transform.localScale = new Vector3 (1.1f, 1.1f, 0);
		}

		for (int i = 0; i < turnCards.Length; i++) {
			turnCards [i].sprite = cards [i].sprite;
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


		turnCards = new Image[4];
		for (int i = 0; i < 4; i++) {
			turnCards[i] = Instantiate (card2);
			turnCards [i].transform.position = card2.transform.position;
			turnCards [i].transform.position = new Vector3(turnCards [i].transform.position.x + i * 1.5f, turnCards [i].transform.position.y);
			turnCards [i].transform.SetParent (canvas.transform);
			turnCards [i].transform.localScale = new Vector3 (1.8f, 1.8f, 0);
			turnCards [i].gameObject.SetActive (true);
		}

	}

}
