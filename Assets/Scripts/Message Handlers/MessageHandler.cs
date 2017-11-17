using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;



public class MessageHandler<T, U> where T : BaseGameResponse 
								   where U : BaseStateController
{
	private U controller;
	private Game game;
	
	public MessageHandler (U controller, Game game)
	{
		this.controller = controller;
		this.game = game;
	}

	public void Handle(string msg) {
		//检查消息的类型，根据消息的类型，将消息转为为相应的类型。
		Debug.Log(typeof(T).Name + ": " + msg);
		T resp = JsonConvert.DeserializeObject<T>(msg);
		//Debug.Log(resp);
		if (resp.status != 0) {
			Debug.LogError("出错了");
			return;
		}

		while (!game.isInited) {
		}
		//controller.HandleResponse(resp);


		MethodInfo method = null;
		MethodInfo[] methods = controller.GetType ().GetMethods ();
		for (int i = 0; i < methods.Length; i++) {
			MethodInfo m = methods [i];
			/*
			if (m.Name == "HandleResponse") {
				Debug.Log ( m.Name);
				Debug.Log ( m.GetParameters ().Length);
				if (m.GetParameters ().Length > 0)
					Debug.Log (m.GetParameters () [0].ParameterType.Name);
				Debug.Log ("------------------------------------------");
			}*/
			if (m.Name == "HandleResponse" &&
			    m.GetParameters ().Length == 1 &&
				m.GetParameters () [0].ParameterType.Name == typeof(T).Name) {
				method = m;
				//Debug .Log("找到了");
				break;
			}
		}
			
		method.Invoke (controller, new T[]{ resp });
	}

}


