﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public AudioSource curMusic;
	public AudioSource[] musics;

	public float volume = 0.5f;
	private float musVolume = 0.5f;
	public SoundEffect effectPrefab;
	public AudioClip[] effects;

	public AudioLowPassFilter lowpass;
	public AudioHighPassFilter highpass;

	// private AudioReverbFilter reverb;
	// private AudioReverbPreset fromReverb, toReverb;

	private Animator anim;
	private AudioSource prevMusic;

	private float fadeOutPos = 0f, fadeInPos = 0f;
	private float fadeOutDuration = 1f, fadeInDuration = 3f;

	private bool doingLowpass, doingHighpass;

    public float targetPitch = 1f;

	/******/

	private static AudioManager instance = null;
	public static AudioManager Instance {
		get { return instance; }
	}

	void Awake() {
		if (instance != null && instance != this) {
			Destroy (this.gameObject);
			return;
		} else {
			instance = this;
		}

		// reverb = GetComponent<AudioReverbFilter> ();
        //
		// fromReverb = AudioReverbPreset.Hallway;
		// toReverb = AudioReverbPreset.Off;

		DontDestroyOnLoad(instance.gameObject);

        if (PlayerPrefs.HasKey("MusicVolume"))
            ChangeMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));

        if (PlayerPrefs.HasKey("SoundVolume"))
            volume = PlayerPrefs.GetFloat("SoundVolume");
	}

	public void BackToDefaultMusic() {
		if (curMusic != musics [0]) {
			ChangeMusic (0, 0.5f, 2f, 1f);
		}
	}

	public void Lowpass(bool state = true) {
		doingLowpass = state;
		doingHighpass = false;
	}

	public void Highpass(bool state = true) {
		doingHighpass = state;
		doingLowpass = false;
	}

	public void ChangeMusic(int next, float fadeOutDur, float fadeInDur, float startDelay) {
		fadeOutPos = 0f;
		fadeInPos = -1f;

		fadeOutDuration = fadeOutDur;
		fadeInDuration = fadeInDur;

		prevMusic = curMusic;
		curMusic = musics [next];

		prevMusic.time = 0f;

		Invoke ("StartNext", startDelay);
	}

	private void StartNext() {
		fadeInPos = 0f;
		curMusic.time = 0f;
		curMusic.volume = 0f;
		curMusic.Play ();
	}

	void Start() {
	}

	void Update() {

        if (fadeInPos < 1f)
        {
            fadeInPos += Time.unscaledDeltaTime / fadeInDuration;
        }

        if (fadeOutPos < 1f)
        {
            fadeOutPos += Time.unscaledDeltaTime / fadeOutDuration;
        }

        if (curMusic && fadeInPos >= 0f)
        {
            curMusic.volume = Mathf.Lerp(0f, musVolume, fadeInPos);
        }

        if (prevMusic)
        {
            prevMusic.volume = Mathf.Lerp(musVolume, 0f, fadeOutPos);

            if (prevMusic.volume <= 0f)
            {
                prevMusic.Stop();
            }
        }

		float targetLowpass = (doingLowpass) ? 5000f : 15000f;
		float targetHighpass = (doingHighpass) ? 600f : 10f;
		float changeSpeed = 0.1f;

		curMusic.pitch = Mathf.MoveTowards (curMusic.pitch, targetPitch, 0.005f * changeSpeed);
		lowpass.cutoffFrequency = Mathf.MoveTowards (lowpass.cutoffFrequency, targetLowpass, 750f * changeSpeed);
		highpass.cutoffFrequency = Mathf.MoveTowards (highpass.cutoffFrequency, targetHighpass, 100f * changeSpeed);
	}

	public void PlayEffectAt(AudioClip clip, Vector3 pos, float volume, bool pitchShift = true) {
		SoundEffect se = Instantiate (effectPrefab, pos, Quaternion.identity);
		se.Play (clip, volume, pitchShift);
		se.transform.parent = transform;
	}

	public void PlayEffectAt(AudioClip clip, Vector3 pos, bool pitchShift = true) {
		PlayEffectAt (clip, pos, 1f, pitchShift);
	}

	public void PlayEffectAt(int effect, Vector3 pos, bool pitchShift = true) {
		PlayEffectAt (effects [effect], pos, 1f, pitchShift);
	}

	public void PlayEffectAt(int effect, Vector3 pos, float volume, bool pitchShift = true) {
		PlayEffectAt (effects [effect], pos, volume, pitchShift);
	}

	public void ChangeMusicVolume(float vol) {
        PlayerPrefs.SetFloat("MusicVolume", vol);
		curMusic.volume = vol * 2f;
		musVolume = vol * 2f;
	}
}
