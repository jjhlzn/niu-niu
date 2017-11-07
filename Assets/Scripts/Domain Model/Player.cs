using System;
using UnityEngine;
using UnityEngine.UI;


public class Player
{
	private string _userId;
	public string userId {
		get {
			return _userId;
		}
		set {
			Debug.Log ("Player userid change to [" + value + "]");
			_userId = value;
		}
	}
	public bool isReady;
	public Seat seat;  //座位的索引号，
	public int score;
	public bool isPlaying;  //是否已经在完，用于区分仅仅坐下，但是没有玩的玩家
	public Image[] cards {
		get {
			return seat.cards;
		}
	}

	public Player ()
	{
		score = 0;
	}

	public Player(string userId) {
		score = 0;
		this.userId = userId;
	}

	public static Player Me = new Player("jinjunhang");

	public void Sit(Seat seat) {
		this.seat = seat;
		this.seat.player = this;
	}

	public void Standup() {
		if (seat != null) {
			seat.player = null;
			this.seat = null;
		}
	}
}


