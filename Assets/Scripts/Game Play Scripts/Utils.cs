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
}


