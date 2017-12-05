using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : BaseMonoBehaviour {
	public static MusicController instance;

	private Dictionary<string, AudioSource> manAudios = new Dictionary<string, AudioSource>();
	private Dictionary<string, AudioSource> womenAudios = new Dictionary<string, AudioSource>();
	private AudioSource bgAudioSource;

	void Awake() {
		SetUp ();
		MakeSingleton ();
	}

	void MakeSingleton() {
		if (instance != null) {
			Destroy (gameObject);
		} else {
			instance = this;
			DontDestroyOnLoad (gameObject);
		}
	}

	private void SetUp() {
		foreach (AudioItem item in AudioItem.audioItems) {
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = Resources.Load(item.fileName) as AudioClip;
			if (item.sex == AudioItem.MAN) {
				manAudios [item.name] = audioSource;
			}

		}


	    bgAudioSource = gameObject.AddComponent<AudioSource>();
		bgAudioSource.clip = Resources.Load("sounds/GameSound") as AudioClip;
		bgAudioSource.volume = 0.3f;
		bgAudioSource.loop = true;
		if (IsMusicOn())
			bgAudioSource.Play (); 
	}


	private bool IsAudioOn() {
		return PlayerPrefs.GetInt (Utils.Audio_Key, 1) != 0;
	}

	private bool IsMusicOn() {
		return PlayerPrefs.GetInt (Utils.Music_Key, 1) != 0;
	}

	public void PlayBackgroundMusic(bool play) {
		if (play) {
			bgAudioSource.Play ();
		} else {
			bgAudioSource.Stop ();
		}
	}

	public void Play(string audioName, int sex = 1, bool isLoop = false ) {
		if (!IsAudioOn()) {
			return;
		}

		if (manAudios.ContainsKey (audioName)) {
			manAudios [audioName].loop = isLoop;
			if (!manAudios [audioName].isPlaying) {
				manAudios [audioName].Play ();
			}
		}
	}

	public void Stop(string audioName, int sex = 1) {
		if (!IsAudioOn()) {
			return;
		}

		if (manAudios.ContainsKey(audioName)) {
			if (manAudios[audioName].isPlaying) {
				manAudios[audioName].Stop();
			}
		}
	}
}


public class AudioItem {
	public static int MAN = 1;
	public static int WOMEN = 2;
	public string name = "";
	public int sex = 0;
	public string fileName = "";

	public AudioItem(string name, int sex, string fileName) {
		this.name = name;
		this.sex = sex;
		this.fileName = fileName;
	}

	public static List<AudioItem> audioItems = new List<AudioItem>();
    static AudioItem() {
		audioItems.Add(new AudioItem(Bet, MAN, "sounds/audio_raise"));
		audioItems.Add(new AudioItem(Bet, WOMEN, "sounds/audio_raise"));

		audioItems.Add(new AudioItem(Banker, MAN, "sounds/banker2"));
		audioItems.Add(new AudioItem(Banker, WOMEN, "sounds/banker2"));

		audioItems.Add(new AudioItem(Deal, MAN, "sounds/dell"));
		audioItems.Add(new AudioItem(Deal, WOMEN, "sounds/dell"));
		//audioItems.Add(new AudioItem(Deal, MAN, "sounds/deal_card"));
		//audioItems.Add(new AudioItem(Deal, WOMEN, "sounds/deal_card"));

		/*
		audioItems.Add(new AudioItem(Ready, MAN, "sounds/ready"));
		audioItems.Add(new AudioItem(Ready, WOMEN, "sounds/ready"));
		*/

		audioItems.Add(new AudioItem(Rob, MAN, "sounds/qiang"));
		audioItems.Add(new AudioItem(Rob, WOMEN, "sounds/qiang"));

		audioItems.Add(new AudioItem(NotRob, MAN, "sounds/buqiang"));
		audioItems.Add(new AudioItem(NotRob, WOMEN, "sounds/buqiang"));

		audioItems.Add(new AudioItem(TransmitCoin, MAN, "sounds/translate_coins"));
		audioItems.Add(new AudioItem(TransmitCoin, WOMEN, "sounds/translate_coins"));

		//audioItems.Add(new AudioItem(RandomSelectBanker, MAN, "sounds/random_banker"));
		//audioItems.Add(new AudioItem(RandomSelectBanker, WOMEN, "sounds/random_banker"));

		for (int i = 0; i < 13; i++) {
			audioItems.Add(new AudioItem("niu"+i, MAN, "sounds/man/cow_"+i));
		}
	}

	public static string Bet = "Bet";
	public static string Banker = "Banker";
	public static string Deal = "Deal";
	public static string Ready = "Ready";
	public static string Rob = "Rob";
	public static string NotRob = "NotRob";
	public static string TransmitCoin = "TransmitCoin";
	public static string RandomSelectBanker = "RandomSelectBanker";
	public static string niu0 = "niu0";
	public static string niu1 = "niu1";
	public static string niu2 = "niu2";
	public static string niu3 = "niu3";
	public static string niu4 = "niu4";
	public static string niu5 = "niu5";
	public static string niu6 = "niu6";
	public static string niu7 = "niu7";
	public static string niu8 = "niu8";
	public static string niu9 = "niu9";
	public static string niu10 = "niu10";
	public static string niu11 = "niu11";
	public static string niu12 = "niu12";
	public static string niu13 = "niu13";
}


