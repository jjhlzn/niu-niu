using System;


public class Messages
{
	public static string GoToReady = "GoToReady";
	public static string GoToFirstDeal = "GoToFirstDeal";
	public static string GoToRobBanker = "GoToRobBanker"; 
	public static string GoToChooseBanker = "GoToChooseBanker";
	public static string GoToBet = "GoToBet";
	public static string GoToSecondDeal = "GoToSecondDeal";
	public static string GoToCheckCard = "GoToCheckCard";
	public static string GoToCompareCard = "GoToCompareCard";
	public static string GoToGameOver = "GoToGameOver";

	public static string Seat = "Seat";
	public static string RobBanker = "RobBanker";
	public static string Bet = "Bet";
	public static string ShowCard = "ShowCard";
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
	public int number;
	public string[] cards;
	public int[] bets;
}

public class GoToChooseBankerNotity : BaseGameResponse {
	public string banker;
}

public class GoToSecondDealNotify : BaseGameResponse {
	public string card;
}

public class GoToCheckCardNotify : BaseGameResponse {
	public string card;
}

