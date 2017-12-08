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

	public Game game {
		get {
			return GetGamePlayController().game;
		}
	}

	public List<Player> playingPlayers {
		get {
			return GetGamePlayController().game.PlayingPlayers;
		}
	}

	public Deck deck {
		get {
			return GetGamePlayController().game.deck;
		}
	}
	public Seat[] seats {
		get {
			return GetGamePlayController().game.seats;
		}
	}
}
