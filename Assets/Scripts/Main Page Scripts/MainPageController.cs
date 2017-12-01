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
	private GameObject confirmMessagePanel;
	[SerializeField]
	private GameObject messagePanel;
	[SerializeField]
	private Text messageLabel;

	[SerializeField]
	private GameObject joinRoomPanel;
	[SerializeField]
	private Text[] numberLabels;
	private int curNumberIndex;


	// Use this for initialization
	void Start () {
		//Player.Me = LoginController.CreateMockPlayer ();
		if (!LoginController.isFromLogin) {
			Player.Me = LoginController.CreateMockPlayer ();
		}
		nickNameLabel.text = Player.Me.nickname;
		idLabel.text = "ID: " + Player.Me.userId;
		coinLabel.text = "1000";

		ImageLoader.Instance.Load (Player.Me.headimgurl, (Sprite sprite) => {
			userImage.sprite = sprite;
			Player.Me.userHeadImage = userImage.sprite; 
		});

		ResetNumberLabels ();

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
			ShowConfirmMessagePanel (message);
		}
	}

	private void ShowConfirmMessagePanel(string msg) {
		messageLabel.text = msg;
		messageLabel.gameObject.SetActive (true);
		confirmMessagePanel.SetActive (true);
	}

	private void ShowMessagePanel(string msg) {
		messageLabel.text = msg;
		messageLabel.gameObject.SetActive (true);
		messagePanel.SetActive (true);
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
		Debug.Log ("createRoomUrl: " + ServerUtils.GetCreateRoomUrl());
		StartCoroutine (PostRequest(ServerUtils.GetCreateRoomUrl(), JsonConvert.SerializeObject(new {userId = Player.Me.userId}), createRoom));
	}

	void OnDestory() {
		Debug.Log ("OnDestory called");
	}

	public void JoinRoomClick() {
		joinRoomPanel.SetActive (true);
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
		StartCoroutine (ServerUtils.PostRequest(ServerUtils.GetCheckUserInGameUrl() + "?req=" 
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
			ShowConfirmMessagePanel ("服务器连接失败");
		}
		else
		{
			Debug.Log("Received: " + req.downloadHandler.text);
			handle (req.downloadHandler.text);
		}
	}
		
	public void MessagePanelSureButtonClick() {
		confirmMessagePanel.gameObject.SetActive (false);
	}

	public void NumberClick() {
		if (curNumberIndex == 6)
			return;
		string buttonName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
		int number = int.Parse (buttonName [buttonName.Length - 1] + "");
		numberLabels [curNumberIndex++].text = number + "";
		if (curNumberIndex == 6) {
			string roomNo = "";
			foreach (Text text in numberLabels) {
				roomNo += text.text;
			}
			JoinRoom (roomNo);
		}
	}

	public void ReinputClick() {
		ResetNumberLabels ();
	}

	public void DeleteClick() {
		if (curNumberIndex == 0)
			return;
		numberLabels [--curNumberIndex].text = "";
	}

	public void CloseJoinRoomPanel() {
		joinRoomPanel.SetActive (false);
		ResetNumberLabels ();
	}

	private void ResetNumberLabels() {
		foreach (Text text in numberLabels) {
			text.text = "";
		}
		curNumberIndex = 0;
	}

	private void JoinRoom(string roomNo) {
		Debug.Log ("JoinRoom roomNo = " + roomNo);
		ShowMessagePanel ("查找房间中...");
		//检查房间是否存在， 如果存在就跳转到游戏的界面
		ResponseHandle handler = delegate(string jsonString){
			Debug.Log("GetRoomResponse: " + jsonString);
			//加入玩家已经游戏了，那么跳转到Gameplay Scene。否则什么都不需要坐。
			GetRoomResponse resp = JsonConvert.DeserializeObject<GetRoomResponse>(jsonString);
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			if (resp.isExist) {
				parameters["roomNo"] = resp.roomNo;
				parameters["serverUrl"] = resp.serverUrl;
				Scenes.Load("Gameplay", parameters); 
			} else {
				//房间不存在
				ShowConfirmMessagePanel("该房间不存在");
			}
		};
		StartCoroutine( ServerUtils.PostRequest(ServerUtils.GetRoomUrl(), JsonConvert.SerializeObject(new {roomNo = roomNo}), handler));
	}

	public void JoinRoomFromUrl( string url )  
	{  
		Debug.Log( "openUrl： " + url );  
		string roomNo = url.Replace ("wx73653b5260b24787://?room=", ""); 
		Debug.Log ("roomNo = " + roomNo);
		JoinRoom (roomNo);
	}  


	private void LoadActivityParamsForAndroid() {
		if (Application.platform != RuntimePlatform.Android)
			return;

		string arguments = "";
		AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		if (currentActivity == null) {
			Debug.Log ("currentActivity is null");
			return;
		}

		AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");

		string data = intent.Call<string> ("getDataString");
		Debug.Log ("data = " + data);

		if (!string.IsNullOrEmpty (data)) {
			
			string roomNo = data.Replace ("wx73653b5260b24787://?room=", "");
			Debug.Log ("roomNo = " + roomNo);
			JoinRoom (roomNo);
		}
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus) {
		} else {  //回到主界面
			Debug.Log("");
			LoadActivityParamsForAndroid();
		}
	}

}
	