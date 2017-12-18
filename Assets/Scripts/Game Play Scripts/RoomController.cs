using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;


public class RoomController : MonoBehaviour
{
	[SerializeField]
	private GamePlayController gamePlayController; 

	[SerializeField]
	private SetupCardGame setupGame;

	[SerializeField]
	private GameObject dismissRoomPanel;



	void Start() {
		
	}

	//解散房间，目前只允许游戏未开始的时候解散房间
	public void DismissRoomClick() {
		setupGame.CloseMenuClick ();
		dismissRoomPanel.gameObject.SetActive (true);
	}


	public void DismissRoomSureClick() {
		Debug.Log ("DismissRoomSureClick called");
		var game = gamePlayController.game;
		var socket = gamePlayController.gameSocket;

		if (game.state != GameState.BeforeStart) {
			Debug.Log ("state is not BeforeStart, state = " + game.state.value);
			return;
		}

		var req = new {
			userId = Player.Me.userId,
			roomNo = game.roomNo,
			clientInfo = Utils.GetClientInfo(),
			userInfo = Utils.GetUserInfo()
		};
		socket.Emit(Messages.DismissRoom, (s, packet, args) => {
			string msg = packet.ToString();
			Debug.Log("DismissRoomAck: " + msg);
			DismissRoomResponse resp = JsonConvert.DeserializeObject<DismissRoomResponse[]>(msg)[0];
			if (resp.status != 0)
				return;

			//处理
			Dictionary<string, string> parameters = new Dictionary<string, string> ();
			//parameters[Utils.Message_Key] = "该房间已经被解散了";
			Scenes.Load ("MainPage", parameters);
			}, JsonConvert.SerializeObject (req));
	}

	public void DismissRoomCancelClick() {
		dismissRoomPanel.gameObject.SetActive (false);
	}

	//目前房主不能离开房间，房主只能解散房间，游戏开始之后就不能离开房间
	public void LeaveRoomClick() {
		Debug.Log ("Leave Room Click");

		var game = gamePlayController.game;
		var socket = gamePlayController.gameSocket;

		if (game.state != GameState.BeforeStart || Player.Me.userId == game.creater) {
			Debug.Log("game.state is not BeforeStart or I am room ceater, can't leave room");
			return;
		}

		//检查用户是否坐下座位上
		if (Player.Me.seat != null) {
			Debug.Log("Player.Me.seat is not null, can't leave room");
			return;
		}

		var req = new {
			userId = Player.Me.userId,
			roomNo = game.roomNo,
			clientInfo = Utils.GetClientInfo(),
			userInfo = Utils.GetUserInfo()
		};

		socket.Emit (Messages.LeaveRoom, (s, packet, args) => {
			string msg = packet.ToString();
			Debug.Log("LeaveRoomResponse: " + msg);
			LeaveRoomResponse resp = JsonConvert.DeserializeObject<LeaveRoomResponse[]>(msg)[0];
			if (resp.status != 0)
				return;

			Scenes.Load("MainPage");
		}, JsonConvert.SerializeObject (req) );

		setupGame.CloseMenuClick ();
	}





}


