using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Utils
{
	public static string Message_Key = "message";

	public static string Music_Key = "Music";
	public static string Audio_Key = "Audio";

	public Utils ()
	{
	}

	public static void ShowConfirmMessagePanel(string msg, GameObject panel) {
		
		Text label = panel.GetComponentInChildren<Text> ();
		if (label != null)
			label.text = msg;
		panel.SetActive (true);
		
	}

	public static void ShowMessagePanel(string msg, GameObject panel) {
		
		Text label = panel.GetComponentInChildren<Text> ();
		if (label != null)
			label.text = msg;
		panel.SetActive (true);
		
	}

	public static void HideMessagePanel(GameObject panel) {
		panel.SetActive (false);
	}
		
	public static bool isTwoPositionIsEqual(Vector3 v1, Vector3 v2) {
		
		float deltaX = Mathf.Abs(v1.x - v2.x);
		float deltaY = Mathf.Abs(v1.y - v2.y);
		//Debug.Log ("deltaX = " + deltaX + ", deltaY = " + deltaY);
		return deltaX < 0.000001f && deltaY  < 0.000001f; 
	}

	public static string GetNumberSring(int number) {
		if (number > 0)
			return "+" + number;
		return "" + number;
	}

	public static string GetPlatform() {
		if (Application.platform == RuntimePlatform.Android)
			return "Android";
		else if (Application.platform == RuntimePlatform.IPhonePlayer)
			return "iOS";
		else
			return "PC_Or_MAC";
	}

	public static string GetVersion() {
		return Application.version;
	}

	public static string GetShareGameResultUrl() {
		return System.IO.Path.Combine (Application.persistentDataPath, GetShareGameResultFileName());
	}

	public static string GetShareGameResultFileName() {
		return  "zhanji.png";;
	}
}


