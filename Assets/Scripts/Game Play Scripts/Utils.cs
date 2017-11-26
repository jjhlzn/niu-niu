using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Utils
{
	public static string Message_Key = "message";

	public Utils ()
	{
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
}


