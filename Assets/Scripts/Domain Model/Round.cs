using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Round
{
	//public string[] myCards;
	public int[] myBets;
	public int myBet;

	public Dictionary<string, bool> robBankerDict = new Dictionary<string, bool> ();
	public string[] robBankerPlayers; //这个数组表示抢庄的用户


	public string banker;
	public int[] playerBets;
	public Dictionary<string, string[]> playerCardsDict;

	public int[] niuArray;
	public int[][] cardSequenceArray;
	public int[] multipleArray;

	public Dictionary<string, int> resultDict; //输赢关系

	public Round ()
	{
		//myCards = new string[5];
		playerBets = new int[Game.SeatCount];
		for (int i = 0; i < playerBets.Length; i++) {
			playerBets [i] = -1;
		}
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


