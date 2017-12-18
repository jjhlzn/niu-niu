using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using cn.sharesdk.unity3d;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Net;
using System;
using System.IO;

public class LoginController : BaseMonoBehaviour {

	public static bool isFromLogin = false;
	public static string roomNo = "";

	[SerializeField]
	private ShareSDK ssdk;


	[SerializeField]
	private GameObject messagePanel;
	[SerializeField]
	private GameObject confirmMessagePanel;
	[SerializeField]
	private Text versionLabel;
	[SerializeField]
	private Button loginButton;
	[SerializeField]
	private InputField userNameInputField;
	[SerializeField]
	private InputField passwordInputField;
	private Sprite loginBtnSprite;

	private WWW www;
	private bool isDonwloading;
	private string newVersion;

	private bool isAuditVersion;

	void Start() {
		isFromLogin = true;
		versionLabel.text = "版本号: " + Utils.GetVersion ();

		ssdk.authHandler = AuthResultHandler;
		ssdk.showUserHandler = GetUserInfoResultHandler;


		passwordInputField.inputType = InputField.InputType.Password;
		loginBtnSprite = Resources.Load<Sprite> ("sprites/loginpage/login_btn");

		/*正式代码
		CheckUpdate ();
		*/
		CheckiOSAuditVersion ();
	}

	void Update() {
		if (isDonwloading && www != null) {
			//Debug.Log ("progress = " + www.progress);
			Utils.ShowMessagePanel ("发现新版本:" + newVersion  + ", 已下载" + (int)(www.progress * 100) + '%', messagePanel);
		}
	}

