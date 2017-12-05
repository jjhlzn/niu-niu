using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using socket.io;

//将图片进行缓存
public class ImageLoader : BaseMonoBehaviour
{
	private Dictionary<string, Sprite> dict = new Dictionary<string, Sprite> ();

	public static ImageLoader Instance;

	void Awake() {
		MakeSingleton ();
	}

	void MakeSingleton() {
		if (Instance != null) {
			Destroy (gameObject);
		} else {
			Instance = this;
			DontDestroyOnLoad (gameObject);
		}
	}

	public void Load(string url, ImageHandler imageHanlder) {
		
		//return;
		if (string.IsNullOrEmpty (url))
			return;
		
		if (dict.ContainsKey (url)) {
			Debug.Log ("Find Image in Cache, url = " + url);
			imageHanlder( dict [url] );
			return;
		}

		Debug.Log ("load Image from network, url = " + url);
		StartCoroutine(LoadImage(url, imageHanlder));
	}

	IEnumerator LoadImage(string url, ImageHandler imageHanlder) {
		WWW www = new WWW(url);
		yield return www;

		//Debug.Log ("www.texture.width = " + www.texture.width);

		//www.texture.Compress(false);
		if (www != null && www.texture != null) { 
			Sprite sprite = Sprite.Create (www.texture, new Rect (0, 0, www.texture.width, www.texture.height), new Vector3 (0, 0, 0));
			dict [url] = sprite;
			imageHanlder (sprite); 
		}
	}
}
	
public delegate void ImageHandler(Sprite sprite);


