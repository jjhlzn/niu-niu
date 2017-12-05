using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateController : BaseMonoBehaviour {

	public abstract void Reset ();

	public abstract GamePlayController GetGamePlayController ();

	public void Update() {
		if (GetGamePlayController () != null && !GetGamePlayController ().isInited) {
			return;
		}
	}
}
