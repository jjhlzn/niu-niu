using System;


public class Player
{
	public string userId;
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


