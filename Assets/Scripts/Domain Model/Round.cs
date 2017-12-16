using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Round
{
	public List<Player> players = new List<Player>(); 
	public int[] myBets;

	public Dictionary<string, bool> robBankerDict = new Dictionary<string, bool> ();
	public Dictionary<string, int> resultDict; //输赢关系
	public string[] robBankerPlayers; //这个数组表示抢庄的用户
	public string banker;

	public Dictionary<string, string[]> playerCardsDict;
	public Dictionary<string, int> playerBets;
	public Dictionary<string, int> niuArray;
	public Dictionary<string, int> multipleArray;
	public Dictionary<string, int[]> cardSequenceArray;

	public Round ()
	{
		playerBets = new Dictionary<string ,int> ();
		playerCardsDict = new Dictionary<string, string[]>();
		niuArray = new Dictionary<string, int>();
		cardSequenceArray = new Dictionary<string, int[]>();
		multipleArray = new Dictionary<string, int>();
		resultDict = new Dictionary<string, int> ();
	}

	public bool HasNiu(string userId) {
		return niuArray.ContainsKey (userId) && niuArray [userId] > 0;
	}
}


