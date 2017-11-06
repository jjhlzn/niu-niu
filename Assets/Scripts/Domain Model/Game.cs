using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Game
{
	public static int SeatCount = 6;
	
	public Game ()
	{
		state = GameState.BeforeStart;
		mySeat = -1;
		rounds = new List<Round> ();
		players = new Player[6];
		currentRound = new Round();
		rounds.Add(currentRound);
	}  

	public bool isMeSeated {   //我是否已经坐下
		get {
			foreach (Seat seat in seats) {
				if (seat.hasPlayer () && seat.player.userId == me.userId) {
					return true;
				}
			}
			return false;
		}
	}

	public GameState state;

	public string roomNo;

	public List<Round> rounds;

	public Player[] players;

	public Player me = Player.Me;

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


