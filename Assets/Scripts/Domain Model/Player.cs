using System;


public class Player
{
	public string userId;
	public int seat;  //座位编号
	public bool hasSeat() {
		return seat >= 0 && seat < 6;
	}

	public Player ()
	{
		seat = -1;
	}

	public Player(string userId) {
		seat = -1;
		this.userId = userId;
	}

	public static Player Me = new Player("jinjunhang");
}


