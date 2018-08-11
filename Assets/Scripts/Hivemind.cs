using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Hivemind : MonoBehaviour {

    public Text textBox, commentTextBox, header, postTitle, commentTitle;
    public Image image, fullImage;
    public GameObject videoImage;
    public VideoPlayer video;
    public RenderTexture videoTexture;

    private RedditConnector reddit;

	// Use this for initialization
	void Start () {
        textBox.text = "";

        reddit = new RedditConnector();

        LoadPost();
	}
	
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
        {
            LoadPost();
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

    void LoadPost()
    {
        textBox.text = "Loading...";
        commentTextBox.text = "";
        image.sprite = null;
        fullImage.sprite = null;
        video.Stop();
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

        if (reddit.errorMessage != null)
        {
            textBox.text = reddit.errorMessage;
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

        postTitle.text = post.title;
        header.text = post.subreddit_name_prefixed + "\nu/" + post.author;

        //Debug.Log(post.preview.reddit_video_preview.fallback_url);

        var textOnlyPost = post.selftext.Length > 5;

        if (!string.IsNullOrEmpty(post.thumbnail) && post.thumbnail != "self" && post.thumbnail != "spoiler" && post.thumbnail != "default")
        {
            StartCoroutine(reddit.ShowThumbIn(image));
            StartCoroutine(reddit.ShowImageIn(fullImage, true));
        }
        else
        {
            textOnlyPost = true;
        }

        fullImage.gameObject.SetActive(!textOnlyPost);
        image.gameObject.SetActive(textOnlyPost);

        if (post.preview != null && post.preview.reddit_video_preview != null && !string.IsNullOrEmpty(post.preview.reddit_video_preview.fallback_url))
        {
            video.url = post.preview.reddit_video_preview.fallback_url;
            video.Play();
            videoImage.SetActive(true);
        }

        textBox.text = str;

        //reddit.CurrentPostComments.ForEach(commenent => commentTextBox.text += commenent.score + " : " + commenent.author + " : " + commenent.body + "\n");

        var topComment = reddit.CurrentPostComments[0];
        commentTitle.text = topComment.author + " (" + topComment.score + ")";
        commentTextBox.text = topComment.body;
    }
}
