using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Utils
{
	public Utils ()
	{
	}

	private static Dictionary<string, Sprite> cardDict = new Dictionary<string, Sprite>();

	public static Sprite findCardSprite(Sprite[] cardSprites, string cardValue) {
		return cardSprites [16];

		if (cardDict.ContainsKey (cardValue)) {
			return cardDict [cardValue];
		}

		foreach(Sprite sprite in cardSprites) {
			//Debug.Log (sprite.name);
			if (sprite.name.Contains (cardValue)) {
				cardDict [cardValue] = sprite;
				return sprite;
			}
		}

		Debug.LogError ("can't find sprite for cardValue: " + cardValue);
		return null;
	}

	public static bool isTwoPositionIsEqual(Vector3 v1, Vector3 v2) {
		float deltaX = Mathf.Abs(v1.x - v2.x);
		float deltaY = Mathf.Abs(v1.y - v2.y);
		//Debug.Log ("deltaX = " + deltaX + ", deltaY = " + deltaY);
		return deltaX < .1f && deltaY < .1f;
	}
}


