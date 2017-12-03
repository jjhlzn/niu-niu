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

	[SerializeField]
	private GameObject createRoomPanel;

	[SerializeField]
	private GameObject settingsPanel;
	[SerializeField]
	private Button musicButton;
	[SerializeField]
	private Button audioButton;

	public static string Music_On_Key = "Music_On";
	public static string Music_Off_Key = "Music_Off";
	public static string Audio_On_Key = "Audio_On";
	public static string Audio_Off_Key = "Audio_Off";
	private Dictionary<string, Sprite> audioSettingsImageDict = new Dictionary<string, Sprite> ();
	private Dictionary<string, Sprite> gamePropertiesImageDict = new Dictionary<string ,Sprite>();

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

		Sprite[] sprites = Resources.LoadAll<Sprite>("sprites/mainpage/createroom");
		foreach (Sprite sprite in sprites) {
			gamePropertiesImageDict [sprite.name] = sprite;
		}

		audioSettingsImageDict[Music_On_Key] = Resources.Load<Sprite> ("sprites/mainpage/settings/btn_open");
		audioSettingsImageDict[Audio_On_Key] = Resources.Load<Sprite> ("sprites/mainpage/settings/btn_open");
		audioSettingsImageDict[Music_Off_Key] = Resources.Load<Sprite> ("sprites/mainpage/settings/btn_close");
		audioSettingsImageDict[Audio_Off_Key] = Resources.Load<Sprite> ("sprites/mainpage/settings/btn_close");
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

	private void HideMessagePanel() {
		messagePanel.SetActive (false);
	}

	public void ShowCreateRoomClick() {
		createRoomPanel.SetActive (true);

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

		var req = new {
			userId = Player.Me.userId,
			jushu = int.Parse(GetGameProperty("gameproperty_jushu")),
			fangfei = GetGameProperty("gameproperty_fangfei"),
			fengshu = GetGameProperty("gameproperty_fengshu"),
			qz = GetGameProperty("gameproperty_qz"),
			wanfa = GetGameProperty("gameproperty_wanfa")
		};

		StartCoroutine (PostRequest(ServerUtils.GetCreateRoomUrl(), JsonConvert.SerializeObject(req), createRoom));
	}
		

	private string GetGameProperty(string tag) {
		GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject obj in objs) {
			Button btn = obj.GetComponent<Button> ();
			if (btn.image.sprite.name.EndsWith ("_y")) {
				int index = btn.image.name.IndexOf ("_");
				string result = btn.name.Substring (index + 1);
				Debug.Log (result);
				return result;
			}
		}
		return "";
	}

	public void CloseCreateRoomClick() {
		createRoomPanel.SetActive (false);
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

		ResponseHandle errorHandler = delegate (string error) {
			Debug.Log("errorHandler is called");
			HideMessagePanel();
			ShowConfirmMessagePanel("连接服务器失败，请检查你的网络");	
		};

		StartCoroutine( ServerUtils.PostRequest(ServerUtils.GetRoomUrl(), JsonConvert.SerializeObject(new {roomNo = roomNo}), handler, errorHandler));
	}

	public void JoinRoomFromUrl( string url )  
	{  
		Debug.Log( "openUrl： " + url );  
		string roomNo = url.Replace ("wx73653b5260b24787://?room=", ""); 
		Debug.Log ("roomNo = " + roomNo);
		JoinRoom (roomNo);
	}  


	public void GamePropertyClick() {
		string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

		string tag = "";
		if (name.StartsWith ("jushu_")) {
			tag = "gameproperty_jushu";
			
		} else if (name.StartsWith("fangfei_")){
			tag = "gameproperty_fangfei";
		} else if (name.StartsWith("qz_")) {
			tag = "gameproperty_qz";
		} else if (name.StartsWith("wanfa_")) {
			tag = "gameproperty_wanfa";
		} else if (name.StartsWith("fengshu_")) {
			tag = "gameproperty_fengshu";
		}

		if (!string.IsNullOrEmpty (tag)) {
			GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);
			foreach (GameObject obj in objs) {
				Button btn = obj.GetComponent<Button>();
				string spriteName = "";
				if (btn.name == name) {
					spriteName = btn.name + "_y";
				} else {
					spriteName = btn.name + "_n";
				}

				Debug.Log ("spriteName = " + spriteName);
				btn.image.sprite = gamePropertiesImageDict [spriteName];
				//btn.transform.localScale = new Vector3 (1f, 1f);
			}
		}
	}

	public void ShowSettingsClick() {
		bool isMusicOn = PlayerPrefs.GetInt (Utils.Music_Key, 1) != 0;
		bool isAudioOn = PlayerPrefs.GetInt (Utils.Audio_Key, 1) != 0;
		if (isMusicOn) {
			musicButton.image.sprite = this.audioSettingsImageDict [Music_On_Key];
		} else {
			musicButton.image.sprite = this.audioSettingsImageDict [Music_Off_Key];
		}

		if (isAudioOn) {
			audioButton.image.sprite = audioSettingsImageDict [Audio_On_Key];
		} else {
			audioButton.image.sprite = audioSettingsImageDict [Audio_Off_Key];
		}
		settingsPanel.SetActive (true);
	}

	public void CloseSettingsClick() {
		settingsPanel.SetActive (false);
	}

	public void MusicButtonClick() {
		bool isMusicOn = PlayerPrefs.GetInt (Utils.Music_Key, 1) != 0;
		if (isMusicOn) {
			musicButton.image.sprite = this.audioSettingsImageDict [Music_Off_Key];
			PlayerPrefs.SetInt (Utils.Music_Key, 0);
			MusicController.instance.PlayBackgroundMusic (false);
		} else {
			musicButton.image.sprite = this.audioSettingsImageDict [Music_On_Key];
			PlayerPrefs.SetInt (Utils.Music_Key, 1);
			MusicController.instance.PlayBackgroundMusic (true);
		}

	}

	public void AudioButtonClick() {
		bool isAudioOn = PlayerPrefs.GetInt (Utils.Audio_Key, 1) != 0;
		if (isAudioOn) {
			audioButton.image.sprite = this.audioSettingsImageDict [Audio_Off_Key];
			PlayerPrefs.SetInt (Utils.Audio_Key, 0);
		} else {
			audioButton.image.sprite = this.audioSettingsImageDict [Audio_On_Key];
			PlayerPrefs.SetInt (Utils.Audio_Key, 1);
		}
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
	