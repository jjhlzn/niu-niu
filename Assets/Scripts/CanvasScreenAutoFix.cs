
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScreenAutoFix : CanvasScaler
{
	public static CanvasScreenAutoFix instance;

	private static float DEPEND_W = 1080;
	private static float DEPEND_H = 1920;

	public float scaleRatio = 1.0f;


	private void ResizeCanvas()
	{
		if (is1920x1080 ()) {
			Debug.Log ("is 1920 * 1080");
			matchWidthOrHeight = 0.5f;

		} else if (isIphonex ()) {
			Debug.Log ("is Height First");
			matchWidthOrHeight = 1f;

		} else if (isWidthFirst ()) {
			Debug.Log ("is Width First");
			matchWidthOrHeight = 0f;

		}
			
	}

     void Awake()
	{
		
		ResizeCanvas();
		if(instance == null)
		{
			instance = this;
		}
	}

	private bool is1920x1080() {
		int screenW = Screen.width;
		int screenH = Screen.height;
		return Mathf.Abs ((float)screenW / screenH - (float)1920 / 1080) < 0.00001;
	}

	private bool isIphonex() {
		int screenW = Screen.width;
		int screenH = Screen.height;
		return (float)screenW / screenH >=  (float)1920 / 1080;
	}

	private bool isWidthFirst() {
		int screenW = Screen.width;
		int screenH = Screen.height;
		return (float)screenW / (float)screenH <  (float)1920 / 1080;
	}

}