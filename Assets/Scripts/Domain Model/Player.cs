using System;
using UnityEngine;

[System.Serializable]
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
	public int seat;  //座位的索引号，
	public int score;
	public bool hasSeat() {
		return seat >= 0 && seat < 6;
	}

	public Player ()
	{
		seat = -1;
		score = 0;
	}

	public Player(string userId) {
		seat = -1;
		score = 0;
		this.userId = userId;
	}

	public static Player Me = new Player("jinjunhang");
}


