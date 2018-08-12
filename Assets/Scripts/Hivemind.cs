using System.Collections.Generic;
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

    public EffectCamera cam;
    public SpeechBubble speechBubble;
    public Animator alientAnimation;

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

    List<string> votedPosts;

    bool doTutorial = true;
    int tutorialStep = 0;

	// Use this for initialization
	void Start () {

        votedPosts = new List<string>();

        textBox.text = "";

        reddit = new RedditConnector();

        postWindow.localPosition = GetWindowPosition(postWindow, postOffX);
        commentWindow.localPosition = GetWindowPosition(commentWindow, commentOffX);
        voteWindow.localPosition = GetWindowPosition(voteWindow, voteOffX);

        addSpot = addition.transform.localPosition;

        shownScore = totalScore = ScoreManager.Instance.GetValidatedScore();
        scoreText.text = totalScore.ToString();

        MoveAlienLoading();

        LoadVotedPosts();

        Invoke("LoadPost", 0.25f);

        doTutorial = !PlayerPrefs.HasKey("Tutorial");

        //if (Application.isEditor) doTutorial = true;
	}

    private void MoveAlienLoading()
    {
        AudioManager.Instance.PlayEffectAt(9, alien.position, 1f);
        AudioManager.Instance.PlayEffectAt(10, alien.position, 1f);
        Tweener.Instance.MoveTo(alien, alienSpotLoading.position, 1f, 0f, TweenEasings.QuarticEaseOut);
    }

    private void MoveAlienVoting()
    {
        AudioManager.Instance.PlayEffectAt(9, alien.position, 1f);
        AudioManager.Instance.PlayEffectAt(10, alien.position, 1f);
        Tweener.Instance.MoveTo(alien, alienSpotVoting.position, 1f, 0f, TweenEasings.QuarticEaseOut);
    }

    private void MoveAlienTo(Vector3 pos)
    {
        AudioManager.Instance.PlayEffectAt(9, alien.position, 1f);
        AudioManager.Instance.PlayEffectAt(10, alien.position, 1f);
        Tweener.Instance.MoveTo(alien, pos, 1f, 0f, TweenEasings.QuarticEaseOut);
    }

    void ShowPostWindow()
    {
        Tweener.Instance.MoveLocalTo(postWindow, GetWindowPosition(postWindow, postOnX), 1f, 0f, TweenEasings.BounceEaseOut);
        AudioManager.Instance.PlayEffectAt(11, new Vector3(-7f, 2f, 0f), 0.3f);
        StartCoroutine(DoWindowBounceSound(12, 0.3f, new Vector3(-7f, 2f, 0f), 0.7f));
    }

    void ShowCommentWindow()
    {
        Tweener.Instance.MoveLocalTo(commentWindow, GetWindowPosition(commentWindow, commentOnX), 1f, 0f, TweenEasings.BounceEaseOut);
        AudioManager.Instance.PlayEffectAt(11, new Vector3(7f, -3.5f, 0f), 0.2f);
        StartCoroutine(DoWindowBounceSound(12, 0.3f, new Vector3(7f, -3.5f), 0.6f));
    }

    void ShowVoteWindow()
    {
        canVote = true;
        Tweener.Instance.MoveLocalTo(voteWindow, GetWindowPosition(voteWindow, voteOnX), 1f, 0f, TweenEasings.BounceEaseOut);
        AudioManager.Instance.PlayEffectAt(11, new Vector3(7f, 0, 0f), 0.1f);
        StartCoroutine(DoWindowBounceSound(12, 0.3f, new Vector3(7f, 0, 0f), 0.5f));
    }

    System.Collections.IEnumerator DoWindowBounceSound(int soundIndex, float delayTime, Vector3 pos, float volume)
    {
        yield return new WaitForSeconds(delayTime);
        AudioManager.Instance.PlayEffectAt(soundIndex, pos, volume);
        yield return new WaitForSeconds(0.15f);
        AudioManager.Instance.PlayEffectAt(soundIndex, pos, volume - 0.1f);
    }

    void HidePostWindow()
    {
        Tweener.Instance.MoveLocalTo(postWindow, GetWindowPosition(postWindow, postOffX), 0.5f, 0f, TweenEasings.QuarticEaseOut);
        AudioManager.Instance.PlayEffectAt(11, new Vector3(-7f, 2f, 0f), 0.3f);
    }

    void HideCommentWindow()
    {
        Tweener.Instance.MoveLocalTo(commentWindow, GetWindowPosition(commentWindow, commentOffX), 0.5f, 0f, TweenEasings.QuarticEaseOut);
        AudioManager.Instance.PlayEffectAt(11, new Vector3(7f, -3.5f, 0f), 0.2f);
    }

    void HideVoteWindow()
    {
        Tweener.Instance.MoveLocalTo(voteWindow, GetWindowPosition(voteWindow, voteOffX), 0.5f, 0f, TweenEasings.QuarticEaseOut);
        AudioManager.Instance.PlayEffectAt(11, new Vector3(7f, 0, 0f), 0.1f);
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
        if(Application.isEditor) 
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
        }

        if(Input.anyKeyDown && doTutorial)
        {
            tutorialStep++;
        }

        if(adding)
        {
            shownScore = Mathf.MoveTowards(shownScore, totalScore, Mathf.Max(scoreChangeSpeed, 0.2f));
            scoreText.text = Mathf.RoundToInt(shownScore).ToString();
        }

        if(!quitting && Input.GetKeyUp(KeyCode.Escape))
        {
            quitting = true;
            HideWindows();
            MoveAlienTo(alienSpotVoting.position + Vector3.right * 10f);
            Invoke("GoToStart", 1f);
        }
    }

    void GoToStart()
    {
        SceneManager.LoadSceneAsync("Start");
    }

    void StartPostLoading()
    {
        //HideWindows();
        AudioManager.Instance.Lowpass(false);
        MoveAlienLoading();
        Invoke("LoadPost", 0.5f);
    }

    void ShowInfo(string message, int fontSize = 50)
    {
        float vol = info.transform.localScale.x < 0.7f ? 0.5f : 0.25f;

        var newtext = "<size=" + fontSize + ">" + message + "</size>";
        if (newtext == info.text) vol = 0f;
        info.text = newtext;

        Tweener.Instance.ScaleTo(info.transform, Vector3.one, 0.25f, 0, TweenEasings.BounceEaseOut);

        AudioManager.Instance.PlayEffectAt(8, Vector3.zero, vol);
    }

    void HideInfo()
    {
        Tweener.Instance.ScaleTo(info.transform, Vector3.zero, 0.25f, 0, TweenEasings.CubicEaseInOut);
    }

    void LoadPost()
    {
        LoadPost("Loading...");
    }

    void LoadPost(string loadingMessage)
    {
        ShowInfo("\n\n  " + loadingMessage);
        textBox.text = "";
        commentTextBox.text = "";
        image.sprite = null;
        fullImage.sprite = null;
        video.Stop();
        ClearVideoTexture();
        videoImage.SetActive(false);
        StartCoroutine(reddit.LoadPost(votedPosts, ShowPost));
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
            LoadPost("Digging deep...");
            return;
        }

        if(votedPosts.Contains(reddit.CurrentPost.id))
        {
            return;
        }

        speechBubble.Hide();

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

        commentTitle.text = currentComment.author;
        commentTextBox.text = ParseHtml(currentComment.body);

        Invoke("ShowPostWindow", 0.25f);

        if (doTutorial)
        {
            StartCoroutine(DoTutorial());
        }
        else
        {
            Invoke("ShowCommentWindow", 0.75f);
            Invoke("ShowVoteWindow", 1.25f);
        }
    }

    System.Collections.IEnumerator DoTutorial()
    {
        yield return new WaitForSeconds(1.75f);
        MoveAlienTo(alienSpotVoting.position + Vector3.down * 1f + Vector3.left * 2f);
        yield return new WaitForSeconds(0.25f);
        alientAnimation.SetBool("pointLeft", true);
        speechBubble.ShowMessage("This is a (post) fetched straight from (Reddit).");
        while (tutorialStep <= 0) yield return null;
        alientAnimation.SetBool("pointLeft", false);
        speechBubble.Hide();
        ShowCommentWindow();
        yield return new WaitForSeconds(0.5f);
        MoveAlienTo(alienSpotVoting.position + Vector3.down * 2.5f);
        yield return new WaitForSeconds(0.5f);
        alientAnimation.SetBool("pointDown", true);
        speechBubble.ShowMessage("This is the (comment) you'll be (guessing) on.");
        while (tutorialStep <= 1) yield return null;
        speechBubble.ShowMessage("You need to guess (wether) the shown (comment)...");
        while (tutorialStep <= 2) yield return null;
        speechBubble.ShowMessage("...has (positive) or (negative) karma.");
        while (tutorialStep <= 3) yield return null;
        alientAnimation.SetBool("pointDown", false);
        speechBubble.ShowMessage("You gain (more points) the (more karma) the comment has.");
        while (tutorialStep <= 4) yield return null;
        speechBubble.ShowMessage("But if you're (wrong), you (lose) the same amount.");
        while (tutorialStep <= 5) yield return null;
        speechBubble.ShowMessage("How well do you know your (Redditors)?");
        while (tutorialStep <= 6) yield return null;
        speechBubble.Hide();
        MoveAlienTo(alienSpotVoting.position + Vector3.down * 1f);
        ShowVoteWindow();
        yield return new WaitForSeconds(1f);
        alientAnimation.SetBool("pointDown", true);
        speechBubble.ShowMessage("And these are your (options)...");
        while (tutorialStep <= 7) yield return null;
        alientAnimation.SetBool("pointDown", false);
        speechBubble.ShowMessage("Lets give it a (go)!");
        while (tutorialStep <= 8) yield return null;
        speechBubble.Hide();
        MoveAlienTo(alienSpotVoting.position);

        doTutorial = false;
        PlayerPrefs.SetString("Tutorial", "Done");
    }

    private void PickComment()
    {
        int targetDirection = Random.value < 0.3f ? 1 : -1;
        currentComment = reddit.CurrentPostComments.Find(c => (int)Mathf.Sign(c.score) == targetDirection);
        var str = "Trying to get post with " + targetDirection + " karma...";

        if(currentComment == null) {
            str += " Not found, getting random one";
            currentComment = reddit.CurrentPostComments[Random.Range(0, 3)];
        } else {
            str += "Success!";
        }

        reddit.CurrentPostComments.Remove(currentComment);

        Invoke("AlienComment", Random.Range(5f, 20f));

        Debug.Log(str);
    }

    void AlienComment()
    {
        var forAlien = reddit.CurrentPostComments.Find(c => c.score > 0 && c.body.Length < 50);

        if (forAlien != null)
            speechBubble.ShowMessage(forAlien.body);
    }

    public void Vote(int score)
    {
        if (!canVote)
            return;

        canVote = false;
        adding = false;

        var success = (score < 0 && currentComment.score < 0 || score > 0 && currentComment.score > 2);
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

        votedPosts.Add(reddit.CurrentPost.id);

        if (votedPosts.Count > 500)
            votedPosts.RemoveAt(0);

        CancelInvoke("AlienComment");

        if(success)
        {
            AudioManager.Instance.PlayEffectAt(0, Vector3.zero, 1f);

            Invoke("Compliment", 0.25f);
        }
        else
        {
            AudioManager.Instance.PlayEffectAt(1, Vector3.zero, 1f);
            AudioManager.Instance.PlayEffectAt(2, Vector3.zero, 1f);

            Invoke("Bark", 0.25f);

            cam.BaseEffect(2f);

            AudioManager.Instance.Lowpass(true);
            AudioManager.Instance.targetPitch = 0.6f;
        }

        PlayerPrefs.SetString("VotedPosts", System.String.Join(",", votedPosts.ToArray()));
    }

    void LoadVotedPosts()
    {
        if(PlayerPrefs.HasKey("VotedPosts")) {
            var str = PlayerPrefs.GetString("VotedPosts");
            votedPosts.AddRange(str.Split(','));
            Debug.Log("PlayerPrefs had " + votedPosts.Count + " voted posts...");
        }
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
        AudioManager.Instance.targetPitch = 1f;

        AudioManager.Instance.PlayEffectAt(4, addition.transform.position, 1f);

        Tweener.Instance.MoveLocalTo(addition.transform, scoreText.transform.localPosition, 0.5f, 0, TweenEasings.CubicEaseIn);
        Tweener.Instance.ScaleTo(addition.transform, Vector3.zero, 0.5f, 0, TweenEasings.CubicEaseIn);
        Invoke("EnableAddition", 0.3f);
    }

    void EnableAddition()
    {
        DoAdditionSound();
        if (totalScore - shownScore > 10) Invoke("DoAdditionSound", 0.15f);
        if (totalScore - shownScore > 50) Invoke("DoAdditionSound", 0.3f);
        adding = true;
    }

    void DoAdditionSound()
    {
        AudioManager.Instance.PlayEffectAt(7, scoreText.transform.position, 1.3f);
    }

    public void Retry()
    {
        HideInfo();
        Tweener.Instance.ScaleTo(retryButton, Vector3.zero, 0.25f, 0, TweenEasings.CubicEaseIn);
        Invoke("LoadPost", 0.5f);
    }

    void Compliment()
    {
        string[] comms = {
            "Great!",
            "Nice!",
            "Well done!",
            "Good job!",
            "Awesome!",
            "Good guess!",
            "Noice!",
            "Whee...",
            "Nicely done!",
            "Success!"
        };

        speechBubble.ShowMessage(comms[Random.Range(0, comms.Length)]);
    }

    void Bark()
    {
        string[] comms = {
            "Eww!",
            "Too bad!",
            "Better luck next time!",
            "Ouch!",
            "Dang!",
            "Not even close!",
            "Nope!",
            "No way!"
        };

        speechBubble.ShowMessage(comms[Random.Range(0, comms.Length)]);
    }
}
