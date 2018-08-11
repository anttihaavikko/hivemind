using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Hivemind : MonoBehaviour {

    public Text textBox, commentTextBox, header, postTitle, commentTitle;
    public Text info, scoreText, addition;
    public Image image, fullImage;
    public GameObject videoImage;
    public VideoPlayer video;
    public RenderTexture videoTexture;
    public Transform retryButton;

    public RectTransform postWindow, commentWindow, voteWindow;
    public Transform alien, alienSpotLoading, alienSpotVoting;

    RedditComment currentComment;

    RedditConnector reddit;

    int postOffX = -1500;
    int postOnX = -299;

    int commentOffX = 1500;
    int commentOnX = 377;

    int voteOffX = 1500;
    int voteOnX = 528;

    int totalScore = 0;
    float shownScore = 0;

    float scoreChangeSpeed = 0.2f;

    Vector3 addSpot;
    bool adding;
    bool canVote = true;

    bool quitting = false;

	// Use this for initialization
	void Start () {
        textBox.text = "";

        reddit = new RedditConnector();

        postWindow.localPosition = GetWindowPosition(postWindow, postOffX);
        commentWindow.localPosition = GetWindowPosition(commentWindow, commentOffX);
        voteWindow.localPosition = GetWindowPosition(voteWindow, voteOffX);

        addSpot = addition.transform.localPosition;

        shownScore = totalScore = ScoreManager.Instance.GetValidatedScore();
        scoreText.text = totalScore.ToString();

        MoveAlienLoading();

        Invoke("LoadPost", 0.25f);
	}

    private void MoveAlienLoading()
    {
        Tweener.Instance.MoveTo(alien, alienSpotLoading.position, 1f, 0f, TweenEasings.QuarticEaseOut);
    }

    private void MoveAlienVoting()
    {
        Tweener.Instance.MoveTo(alien, alienSpotVoting.position, 1f, 0f, TweenEasings.QuarticEaseOut);
    }

    void ShowPostWindow()
    {
        Tweener.Instance.MoveLocalTo(postWindow, GetWindowPosition(postWindow, postOnX), 1f, 0f, TweenEasings.BounceEaseOut);
    }

    void ShowCommentWindow()
    {
        Tweener.Instance.MoveLocalTo(commentWindow, GetWindowPosition(commentWindow, commentOnX), 1f, 0f, TweenEasings.BounceEaseOut);
    }

    void ShowVoteWindow()
    {
        canVote = true;
        Tweener.Instance.MoveLocalTo(voteWindow, GetWindowPosition(voteWindow, voteOnX), 1f, 0f, TweenEasings.BounceEaseOut);
    }

    void HidePostWindow()
    {
        Tweener.Instance.MoveLocalTo(postWindow, GetWindowPosition(postWindow, postOffX), 0.5f, 0f, TweenEasings.QuarticEaseOut);
    }

    void HideCommentWindow()
    {
        Tweener.Instance.MoveLocalTo(commentWindow, GetWindowPosition(commentWindow, commentOffX), 0.5f, 0f, TweenEasings.QuarticEaseOut);
    }

    void HideVoteWindow()
    {
        Tweener.Instance.MoveLocalTo(voteWindow, GetWindowPosition(voteWindow, voteOffX), 0.5f, 0f, TweenEasings.QuarticEaseOut);
    }

    void HideWindows()
    {
        HidePostWindow();
        HideCommentWindow();
        HideVoteWindow();
    }

    Vector3 GetWindowPosition(RectTransform t, float x)
    {
        return new Vector3(x, t.localPosition.y, t.localPosition.z);
    }
	
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
        {
            StartPostLoading();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            video.Play();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            File.WriteAllText(Application.dataPath + "/dump_posts.json", reddit.postsJSON);
            File.WriteAllText(Application.dataPath + "/dump_comments.json", reddit.commentsJSON);
            Debug.Log("Post and comments JSON files dumped.");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Application.OpenURL("https://reddit.com" + reddit.CurrentPost.permalink);
        }

        if(adding)
        {
            shownScore = Mathf.MoveTowards(shownScore, totalScore, Mathf.Max(scoreChangeSpeed, 0.2f));
            scoreText.text = Mathf.RoundToInt(shownScore).ToString();
        }

        if(!quitting && Input.GetKeyUp(KeyCode.Escape))
        {
            quitting = true;
            SceneManager.LoadSceneAsync("Start");
        }
    }

    void StartPostLoading()
    {
        //HideWindows();
        MoveAlienLoading();
        Invoke("LoadPost", 0.5f);
    }

    void ShowInfo(string message, int fontSize = 50)
    {
        info.text = "<size=" + fontSize + ">" + message + "</size>";
        Tweener.Instance.ScaleTo(info.transform, Vector3.one, 0.25f, 0, TweenEasings.BounceEaseOut);
    }

    void HideInfo()
    {
        Tweener.Instance.ScaleTo(info.transform, Vector3.zero, 0.25f, 0, TweenEasings.CubicEaseInOut);
    }

    void LoadPost()
    {
        ShowInfo("\n\n Loading...");
        textBox.text = "";
        commentTextBox.text = "";
        image.sprite = null;
        fullImage.sprite = null;
        //video.Stop();
        ClearVideoTexture();
        videoImage.SetActive(false);
        StartCoroutine(reddit.LoadPost(ShowPost));
    }

    void ClearVideoTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = videoTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }

    void ShowPost()
    {
        if(reddit.CurrentPost.num_comments < 3)
        {
            Debug.Log("Not enough comments...");
            LoadPost();
            return;
        }

        HideInfo();

        if (reddit.errorMessage != null)
        {
            ShowInfo("\n\n" + reddit.errorMessage, 40);
            Tweener.Instance.ScaleTo(retryButton, Vector3.one, 0.3f, 0, TweenEasings.BounceEaseOut);
            return;
        }

        var post = reddit.CurrentPost;

        var str = "";

        //str += post.subreddit_name_prefixed + "\n";
        //str += post.title + "\n";
        //str += "by " + post.author + "\n";
        //str += post.num_comments + " comments\n";
        //str += post.score + "\n\n";
        str += post.selftext;
        str += "\n";

        postTitle.text = ParseHtml(post.title);
        header.text = post.subreddit_name_prefixed + "\nu/" + post.author;

        //Debug.Log(post.preview.reddit_video_preview.fallback_url);

        var textOnlyPost = post.selftext.Length > 5;

        if (!string.IsNullOrEmpty(post.thumbnail) && post.thumbnail != "self" && post.thumbnail != "spoiler" && post.thumbnail != "default")
        {
            //StartCoroutine(reddit.ShowThumbIn(image));
            StartCoroutine(reddit.ShowImageIn(fullImage, true));
        }
        else
        {
            textOnlyPost = true;
        }

        fullImage.gameObject.SetActive(!textOnlyPost);

        if (post.preview != null && post.preview.reddit_video_preview != null && !string.IsNullOrEmpty(post.preview.reddit_video_preview.fallback_url))
        {
            video.url = post.preview.reddit_video_preview.fallback_url;
            video.Play();
            videoImage.SetActive(true);
        }

        textBox.text = ParseHtml(str);

        //reddit.CurrentPostComments.ForEach(commenent => commentTextBox.text += commenent.score + " : " + commenent.author + " : " + commenent.body + "\n");

        if(reddit.CurrentPostComments.Count <= 3)
        {
            LoadPost();
            return;
        }

        Invoke("MoveAlienVoting", 0.5f);

        PickComment();

        currentComment = reddit.CurrentPostComments[0];
        commentTitle.text = currentComment.author;
        commentTextBox.text = ParseHtml(currentComment.body);

        Invoke("ShowPostWindow", 0.25f);
        Invoke("ShowCommentWindow", 0.75f);
        Invoke("ShowVoteWindow", 1.25f);
    }

    private void PickComment()
    {
        int targetDirection = Random.value < 0.35f ? 1 : -1;
        currentComment = reddit.CurrentPostComments.Find(c => (int)Mathf.Sign(c.score) == targetDirection);
        var str = "Trying to get post with " + targetDirection + " karma...";

        if(currentComment == null) {
            str += " Not found, getting random one";
            currentComment = reddit.CurrentPostComments[Random.Range(0, 3)];
        } else {
            str += "Success!";
        }

        Debug.Log(str);
    }

    public void Vote(int score)
    {
        if (!canVote)
            return;

        canVote = false;
        adding = false;

        var success = (score < 0 && currentComment.score < 0 || score > 0 && currentComment.score > 0);
        var dir = success ? 1 : -1;
        var amt = Mathf.Abs(currentComment.score);
        var sign = success ? "+" : "-";
        totalScore += amt * dir;
        addition.text = sign + amt.ToString();

        scoreChangeSpeed = amt * Time.deltaTime;

        addition.transform.localPosition = addSpot;

        ScoreManager.Instance.SubmitScore(PlayerPrefs.GetString("PlayerName"), totalScore);

        Tweener.Instance.ScaleTo(addition.transform, Vector3.one, 0.33f, 0f, TweenEasings.BounceEaseOut);
        Invoke("MoveAddition", 0.75f);

        Invoke("HideVoteWindow", 0.5f);
        Invoke("HideCommentWindow", 0.75f);
        Invoke("HidePostWindow", 1f);
        Invoke("StartPostLoading", 1.25f);
    }

    string ParseHtml(string text)
    {
        text = text.Replace("&amp;", "&");
        text = text.Replace("&gt;", ">");
        text = text.Replace("&lt;", "<");
        return text;
    }

    void MoveAddition()
    {
        Tweener.Instance.MoveLocalTo(addition.transform, scoreText.transform.localPosition, 0.5f, 0, TweenEasings.CubicEaseIn);
        Tweener.Instance.ScaleTo(addition.transform, Vector3.zero, 0.5f, 0, TweenEasings.CubicEaseIn);
        Invoke("EnableAddition", 0.3f);
    }

    void EnableAddition()
    {
        adding = true;
    }

    public void Retry()
    {
        HideInfo();
        Tweener.Instance.ScaleTo(retryButton, Vector3.zero, 0.25f, 0, TweenEasings.CubicEaseIn);
        Invoke("LoadPost", 0.5f);
    }
}
