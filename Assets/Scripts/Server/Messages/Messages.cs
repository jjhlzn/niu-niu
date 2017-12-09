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
	public static string DismissRoom = "DismissRoom";
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
	public static string Delegate = "Delegate";   //委托给服务器
	public static string SomePlayerDelegate = "SomePlayerDelegate";  //手机切入后台 & 断线 
	public static string NotDelegate = "NotDelegate";   //委托给服务器
	public static string SomePlayerNotDelegate = "SomePlayerNotDelegate";  //手机切入后台 & 断线 
	public static string RoomHasDismissed = "RoomHasDismissed";
}

public class StartGameRequest {
	public string type = "startGame";
}

public class BaseGameResponse {
	public int status;
	public string errorMessage;
	public string roomNo;

	public bool IsSuccess() {
		return status == 0;
	}

	public override string ToString() {
		return "status = " + status + ", erorrMessage = " + errorMessage;
	}
}

public class FirstDealResponse : BaseGameResponse {
	public int roundNo;
	public Dictionary<string, string[]> cardsDict = new Dictionary<string, string[]>();
	public Dictionary<string, int[]> betsDict = new Dictionary<string, int[]>();
}

public class GoToChooseBankerNotity : BaseGameResponse {
	public string banker;
	public string[] robBankerPlayers;
}

public class GoToSecondDealNotify : BaseGameResponse {
	public Dictionary<string, string> cardsDict;
}

public class GoToCheckCardNotify : BaseGameResponse {
	public string card;
}

public class SomePlayerSitDownNotify : BaseGameResponse {
	public string seat = "";
	public string userId = "";
	public string headimgurl = "";
	public string nickname = "";
	public int sex = 1;
	public string ip = "";
}

public class SomePlayerStandUpNotify : BaseGameResponse {
	public string userId;
}

public class StartGameNotify : BaseGameResponse {
	public Dictionary<string, string[]> cardsDict = new Dictionary<string, string[]>();
	public Dictionary<string, int[]> betsDict = new Dictionary<string, int[]>();
}

public class SomePlayerReadyNotify : BaseGameResponse {
	public string userId;
}

public class SomePlayerRobBankerNotify : BaseGameResponse {
	public string userId;
	public bool isRob;
}

public class SomePlayerBetNotify : BaseGameResponse {
	public string userId;
	public int bet;
}

public class BetAck : BaseGameResponse {
}

public class SomePlayerShowCardNotify : BaseGameResponse {
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
	public Dictionary<string, int> resultDict;   //输赢关系
	public Dictionary<string, int> scoreDict;
}

public class ShowCardResult {
	public int niu;
	public string[] cards;
	public int[] cardSequences;
	public int multiple;
}

public class JoinRoomResponsePlayerInfo {
	public string seat = "";
	public string userId = "";
	public string headimgurl = "";
	public string nickname = "";
	public int sex = 1;
	public string ip = "";
}

public class JoinRoomResponse : BaseGameResponse {
	public string creater;
	public string state;
	public string banker;
	public int totalRoundCount;
	public int currentRoundNo;

	public Dictionary<string, string[]> playerCards = new Dictionary<string, string[]>();
	public Dictionary<string, int[]> playerBets = new Dictionary<string, int[]>();
	public Dictionary<string, int> scores = new Dictionary<string, int>();

	public Dictionary<string, JoinRoomResponsePlayerInfo> sitdownPlayers = new Dictionary<string, JoinRoomResponsePlayerInfo>();
	public Dictionary<string, bool> robBankerPlayers = new Dictionary<string, bool>();
	public Dictionary<string, int> betPlayers = new Dictionary<string, int>();
	public Dictionary<string, ShowCardResult> showcardPlayers = new Dictionary<string, ShowCardResult> ();
	public Dictionary<string, bool> readyPlayers = new Dictionary<string,bool> ();
	public string[] delegatePlayers = new string[0];
}

public class CheckUserInGameResponse : BaseGameResponse {
	public bool isInGame;
	public string serverUrl;
}

public class CreateRoomResponse : BaseGameResponse {
	public string serverUrl;
}

public class DismissRoomResponse : BaseGameResponse {
}

public class LeaveRoomResponse : BaseGameResponse {
}
	
public class GameOverResponse : BaseGameResponse {
	public bool isPlayed;
	public Dictionary<string, int> scores = new Dictionary<string, int>(); 
	public string gameOverTime;
	public List<string> bigWinners = new List<string>();
	public List<string> bigLosers = new List<string>();

	public bool gameOverAfterRound;
	public Dictionary<string, int> resultDict;   //本局的输赢关系
}

public class GetRoomResponse : BaseGameResponse {
	public string serverUrl;
	public bool isExist;
}

public class LoginResponse : BaseGameResponse {
	public string userId;
}
	

public class SomePlayerDeleteNotify : BaseGameResponse {
	public string userId;
}

public class SomePlayerNotDeleteNotify : BaseGameResponse {
	public string userId;
}

public class RoomHasDismissedNotify : BaseGameResponse {
}

public class CheckUpdateResponse : BaseGameResponse {
	public bool isNeedUpdate;
	public string newVersion = "";
	public string updateUrl = "";
}