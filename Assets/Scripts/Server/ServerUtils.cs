﻿using System.Collections;
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

	public static string mainServer = "192.168.31.175"; //  "niu.yhkamani.com" ; //"192.168.1.117" ; //"localhost" ;  //"192.168.31.175";
	public static string protocol = "http";
	public static int port = 3001;
	public static int socketIOPort = 3001;

	public static string GetSocketIOUrl() {
		return protocol + "://" + mainServer + ":" + socketIOPort + "/socket.io/";
	}

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

	public static string CheckUpdateUrl() {
		return protocol + "://" + mainServer + ":" + port + "/checkupdate";
	}

	public static string ReportErrorUrl() {
		return protocol + "://" + mainServer + ":" + port + "/reporterror";
	}

	public static string CheckIOSAuditVersionUrl() {
		return protocol + "://" + mainServer + ":" + port + "/checkiosauditversion";
	}

	public static string AuditLoginUrl() {
		return protocol + "://" + mainServer + ":" + port + "/auditlogin";
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
			if (handle != null)
				handle (req.downloadHandler.text);
		}
	}


}


public delegate void ResponseHandle(string jsonString);