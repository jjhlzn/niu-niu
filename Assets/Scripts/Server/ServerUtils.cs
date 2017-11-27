using System;

public class ServerUtils
{
	public ServerUtils ()
	{
	}

	public static string mainServer =   "192.168.1.114"; // "192.168.31.175"; //"localhost" ;  //"192.168.1.114";
	public static string protocol = "http";
	public static int port = 3000;

	public static string GetCheckUserInGameUrl() {
		return protocol + "://" + mainServer + ":" + port + "/checkuseringame";
	}

	public static string GetCreateRoomUrl() {
		return protocol + "://" + mainServer + ":" + port + "/createroom";
	}

	public static string GetRoomUrl() {
		return protocol + "://" + mainServer + ":" + port + "/getroom";
	}
}