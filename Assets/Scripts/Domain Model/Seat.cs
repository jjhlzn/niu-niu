using System;


public class Seat
{
	public Seat ()
	{
	}

	public string seatNo;
	public Player player;

	public bool hasPlayer() {
		return player != null;
	}
}


