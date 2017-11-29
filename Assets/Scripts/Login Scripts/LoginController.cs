using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using cn.sharesdk.unity3d;
using Newtonsoft.Json;

public class LoginController : MonoBehaviour {

	public static bool isFromLogin = false;

	[SerializeField]
	private ShareSDK ssdk;

	[SerializeField]
	private Text debugLabel;

	[SerializeField]
	private Text userInfoLabel;

	[SerializeField]
	private GameObject loginTipPanel;

	void Start() {
		ssdk.authHandler = AuthResultHandler;
		ssdk.showUserHandler = GetUserInfoResultHandler;
	}

	public void LoginClick() {
		isFromLogin = true;
		Debug.Log ("login clicked");
		//Player.Me = CreateMockPlayer ();
		//GoToMainPage ();
		ssdk.Authorize (PlatformType.WeChat);
		//TODO： 应该有个动画
	}

	public void AuthResultHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		Debug.Log ("AuthResultHandler called");
		if (state == ResponseState.Success)
		{
			Debug.Log ("authorize success !");
			debugLabel.text = "authorize success !";
			ssdk.GetUserInfo(PlatformType.WeChat);
			loginTipPanel.SetActive (true);
		}
		else if (state == ResponseState.Fail)
		{
			Debug.Log  ("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
			debugLabel.text = "fail! throwable stack = " + result ["stack"] + "; error msg = " + result ["msg"];
		}
		else if (state == ResponseState.Cancel) 
		{
			Debug.Log  ("cancel !");
			debugLabel.text = "cancel !";
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
			userInfoLabel.text = me.nickname + " " + me.userId;

			ResponseHandle handler = (string msg) => {
				loginTipPanel.SetActive (false);
				LoginResponse resp = JsonConvert.DeserializeObject<LoginResponse>(msg);
				if (resp.status != 0) {
					Debug.LogError("登陆失败，errorMessage = " + resp.errorMessage);
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
		}
		else if (state == ResponseState.Cancel) 
		{
			print ("cancel !");
		}
	}

	private void GoToMainPage() {
		SceneManager.LoadScene ("MainPage");
	}

	public static Player CreateMockPlayer() {
		string json = @"{""country"":""CN"", ""province"":""Zhejiang"", ""headimgurl"":""http://www.gx8899.com/uploads/allimg/2016101713/0rbdwhgad3z.jpg"", ""unionid"":""omrAqw3jZJyVBtnJHN2atDrlddFDRY"", ""openid"":""oa75AwCba3xVbNeCNraVMZENVeg0"", ""nickname"":""\u91d1"", ""city"":""Jinhua"", ""sex"":1, ""language"":""zh_CN"", ""privilege"":[]}";
		Player me = JsonConvert.DeserializeObject<Player> (json);
		me.userId = "7654321";
		me.nickname = "7654321";
		return me;
	}

}
