using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;


public class ServerUtils
{
	public ServerUtils ()
	{
	}

	public static string mainServer = "niu.yhkamani.com";  //  "niu.yhkamani.com" ; //"192.168.1.114" ; //"localhost" ;  //"192.168.31.175";
	public static string protocol = "http";
	public static int port = 80;
	public static int socketIOPort = 3001;

	public static string GetCheckUserInGameUrl() {
		return protocol + "://" + mainServer + ":" + port + "/checkuseringame";
	}

	public static string GetCreateRoomUrl() {
		return protocol + "://" + mainServer + ":" + port + "/createroom";
	}

	public static string GetRoomUrl() {
		return protocol + "://" + mainServer + ":" + port + "/getroom";
	}

	public static string GetLoginUrl() {
		return protocol + "://" + mainServer + ":" + port + "/login";
	}


	public static IEnumerator PostRequest(string url, string json, ResponseHandle handle, ResponseHandle errorHandle = null)
	{
		Debug.Log ("Post Request: url = " + url);
		var req = new UnityWebRequest(url, "POST");
		byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
		req.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
		req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

		req.SetRequestHeader("Content-Type", "application/json");

		//Send the request then wait here until it returns
		yield return req.SendWebRequest();

		if (req.isNetworkError)
		{
			Debug.Log("Error While Sending: " + req.error);
			if (errorHandle != null)
				errorHandle (req.error);
			else {
				Debug.Log ("errorHandler is null");
			}
		}
		else
		{
			Debug.Log("Received: " + req.downloadHandler.text);
			handle (req.downloadHandler.text);
		}
	}
}


public delegate void ResponseHandle(string jsonString);