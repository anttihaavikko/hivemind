﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartView : MonoBehaviour {

    public Text positions, scores;
    int page = 0;

    public MoverWindow leaderboardsWindow, buttonsWindow, startWindow;

	// Use this for initialization
	void Start () {
        positions.text = "";
        scores.text = "";
        ScoreManager.Instance.LoadLeaderBoards(0);
        leaderboardsWindow.Show();
        buttonsWindow.Show();
        startWindow.Show();
	}
	
	// Update is called once per frame
	void Update () {
        positions.text = ScoreManager.Instance.leaderBoardPositionsString;
        scores.text = ScoreManager.Instance.leaderBoardScoresString;
	}

    public void NextPage()
    {
        page = ScoreManager.Instance.endReached ? 0 : page + 1;
        ScoreManager.Instance.LoadLeaderBoards(page);
    }

    public void StartGame()
    {
        HideWindows();
        Invoke("DelayedStartGame", 0.7f);
    }

    private void HideWindows()
    {
        leaderboardsWindow.Hide();
        buttonsWindow.Hide();
        startWindow.Hide();
    }

    public void DelayedStartGame()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            SceneManager.LoadSceneAsync("Main");
        }
        else
        {
            SceneManager.LoadSceneAsync("Entry");
        }
    }

    public void QuitGame()
    {
        HideWindows();
        Invoke("DelayedQuitGame", 0.7f);
    }

    public void DelayedQuitGame()
    {
        Debug.Log("Quit...");
        Application.Quit();
    }
}
