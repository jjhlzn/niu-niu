using System;


public class Seat
{
	public Seat ()
	{
	}

	public int seatNo;
	public Player player;

	public bool hasPlayer() {
		return player != null;
	}
}


