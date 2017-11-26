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

	//
	public void Handle(string msg) {
		
		//检查消息的类型，根据消息的类型，将消息转为为相应的类型。
		Debug.Log(typeof(T).Name + ": " + msg);
		T resp = JsonConvert.DeserializeObject<T>(msg);
		//Debug.Log(resp);
		if (resp.status != 0) {
			Debug.LogError("出错了");
			return;
		}

		if (!this.game.isInited) {
			return;
		}
	
		MethodInfo method = null;
		MethodInfo[] methods = controller.GetType ().GetMethods ();
		for (int i = 0; i < methods.Length; i++) {
			MethodInfo m = methods [i];
			if (m.Name == "HandleResponse" &&
			    m.GetParameters ().Length == 1 &&
				m.GetParameters () [0].ParameterType.Name == typeof(T).Name) {
				method = m;
				//Debug .Log("找到了");
				break;
			}
		}
			
		method.Invoke (controller, new T[]{ resp });
		if (game.isPause) {
			//加入队列。
		} else {
			
		}
	}

}


