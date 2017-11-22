using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainPageController : MonoBehaviour {

	[SerializeField]
	private Text nickNameLabel;
	[SerializeField]
	private Text idLabel;
	[SerializeField]
	private Text coinLabel;
	[SerializeField]
	private Image userImage;

	// Use this for initialization
	void Start () {
		Player.Me = LoginController.CreateMockPlayer ();

		nickNameLabel.text = Player.Me.nickname;
		idLabel.text = "ID: " + Player.Me.userId;
		coinLabel.text = "1000";
		StartCoroutine (LoadImage (Player.Me.headimgurl));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void CreateRoomClick() {
		//创建房间
		Debug.Log("create room click");
		SceneManager.LoadScene("Gameplay");
	}

	public void JoinRoomClick() {
	}

	IEnumerator LoadImage(string url) {
		WWW www = new WWW(url);
		yield return www;
		userImage.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
		Player.Me.userHeadImage = userImage.sprite; 
	}
}
