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
		string seatNos = "ABCDEF";
		for(int i = 0 ; i < SeatCount; i++) {
			seats [i] = new Seat ();
			seats [i].seatNo = seatNos[i] + "";
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

	public int PlayerCount {
		get {
			int count = 0;
			foreach (Seat seat in seats) {
				if (seat.player != null) {
					count++;
				}
			}
			return count;
		}
	}

	public int GetSeatIndex(string userId) {
		for (int i = 0; i < seats.Length; i++) {
			if (seats [i].hasPlayer () && userId == seats [i].player.userId) {
				return i;
			}
		}
		return -1;
	}
}


