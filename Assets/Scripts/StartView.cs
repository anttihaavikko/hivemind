using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartView : MonoBehaviour {

    public Text positions, scores;
    int page = 0;

	// Use this for initialization
	void Start () {
        positions.text = "";
        scores.text = "";
        ScoreManager.Instance.LoadLeaderBoards(0);
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
        SceneManager.LoadSceneAsync("Main");
    }

    public void QuitGame()
    {
        Debug.Log("Quit...");
        Application.Quit();
    }
}
