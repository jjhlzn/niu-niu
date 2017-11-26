using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class Player
{
	public static Player Me = new Player("7654321", "金军航", "http://wx.qlogo.cn/mmopen/vi_32/DYAIOgq83ercUcNbFyyGQUZEwiaSM5X1mylHCibYpfIiaYbysg2FA0ibtwVPBaxSRktg3h2UHTJTAaTwIJjsfrwlmg/0");

	public string headimgurl = "";
	public string province = "";
	public string city = "";
	public string unionid = "";
	public string openid = "";
	public string nickname = "";
	public int sex;
	public string ip = "";
	public string geoPosition = "";

	public Sprite userHeadImage;

	private string _userId;
	public string userId {
		get {
			return _userId;
		}
		set {
			//Debug.Log ("Player userid change to [" + value + "]");
			_userId = value;
		}
	}
	public bool isPlaying;  //用于区分仅仅坐下，但是没有玩的玩家，对于BeforeStart和WaitForNext的时候，坐下的玩家就表示Playing，其他的状态则不是
	public bool isReady;    //这个状态则只对于WaitForNext的时候有用，其他游戏状态不会用到这个状态。

	public Seat seat;  //座位的索引号，
	public int score;
	public Image[] cards {
		get {
			return seat.cards;
		}
	}

	public Player ()
	{
		score = 0;
	}
		
	public Player(string userId) : this(){
		this.userId = userId;
	}

	public Player(string userId, String nickName, string imageUrl) : this(userId) {
		this.nickname = nickName;
		this.headimgurl = imageUrl;
	}

	public void Sit(Seat seat) {
		this.seat = seat;
		this.seat.player = this;
	}

	public void Standup() {
		if (seat != null) {
			seat.player = null;
			this.seat = null;
		}
	}

	public override string ToString ()
	{
		return string.Format ("[Player: userId={0", userId);
	}
}


