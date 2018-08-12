using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsView : MonoBehaviour {

	private bool starting = false;

	public Slider musicSlider, soundSlider;

	private bool optionsOpen = false;
	private bool canQuit = false;
    private float prevSoundStep;

    public MoverWindow optionsWindow;

	void Start() {
		soundSlider.value = AudioManager.Instance.volume;
		musicSlider.value = AudioManager.Instance.curMusic.volume * 0.5f;

        prevSoundStep = AudioManager.Instance.volume;

        optionsWindow.Show();
	}

	void EnableQuit() {
		canQuit = true;
	}

	void DoInputs() {

		if (Input.GetKeyUp (KeyCode.Escape)) {
			canQuit = true;
			return;
		}

		if (!canQuit) {
			return;
		}
	}
	
	// Update is called once per frame
	void Update () {

		DoInputs ();
	}

	public void ChangeMusicVolume() {
		AudioManager.Instance.curMusic.volume = musicSlider.value;
        AudioManager.Instance.ChangeMusicVolume(musicSlider.value);
	}

	public void ChangeSoundVolume() {
		if (Mathf.Abs(soundSlider.value - prevSoundStep) > 0.075f) {
            AudioManager.Instance.PlayEffectAt (13, Vector3.zero, 1f);
            prevSoundStep = soundSlider.value;
		}

        AudioManager.Instance.volume = soundSlider.value;
        PlayerPrefs.SetFloat("SoundVolume", soundSlider.value);
	}

    public void EraseSave()
    {
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("HiScore");
        PlayerPrefs.DeleteKey("CheckNumber");
        PlayerPrefs.DeleteKey("Tutorial");
        PlayerPrefs.DeleteKey("ComboTutorial");
        PlayerPrefs.DeleteKey("VotedPosts");
    }

    public void BackToStart()
    {
        optionsWindow.Hide();
        Invoke("DoBack", 1f);
    }

    void DoBack()
    {
        SceneManager.LoadSceneAsync("Start");
    }
}
