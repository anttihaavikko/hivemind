using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EntryView : MonoBehaviour {

    public Text nameText;
    string playerName = "";
    bool showLine;
    bool canWrite = true;

    public RectTransform window;
    Vector3 windowPosition;

	// Use this for initialization
	void Start () {
        Invoke("LineToggle", 0.3f);
        ShowWindow();
	}

    void ShowWindow()
    {
        windowPosition = window.localPosition;
        Tweener.Instance.MoveLocalTo(window, Vector3.zero, 1f, 0f, TweenEasings.BounceEaseOut);
        AudioManager.Instance.PlayEffectAt(11, new Vector3(0f, -3f, 0f), 0.15f);
        StartCoroutine(DoWindowBounceSound(12, 0.3f, new Vector3(0f, -3f, 0f), 0.5f));
    }

    void HideWindow()
    {
        Tweener.Instance.MoveLocalTo(window, windowPosition, 0.4f, 0f, TweenEasings.QuadraticEaseOut);
        AudioManager.Instance.PlayEffectAt(11, Vector3.zero, 0.15f);
    }

    IEnumerator DoWindowBounceSound(int soundIndex, float delayTime, Vector3 pos, float volume)
    {
        yield return new WaitForSeconds(delayTime);
        AudioManager.Instance.PlayEffectAt(soundIndex, pos, volume);
        yield return new WaitForSeconds(0.15f);
        AudioManager.Instance.PlayEffectAt(soundIndex, pos, volume - 0.1f);
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.Escape)) {
            HideWindow();
            Invoke("GoToStart", 0.7f);
        }

        if (!canWrite)
            return;

        foreach (char c in Input.inputString)
        {

            if (c == "\b"[0])
            {

                if (playerName.Length != 0)
                {
                    playerName = playerName.Substring(0, playerName.Length - 1);
                    //AudioManager.Instance.PlayEffectAt(AudioManager.WRONG, Vector3.zero, 1f);
                }

            }
            else
            {

                if (c == ';' || c == ',' || c == ':' || c == ' ')
                {
                    return;
                }

                if (c == "\n"[0] || c == "\r"[0])
                {

                    if (playerName != "")
                        AnimateAndStart();

                }
                else
                {
                    if (playerName.Length < 9)
                    {
                        playerName += c.ToString().ToUpper();
                        //AudioManager.Instance.PlayEffectAt(AudioManager.CHARGE, Vector3.zero, 1f);
                    }
                }

            }
        }

        nameText.text = playerName + (showLine && canWrite ? "" : "_");
	}

    void LineToggle()
    {
        showLine = !showLine;
        Invoke("LineToggle", 0.3f);
    }

    public void StartGame()
    {
        if (playerName != "")
            AnimateAndStart();
    }

    void AnimateAndStart()
    {
        canWrite = false;
        PlayerPrefs.SetString("PlayerName", playerName);
        HideWindow();
        Invoke("GoToMain", 0.7f);
    }

    void GoToMain()
    {
        SceneManager.LoadSceneAsync("Main");
    }

    void GoToStart()
    {
        SceneManager.LoadSceneAsync("Start");
    }
}
