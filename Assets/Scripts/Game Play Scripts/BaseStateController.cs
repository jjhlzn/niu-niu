using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateController : MonoBehaviour {

	public abstract void Reset ();

	public abstract GamePlayController GetGamePlayController ();

	public void Update() {
		if (GetGamePlayController () != null && !GetGamePlayController ().isInited) {
			return;
		}
	}

	public void SetUI() {
	}


}
