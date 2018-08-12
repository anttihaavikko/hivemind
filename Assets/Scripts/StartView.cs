using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartView : MonoBehaviour {

    public Text positions, scores;
    public Text pbPos, pbScore;
    int page = 0;

    public MoverWindow leaderboardsWindow, buttonsWindow, startWindow;
    public Transform stripe;

	// Use this for initialization
	void Start () {
        positions.text = "";
        scores.text = "";
        ScoreManager.Instance.LoadLeaderBoards(0);
        leaderboardsWindow.Show();
        buttonsWindow.Show();
        startWindow.Show();
        Tweener.Instance.ScaleTo(stripe, Vector3.one, 0.5f, 0f, TweenEasings.BounceEaseOut);

        //if (Application.isEditor) PlayerPrefs.DeleteKey("PlayerName");
	}
	
	// Update is called once per frame
	void Update () {
        positions.text = ScoreManager.Instance.leaderBoardPositionsString;
        scores.text = ScoreManager.Instance.leaderBoardScoresString;

        pbPos.text = ScoreManager.Instance.personalBestPos;
        pbScore.text = ScoreManager.Instance.personalBestScore;
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
        Tweener.Instance.ScaleTo(stripe, new Vector3(1f, 0f, 1f), 0.2f, 0.5f, TweenEasings.QuadraticEaseIn);
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

    public void GoOptions()
    {
        HideWindows();
        Invoke("DelayedGoOptions", 0.7f);
    }

    void DelayedGoOptions()
    {
        SceneManager.LoadSceneAsync("Options");
    }
}
