using System;
using UnityEngine;

[System.Serializable]
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


