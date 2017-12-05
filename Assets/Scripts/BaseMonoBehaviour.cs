using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class BaseMonoBehaviour : MonoBehaviour
{
	private bool hadDestroied = false;
	//Called when there is an exception

	void OnDestroy() {
		hadDestroied = true;
	}

	public void LogCallback(string condition, string stackTrace, LogType type)
	{
		//Send Email
		if (type == LogType.Exception || type == LogType.Error) {
			var req = new {
				client = new {
					platform = Utils.GetPlatform(),
					version = Utils.GetVersion()
				},
				user = new {
					id = Player.Me != null ? Player.Me.userId : "",
					nickname = Player.Me != null ? Player.Me.nickname : "",
				},
				type = type,
				condition = condition,
				stackTrace = stackTrace
			};

			//Debug.Log ("-----------------------------------------------------------------");
			//Debug.Log ("LogType: " + type + ", condition: " + condition);
			//Debug.Log ("stackTrace: " + stackTrace);
			//Debug.Log ("-----------------------------------------------------------------");
			//if (gameObject.activeInHierarchy)
			//Debug.Log("reportErrorUrl: " + ServerUtils.ReportErrorUrl());
			if (!hadDestroied)
				StartCoroutine (ServerUtils.PostRequest(ServerUtils.ReportErrorUrl(),  JsonConvert.SerializeObject(req), null, null));
		}
	}

	 /*
	void OnEnable()
	{
		Application.logMessageReceived += LogCallback;
	}

	void OnDisable() {
		
		Application.logMessageReceived -= LogCallback;
	}

	//Called when there is an exception
	void LogCallback(string condition, string stackTrace, LogType type)
	{
		//Send Email
		if (type == LogType.Exception || type == LogType.Error) {
			var req = new {
				type = type,
				condition = condition,
				stackTrace = stackTrace
			};

			Debug.Log ("-----------------------------------------------------------------");
			Debug.Log ("LogType: " + type + ", condition: " + condition);
			Debug.Log ("stackTrace: " + stackTrace);
			Debug.Log ("-----------------------------------------------------------------");
			if (gameObject.activeInHierarchy)
				StartCoroutine (PostRequest(ServerUtils.ReportErrorUrl(),  JsonConvert.SerializeObject(req), null, null));
		}
	}

	public static IEnumerator PostRequest(string url, string json, ResponseHandle handle, ResponseHandle errorHandle = null)
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
			if (errorHandle != null)
				errorHandle (req.error);
			else {
				//Debug.Log ("errorHandler is null");
			}
		}
	}
	*/
}


