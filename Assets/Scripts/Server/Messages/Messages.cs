using System;
using System.Collections;
using System.Collections.Generic;


public class Messages
{
	public static string GoToFirstDeal = "GoToFirstDeal";
	public static string GoToRobBanker = "GoToRobBanker"; 
	public static string GoToChooseBanker = "GoToChooseBanker";
	public static string GoToBet = "GoToBet";
	public static string GoToSecondDeal = "GoToSecondDeal";
	public static string GoToCheckCard = "GoToCheckCard";
	public static string GoToCompareCard = "GoToCompareCard";
	public static string GoToGameOver = "GoToGameOver";

	public static string JoinRoom = "JoinRoom";
	public static string LeaveRoom = "LeaveRoom";
	public static string SitDown = "SitDown";
	public static string SomePlayerSitDown = "SomePlayerSitDown";
	public static string SomePlayerStandUp = "SomePlayerStandUp"; 
	public static string StandUp = "StandUp";
	public static string StartGame = "StartGame";
	public static string RobBanker = "RobBanker";
	public static string SomePlayerRobBanker = "SomePlayerRobBanker";
	public static string Bet = "Bet";
	public static string SomePlayerBet = "SomePlayerBet";
	public static string ShowCard = "ShowCard";
	public static string SomePlayerShowCard = "SomePlayerShowCard";
	public static string Ready = "Ready";
	public static string SomePlayerReady = "SomePlayerReady";
}

public class StartGameRequest {
	public string type = "startGame";

}

public class BaseGameResponse {
	public int status;
	public string errorMessage;

	public bool IsSuccess() {
		return status == 0;
	}

	public override string ToString() {
		return "status = " + status + ", erorrMessage = " + errorMessage;
	}
}

public class FirstDealResponse : BaseGameResponse {
	public string roomNo;
	public int roundNo;
	public Dictionary<string, string[]> cardsDict = new Dictionary<string, string[]>();
	public Dictionary<string, int[]> betsDict = new Dictionary<string, int[]>();
}

public class GoToChooseBankerNotity : BaseGameResponse {
	public string banker;
	public string[] robBankerPlayers;
}

public class GoToSecondDealNotify : BaseGameResponse {
	public string roomNo;
	public Dictionary<string, string> cardsDict;
}

public class GoToCheckCardNotify : BaseGameResponse {
	public string card;
}

public class SomePlayerSitDownNotify : BaseGameResponse {
	public string roomNo;
	public string seat;
	public string userId;
}

public class SomePlayerStandUpNotify : BaseGameResponse {
	public string roomNo;
	public string userId;
}

public class StartGameNotify : BaseGameResponse {
	public string roomNo;
	public Dictionary<string, string[]> cardsDict = new Dictionary<string, string[]>();
	public Dictionary<string, int[]> betsDict = new Dictionary<string, int[]>();
}

public class SomePlayerReadyNotify : BaseGameResponse {
	public string roomNo;
	public string userId;
}

public class SomePlayerRobBankerNotify : BaseGameResponse {
	public string roomNo;
	public string userId;
	public bool isRob;
}

public class SomePlayerBetNotify : BaseGameResponse {
	public string roomNo;
	public string userId;
	public int bet;
}

public class SomePlayerShowCardNotify : BaseGameResponse {
	public string roomNo;
	public string userId;
	public string[] cards;
	public int[] cardSequences;
	public int niu;   //0 表示没有牛，表示牛牛， 1 - 10 表示牛1 - 牛牛， 11表示炸弹，以此类推。
	public int multiple; //倍数
}

public class ShowCardAck : BaseGameResponse {
	public int[] cardSequences;
	public int niu;   //0 表示没有牛，表示牛牛， 1 - 10 表示牛1 - 牛牛， 11表示炸弹，以此类推。
	public int multiple; //倍数

}

public class GoToCompareCardNotify : BaseGameResponse {
	public string roomNo;
	public Dictionary<string, int> resultDict;   //输赢关系
	public Dictionary<string, int> scoreDict;
}

