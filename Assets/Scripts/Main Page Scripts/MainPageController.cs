using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class MainPageController : MonoBehaviour {

	[SerializeField]
	private Text nickNameLabel;
	[SerializeField]
	private Text idLabel;
	[SerializeField]
	private Text coinLabel;
	[SerializeField]
	private Image userImage;

	[SerializeField]
	private GameObject messagePanel;
	[SerializeField]
	private Text messageLabel;

	// Use this for initialization
	void Start () {
		Player.Me = LoginController.CreateMockPlayer ();

		nickNameLabel.text = Player.Me.nickname;
		idLabel.text = "ID: " + Player.Me.userId;
		coinLabel.text = "1000";
		StartCoroutine (LoadImage (Player.Me.headimgurl));

		ImageLoader.instance.Load (Player.Me.headimgurl, (Sprite sprite) => {
			userImage.sprite = sprite;
			Player.Me.userHeadImage = userImage.sprite; 
		});

		CheckPlayerInGame ();
		ShowMessageIfNeed ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void ShowMessageIfNeed() {
		Dictionary<string, string> parameters = Scenes.getSceneParameters ();
		if (parameters != null && parameters.ContainsKey (Utils.Message_Key)) {
			string message = parameters [Utils.Message_Key];
			messageLabel.text = message;
			messageLabel.gameObject.SetActive (true);
		}
	}

	public void CreateRoomClick() {
		//创建房间
		Debug.Log("create room click");
		ResponseHandle createRoom = delegate(string jsonString){
			Debug.Log("CreateRoomResponse: " + jsonString);
			//加入玩家已经游戏了，那么跳转到Gameplay Scene。否则什么都不需要坐。
			CreateRoomResponse resp = JsonConvert.DeserializeObject<CreateRoomResponse>(jsonString);
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			if (resp.status == 0) {
				parameters["roomNo"] = resp.roomNo;
				parameters["serverUrl"] = resp.serverUrl;
				Scenes.Load("Gameplay", parameters); 
			}
		};
		StartCoroutine (PostRequest(ServerUtils.GetCreateRoomUrl(), JsonConvert.SerializeObject(new {userId = Player.Me.userId}), createRoom));

	
	}

	void OnDestory() {
		Debug.Log ("OnDestory called");
	}

	public void JoinRoomClick() {
		
	}

	IEnumerator LoadImage(string url) {
		WWW www = new WWW(url);
		yield return www;
		userImage.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
		Player.Me.userHeadImage = userImage.sprite; 
	}

	private void CheckPlayerInGame() {
		ResponseHandle checkUserInGame = delegate(string jsonString){
			Debug.Log("CheckPlayerInGameResponse: " + jsonString);
			//加入玩家已经游戏了，那么跳转到Gameplay Scene。否则什么都不需要坐。
			CheckUserInGameResponse resp = JsonConvert.DeserializeObject<CheckUserInGameResponse>(jsonString);
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			if (resp.isInGame) {
				parameters["roomNo"] = resp.roomNo;
				parameters["serverUrl"] = resp.serverUrl;
				Scenes.Load("Gameplay", parameters); 
			}
		};
		StartCoroutine (PostRequest(ServerUtils.GetCheckUserInGameUrl() + "?req=" 
			+ JsonConvert.SerializeObject(new {userId = Player.Me.userId}), "{}", checkUserInGame));
	}

	IEnumerator PostRequest(string url, string json, ResponseHandle handle)
	{
		var req = new UnityWebRequest(url, "POST");
		byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
		req.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
		req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

		req.SetRequestHeader("Content-Type", "application/json");

		//Send the request then wait here until it returns
		yield return req.SendWebRequest();

		if (req.isNetworkError)
		{
			Debug.Log("Error While Sending: " + req.error);
		}
		else
		{
			Debug.Log("Received: " + req.downloadHandler.text);
			handle (req.downloadHandler.text);
		}
	}


	public void MessagePanelSureButtonClick() {
		messagePanel.gameObject.SetActive (false);
	}
}
public delegate void ResponseHandle(string jsonString);
/*
public interface ResponseHandler {
    void Handle(string jsonString);
} */
	
