using UnityEngine;
using System.Collections;
using System;

namespace cn.sharesdk.unity3d 
{
	[Serializable]
	public class DevInfoSet
	{
		
		public WeChat wechat;
		public WeChatMoments wechatMoments; 
		public WeChatFavorites wechatFavorites;


		#if UNITY_ANDROID

		#elif UNITY_IPHONE		

		public WechatSeries wechatSeries;						//iOS端微信系列, 可直接配置微信三个子平台 		[仅支持iOS端]
					
		#endif

	}

	public class DevInfo 
	{	
		public bool Enable = true;
	}



	
	[Serializable]
	public class WeChat : DevInfo 
	{	
		#if UNITY_ANDROID
		public string SortId = "5";
		public const int type = (int) PlatformType.WeChat;
		public string AppId = "wx73653b5260b24787";
		public string AppSecret = "fc31886ecfd2fe190822b4fb72adb9f7";
		public string userName = "gh_afb25ac019c9@app";
		public string path = "/page/API/pages/share/share";
		public bool BypassApproval = true;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.WeChat;
		public string app_id = "wx73653b5260b24787";
		public string app_secret = "fc31886ecfd2fe190822b4fb72adb9f7";
		#endif
	}

	[Serializable]
	public class WeChatMoments : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "6";
		public const int type = (int) PlatformType.WeChatMoments;
		public string AppId = "wx73653b5260b24787";
		public string AppSecret = "fc31886ecfd2fe190822b4fb72adb9f7";
		public bool BypassApproval = false;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.WeChatMoments;
		public string app_id = "wx73653b5260b24787";
		public string app_secret = "fc31886ecfd2fe190822b4fb72adb9f7";
		#endif
	}

	[Serializable]
	public class WeChatFavorites : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "7";
		public const int type = (int) PlatformType.WeChatFavorites;
		public string AppId = "wx73653b5260b24787";
		public string AppSecret = "fc31886ecfd2fe190822b4fb72adb9f7";
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.WeChatFavorites;
		public string app_id = "wx73653b5260b24787";
		public string app_secret = "fc31886ecfd2fe190822b4fb72adb9f7";
		#endif
	}


	// 		安卓描述:   
	//		在中国大陆，印象笔记有两个服务器，一个是沙箱（sandbox），一个是生产服务器（china）。
	//		一般你注册应用，它会先让你使用sandbox，当你完成测试以后，可以到
	//		http://dev.yinxiang.com/support/上激活你的ConsumerKey，激活成功后，修改HostType
	//		为china就好了。至于如果您申请的是国际版的印象笔记（Evernote），则其生产服务器类型为
	//		“product”。
	//		如果目标设备上已经安装了印象笔记客户端，ShareSDK允许应用调用本地API来完成分享，但
	//		是需要将应用信息中的“ShareByAppClient”设置为true，此字段默认值为false。
	//      

	//      iOS描述:
	//		配置好consumerKey 和 secret, 如果为沙箱模式，请对参数isSandBox传入非0值，否则传入0.

	//在以下的配置里，安卓请选择Evernote配置。
	//iOS则需要区分，国内版为Evernote，国际版EvernoteInternational。


	[Serializable]
	public class Copy : DevInfo 
	{
		#if UNITY_ANDROID
		public const int type = (int) PlatformType.Copy;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.Copy;
		#endif
	}




	[Serializable]
	public class WechatSeries : DevInfo 
	{	
		#if UNITY_ANDROID
		//for android,please set the configuraion in class "Wechat" ,class "WechatMoments" or class "WechatFavorite"
		//对于安卓端，请在类Wechat,WechatMoments或WechatFavorite中配置相关信息↑	
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.WechatPlatform;
		public string app_id = "wx73653b5260b24787";
		public string app_secret = "fc31886ecfd2fe190822b4fb72adb9f7";
		#endif
	}

	
}
