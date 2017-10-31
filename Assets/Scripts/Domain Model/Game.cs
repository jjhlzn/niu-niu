using System;
using System.Collections;
using System.Collections.Generic;

public class Game
{
	public static int SeatCount = 6;
	
	public Game ()
	{
		mySeat = -1;
		rounds = new List<Round> ();
		players = new Player[6];
		currentRound = new Round();
		rounds.Add(currentRound);

		seats = new Seat[SeatCount];
		for(int i = 0 ; i < SeatCount; i++) {
			seats [i] = new Seat ();
			seats [i].seatNo = i;
		}
	}

	public string roomNo;

	public List<Round> rounds;

	public Player[] players;

	public Player me;

	public Round currentRound;

	public int mySeat;

	public int totalRoundCount;
	public int currentRoundNo;

	public Seat[] seats;
}


