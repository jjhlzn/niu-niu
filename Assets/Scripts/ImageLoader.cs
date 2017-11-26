using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

//将图片进行缓存
public class ImageLoader : MonoBehaviour
{
	Dictionary<string, Sprite> dict = new Dictionary<string, Sprite> ();
	private static ImageLoader _instance;
	public static ImageLoader instance
	{
		get    
		{
			//If _instance hasn't been set yet, we grab it from the scene!
			//This will only happen the first time this reference is used.

			if(_instance == null)
				_instance = GameObject.FindObjectOfType<ImageLoader>();
			return _instance;
		}
	}

	public void Load(string url, ImageHandler imageHanlder) {
		//return;
		if (string.IsNullOrEmpty (url))
			return;
		
		if (dict.ContainsKey (url)) {
			imageHanlder( dict [url] );
			return;
		}
		StartCoroutine(LoadImage(url, imageHanlder));
	}

	IEnumerator LoadImage(string url, ImageHandler imageHanlder) {
		WWW www = new WWW(url);
		yield return www;

		//Debug.Log ("www.texture.width = " + www.texture.width);

		//www.texture.Compress(false);
		Sprite sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width,www.texture.height),  new Vector3 (0, 0, 0));
		dict [url] = sprite;
		imageHanlder (sprite); 


	}
}
	
public delegate void ImageHandler(Sprite sprite);


