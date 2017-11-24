using System;
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


	//解散房间，目前只允许游戏未开始的时候解散房间
	public void DismissRoomClick() {
		setupGame.CloseMenuClick ();
		dismissRoomPanel.gameObject.SetActive (true);
	}


	public void DismissRoomSureClick() {
		var game = gamePlayController.game;
		var socket = gamePlayController.gameSocket;

		if (game.state != GameState.BeforeStart) {
			return;
		}

		var req = new {
			userId = Player.Me.userId,
			roomNo = game.roomNo
		};

		socket.Emit (Messages.DismissRoom, JsonConvert.SerializeObject (req), (string msg) => {

			Debug.Log("DismissRoomAck: " + msg);
			DismissRoomResponse resp = JsonConvert.DeserializeObject<DismissRoomResponse[]>(msg)[0];
			if (resp.status != 0)
				return;

			if (resp.roomNo != game.roomNo) {
				return;
			}
			//处理
			Scenes.Load ("MainPage");
		});
	}

	public void DismissRoomCancelClick() {
		dismissRoomPanel.gameObject.SetActive (false);
	}

	//目前房主不能离开房间，房主只能解散房间，游戏开始之后就不能离开房间
	public void LeaveRoomClick() {
		var game = gamePlayController.game;
		var socket = gamePlayController.gameSocket;

		if (game.state != GameState.BeforeStart || Player.Me.userId == game.creater) {
			return;
		}

		//检查用户是否坐下座位上
		if (Player.Me.seat == null)
			return;

		var req = new {
			userId = Player.Me.userId,
			roomNo = game.roomNo
		};

		socket.Emit (Messages.LeaveRoom, JsonConvert.SerializeObject (req), (string msg) => {
			Debug.Log("LeaveRoomResponse: " + msg);
			LeaveRoomResponse resp = JsonConvert.DeserializeObject<LeaveRoomResponse[]>(msg)[0];
			if (resp.status != 0)
				return;

			Scenes.Load("MainPage");
		});
	}
}


