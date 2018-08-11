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

	// Use this for initialization
	void Start () {
        Invoke("LineToggle", 0.3f);
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadSceneAsync("Start");

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
        SceneManager.LoadSceneAsync("Main");
    }
}
