using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Round
{
	public string[] myCards;
	public int[] myBets;
	public int myBet;

	public string banker;
	public int[] playerBets;
	public Dictionary<string, string[]> playerCardsDict;

	public int[] niuArray;
	public int[][] cardSequenceArray;
	public int[] multipleArray;

	public Dictionary<string, int> resultDict; //输赢关系

	public Round ()
	{
		myCards = new string[5];
		playerBets = new int[Game.SeatCount];
		playerCardsDict = new Dictionary<string, string[]>();
		niuArray = new int[Game.SeatCount];
		cardSequenceArray = new int[Game.SeatCount] [];
		multipleArray = new int[Game.SeatCount];
		resultDict = new Dictionary<string, int> ();
	}

	public bool HasNiu(int seatIndex) {
		return niuArray [seatIndex] > 0;
	}
}