	public void CheckiOSAuditVersion() {
		//iOS版本，检查是否是iOS审核版本
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			Utils.ShowMessagePanel ("正在获取服务器信息...", messagePanel);
			ResponseHandle handler = delegate(string jsonString){
				Debug.Log("GetCheckIOSAuditUpdateVersion: " + jsonString);
				Utils.HideMessagePanel (messagePanel);
				//加入玩家已经游戏了，那么跳转到Gameplay Scene。否则什么都不需要坐。
				CheckIOSAuditVersionResponse resp = JsonConvert.DeserializeObject<CheckIOSAuditVersionResponse>(jsonString);
				if (resp.isAuditVersion) {
					userNameInputField.gameObject.SetActive(true);
					passwordInputField.gameObject.SetActive(true);
					loginButton.image.sprite = loginBtnSprite;
					isAuditVersion = true;
				} else {
					CheckUpdate();
				}
			};

			ResponseHandle errorHandler = delegate (string error) {
				Debug.Log("errorHandler is called");
				Utils.HideMessagePanel (messagePanel);
				Utils.ShowConfirmMessagePanel("连接服务器失败，请检查你的网络", confirmMessagePanel);	
			};

			var req = new {
				platform = Utils.GetPlatform(),
				version = Application.version
			};

			StartCoroutine(ServerUtils.PostRequest (ServerUtils.CheckIOSAuditVersionUrl(), JsonConvert.SerializeObject(req), handler, errorHandler));
		} else {
			CheckUpdate ();
		}

	}

	private void CheckUpdate() {
		if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
			return;

		if (Application.platform != RuntimePlatform.IPhonePlayer)
			Utils.ShowMessagePanel ("正在检查新版本...", messagePanel);
		
		ResponseHandle handler = delegate(string jsonString){
			Debug.Log("GetCheckUpdate: " + jsonString);
			Utils.HideMessagePanel (messagePanel);
			//加入玩家已经游戏了，那么跳转到Gameplay Scene。否则什么都不需要坐。
			CheckUpdateResponse resp = JsonConvert.DeserializeObject<CheckUpdateResponse>(jsonString);
			if (resp.isNeedUpdate) {
				if (Application.platform == RuntimePlatform.Android) {
					newVersion = resp.newVersion;
					StartCoroutine(DownloadApk(resp.updateUrl));
				} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
					Utils.ShowConfirmMessagePanel("发现新版本，请使用TestFlight下载最新版本！", confirmMessagePanel); 
				}
			} else {
				//Utils.ShowConfirmMessagePanel("没有新版本", confirmMessagePanel);
				AutoLogin();
			}
		};

		ResponseHandle errorHandler = delegate (string error) {
			Debug.Log("errorHandler is called");
			Utils.HideMessagePanel (messagePanel);
			Utils.ShowConfirmMessagePanel("连接服务器失败，请检查你的网络", confirmMessagePanel);	
		};

		var req = new {
			platform = Utils.GetPlatform(),
			version = Application.version
		};

		StartCoroutine(ServerUtils.PostRequest (ServerUtils.CheckUpdateUrl(), JsonConvert.SerializeObject(req), handler, errorHandler));
	}

	public void MessagePanelSureButtonClick() {
		Utils.HideMessagePanel (confirmMessagePanel);
	}

	public void AutoLogin() {
		Utils.ShowMessagePanel ("登陆中...", messagePanel);

		ssdk.GetUserInfo(PlatformType.WeChat);
	}

	public void LoginClick() {
		Debug.Log ("login clicked");
		Utils.ShowMessagePanel ("登陆中...", messagePanel);

		//iOS版本，如果是iOS审核版本，那么就该使用审核版本的登陆
		if (isAuditVersion && Application.platform == RuntimePlatform.IPhonePlayer) {

			var req = new {
				username = userNameInputField.text,
				password = passwordInputField.text
			};
			ResponseHandle handler = (string msg) => {
				Utils.HideMessagePanel(messagePanel);
				LoginResponse resp = JsonConvert.DeserializeObject<LoginResponse>(msg);
				if (resp.status != 0) {
					Debug.LogError("登陆失败，errorMessage = " + resp.errorMessage);
					Utils.ShowConfirmMessagePanel(resp.errorMessage, confirmMessagePanel);
					return;
				}
				Player me = JsonConvert.DeserializeObject<Player> (msg);
				Player.Me = me;
				Scenes.Load("MainPage", new Dictionary<string, string>());
			};

			StartCoroutine (ServerUtils.PostRequest (ServerUtils.AuditLoginUrl(), JsonConvert.SerializeObject(req), handler));
			
		} else {
			
			ssdk.Authorize (PlatformType.WeChat);
		}

	}

	public void XieyiClick() {
		Application.OpenURL ("http://niu.yhkamani.com/xieyi.html");
	}

	private void InstallUpdate(string url) {
		Debug.Log ("Application.platform = " + Application.platform);
		if (Application.platform != RuntimePlatform.Android)
			return;

		string arguments = "";
		AndroidJavaClass UpdatePlugin = new AndroidJavaClass("com.jinjunhang.onlineclass.updateplugin.UpdatePlugin"); 

		AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		UpdatePlugin.CallStatic<Boolean>("InstallUpdate", currentActivity, url);
	}


	private IEnumerator DownloadApk(string url)
	{
		this.www = new WWW(url);

		isDonwloading = true;
		yield return www;
	
		if (www.isDone) {
			Debug.Log ("Download success");
			try
			{
				string filePath = Application.persistentDataPath + "/" + "niuniu.apk";
				using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					fs.Write(www.bytes, 0, www.bytes.Length);
					Debug.Log("save apk success");
					InstallUpdate(filePath);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception caught in process: " + ex);
			}

		}
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.Log ("Download fail");
		}

		isDonwloading = false;
	}
		

	void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
	{
		if (e.Error == null) {
			//AllDone ();
			Debug.Log ("No Error happened.");
		} else {
			Debug.LogError ("Error hanpped");
		}
	}

	public void AuthResultHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		Debug.Log ("AuthResultHandler called");
		if (state == ResponseState.Success)
		{
			Debug.Log ("authorize success !");
			ssdk.GetUserInfo(PlatformType.WeChat);
		}
		else if (state == ResponseState.Fail)
		{
			Debug.Log  ("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
			Utils.HideMessagePanel (messagePanel);
			Utils.ShowConfirmMessagePanel ("登陆失败", confirmMessagePanel);
		}
		else if (state == ResponseState.Cancel) 
		{
			Debug.Log  ("cancel !");
			Utils.HideMessagePanel (messagePanel);
			//Utils.ShowConfirmMessagePanel ("登陆失败", confirmMessagePanel);
		}
	}

	void GetUserInfoResultHandler (int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		Debug.Log ("GetUserInfoResultHandler called");
		if (state == ResponseState.Success)
		{
			print ("get user info result :");
			print (MiniJSON.jsonEncode(result));

			Player me = JsonConvert.DeserializeObject<Player> (MiniJSON.jsonEncode(result));
			Player.Me = me;

			ResponseHandle handler = (string msg) => {
				Utils.HideMessagePanel(messagePanel);

				LoginResponse resp = JsonConvert.DeserializeObject<LoginResponse>(msg);
				if (resp.status != 0) {
					Debug.LogError("登陆失败，errorMessage = " + resp.errorMessage);
					Utils.ShowConfirmMessagePanel(resp.errorMessage, confirmMessagePanel);
					return;
				}
				Player.Me.userId = resp.userId;
				//Parameters<string, string>
				Scenes.Load("MainPage", new Dictionary<string, string>());
			};

			StartCoroutine (ServerUtils.PostRequest (ServerUtils.GetLoginUrl(), MiniJSON.jsonEncode(result), handler));
		}
		else if (state == ResponseState.Fail)
		{
			print ("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
			Utils.HideMessagePanel(messagePanel);
			//Utils.ShowConfirmMessagePanel ("登陆失败", confirmMessagePanel);
		}
		else if (state == ResponseState.Cancel) 
		{
			print ("cancel !");
			Utils.HideMessagePanel(messagePanel);
		}
	}

	private void GoToMainPage() {
		SceneManager.LoadScene ("MainPage");
	}

	private void LoginAsMockPlayer() {
		Player.Me = CreateMockPlayer ();
		Scenes.Load("MainPage", new Dictionary<string, string>());
	}

	public static Player CreateMockPlayer() {
		string json = @"{""country"":""CN"", ""province"":""Zhejiang"", ""headimgurl"":""http://www.gx8899.com/uploads/allimg/2016101713/0rbdwhgad3z.jpg"", ""unionid"":""omrAqw3jZJyVBtnJHN2atDrlddFDRY"", ""openid"":""oa75AwCba3xVbNeCNraVMZENVeg0"", ""nickname"":""\u91d1"", ""city"":""Jinhua"", ""sex"":1, ""language"":""zh_CN"", ""privilege"":[]}";
		Player me = JsonConvert.DeserializeObject<Player> (json);
		me.userId = "7654321";
		me.nickname = "漫天飞雪";
		return me;
	}


	private void LoadActivityParamsForAndroid() {
		if (Application.platform != RuntimePlatform.Android)
			return;

	    roomNo = Utils.GetRoomNoFromIntentUrl ();
		Debug.Log ("roomNo = " + roomNo);
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus) {
		} else {  //回到主界面
			LoadActivityParamsForAndroid();
		}
	}
}
