using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RedditConnector {

    public static string userAgent = Application.platform + ":Hivemind:v1.0 (by /u/anttihaavikko)";

    private string token = null;
    private DateTime tokenCreationTime;
    private string postFilters = "hot";
    private string deviceId;

    private List<string> seenPosts;

    public string errorMessage = null;

    public string postsJSON = null;
    public string commentsJSON = null;

    public RedditPost CurrentPost { get; internal set; }
    public List<RedditComment> CurrentPostComments { get; internal set; }
    public List<RedditPost> AllLoadedPosts { get; internal set; }

    bool customUserAgent = true;

    public RedditConnector()
    {
        CurrentPostComments = new List<RedditComment>();
        tokenCreationTime = DateTime.Now;
        AllLoadedPosts = new List<RedditPost>();

        seenPosts = new List<string>();

        deviceId = Application.platform == RuntimePlatform.WebGLPlayer ? System.Guid.NewGuid().ToString() : SystemInfo.deviceUniqueIdentifier;
    }

    private IEnumerator GetAccessToken()
    {
        var tokenAge = (DateTime.Now - tokenCreationTime).TotalMinutes;

        if (token != null && tokenAge <= 55) {
            //Debug.Log("Using same old token.");
            yield break;
        }

        tokenCreationTime = DateTime.Now;

        Dictionary<string, string> content = new Dictionary<string, string>();
        //Fill key and value
        content.Add("grant_type", "https://oauth.reddit.com/grants/installed_client");
        content.Add("device_id", deviceId);

        UnityWebRequest www = UnityWebRequest.Post("https://www.reddit.com/api/v1/access_token", content);

        if (customUserAgent && Application.platform != RuntimePlatform.WebGLPlayer)
            www.SetRequestHeader("User-Agent", userAgent);

        string authorization = BasicAuth(Secrets.redditApiKey, "");
        www.SetRequestHeader("Authorization", authorization);

        //Send request
        yield return www.Send();

        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            OAuthToken json = JsonUtility.FromJson<OAuthToken>(resultContent);

            //Debug.Log(resultContent);

            token = json.access_token;

            if(json.message != null)
                errorMessage = "Authentication error: " + json.message;

            //Debug.Log("Got new token.");
        }
        else
        {
            errorMessage = www.error;
        }
    }

    private static string BasicAuth(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    } 

    public IEnumerator LoadPost(Action showPost)
    {
        yield return GetAccessToken();

        if(token == null) {
            showPost();
            yield break;
        }

        if (AllLoadedPosts.Count > 0) {
            PickNewPost();
            yield return LoadComments();
            showPost();
            yield break;
        }

        AllLoadedPosts.Clear();

        UnityWebRequest www = UnityWebRequest.Get("https://oauth.reddit.com/" + postFilters + "/.json?limit=100");

        www.SetRequestHeader("Authorization", "Bearer " + token);

        if (customUserAgent && Application.platform != RuntimePlatform.WebGLPlayer)
            www.SetRequestHeader("User-Agent", userAgent);

        //Debug.Log("Loading posts with filter '" + postFilters + "'");

        yield return www.Send();

        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            postsJSON = resultContent;

            //Debug.Log(resultContent);
            PostWrapper json = JsonUtility.FromJson<PostWrapper>(resultContent);

            if (json.message != null) {
                errorMessage = "Error: " + json.message;
                showPost();
                yield break;
            }

            foreach(var p in json.data.children) {
                
                if (!seenPosts.Contains(p.data.id))
                {
                    AllLoadedPosts.Add(p.data);
                }
            }

            //Debug.Log("Loaded " + AllLoadedPosts.Count + " posts.");

            //Debug.Log("--------");
            //Debug.Log(post.score + " : " + post.author + " : " + post.title);
            //Debug.Log("--------");

            if(AllLoadedPosts.Count == 0) {
                postFilters = "r/random";
                yield return LoadPost(showPost);
                yield break;
            }

            PickNewPost();

            yield return LoadComments();

            showPost();
        }
        else
        {
            errorMessage = "Network error: " + www.error;
        }
    }

    private void PickNewPost()
    {
        CurrentPost = AllLoadedPosts[UnityEngine.Random.Range(0, AllLoadedPosts.Count)];
        seenPosts.Add(CurrentPost.id);
        AllLoadedPosts.Remove(CurrentPost);
        //Debug.Log("Picked post from cache, " + AllLoadedPosts.Count + " left.");

        if (postFilters == "r/random")
            AllLoadedPosts.Clear();

        if(AllLoadedPosts.Count == 0) {
            postFilters = "r/random";
        }
    }

    public IEnumerator LoadComments() {

        yield return GetAccessToken();

        CurrentPostComments.Clear();

        UnityWebRequest www = UnityWebRequest.Get("https://oauth.reddit.com" + CurrentPost.permalink + "/.json?sort=controversial");
        www.SetRequestHeader("Authorization", "Bearer " + token);

        if(customUserAgent && Application.platform != RuntimePlatform.WebGLPlayer)
            www.SetRequestHeader("User-Agent", userAgent);

        yield return www.Send();

        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            commentsJSON = resultContent;

            if(!www.isHttpError)
                resultContent = "{\"comments\":" + www.downloadHandler.text + "}";

            //Debug.Log(resultContent);
            CommentsWrapper json = JsonUtility.FromJson<CommentsWrapper>(resultContent);

            if (json.message != null)
            {
                errorMessage = "Error: " + json.message;
                yield break;
            }

            foreach(var c in json.comments) {
                foreach (var ch in c.data.children)
                {
                    var comment = ch.data;

                    if(comment.score != 0 
                       && !string.IsNullOrEmpty(comment.author) 
                       && !string.IsNullOrEmpty(comment.body) 
                       && comment.body != "[deleted]"
                       && comment.body != "[removed]")
                    {
                        CurrentPostComments.Add(comment);
                        //Debug.Log(ch.data.score + " : " + ch.data.author + " : " + ch.data.body);
                    }
                }
            }
        }
        else
        {
            errorMessage = "Network error: " + www.error;
        }
    }

    public IEnumerator ShowThumbIn(Image image, bool setNativeSize = true, Action after = null)
    {
        Texture2D tex = new Texture2D(100, 100, TextureFormat.DXT1, false);

        using (WWW www = new WWW(CurrentPost.thumbnail))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);

            Rect rec = new Rect(0, 0, tex.width, tex.height);
            image.sprite = Sprite.Create(tex, rec, Vector2.zero, 100);

            if(setNativeSize)
                image.SetNativeSize();
        }

        if(after != null) {
            after();
        }
    }

    public IEnumerator ShowImageIn(Image image, bool setNativeSize = true, Action after = null)
    {
        var pic = CurrentPost.preview.images[0].source;
        Texture2D tex = new Texture2D(pic.width, pic.height, TextureFormat.DXT1, false);

        var horizontal = pic.width >= pic.height;
        float ratio = (float)pic.height / (float)pic.width;

        using (WWW www = new WWW(pic.url))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);

            Rect rec = new Rect(0, 0, tex.width, tex.height);
            image.sprite = Sprite.Create(tex, rec, Vector2.zero, 100);

            int maxSize = (ratio < 0.75 || ratio > 1.25) ? 600 : 450;

            image.rectTransform.sizeDelta = horizontal ?
                new Vector2(maxSize, maxSize * ratio) :
                new Vector2(maxSize / ratio, maxSize);
        }

        if (after != null)
        {
            after();
        }
    }
}

[Serializable]
public class OAuthToken
{
    public string access_token;
    public string message;
}

[Serializable]
public class PostWrapper
{
    public PostDatas data;
    public string message;
}

[Serializable]
public class PostDatas
{
    public PostData[] children;
}

[Serializable]
public class PostData
{
    public string kind;
    public RedditPost data;
}

[Serializable]
public class RedditPost
{
    public string subreddit;
    public string subreddit_name_prefixed;
    public string name;
    public string title;
    public string selftext;
    public string selftext_html;
    public int downs;
    public int likes;
    public string score;
    public string author;
    public int num_comments;
    public string permalink;
    public string created_utc;
    public bool is_video;
    public string id;
    public string thumbnail;
    public string url;
    public PostImages preview;
}

[Serializable]
public class PostImages
{
    public PostImage[] images;
    public PostVideo reddit_video_preview;
}

[Serializable]
public class PostImage
{
    public PostImageDetails source;
}

[Serializable]
public class PostImageDetails
{
    public string url;
    public int width;
    public int height;
}

[Serializable]
public class PostVideo
{
    public string fallback_url;
    public int width;
    public int height;
    public bool is_gif;
}

[Serializable]
public class CommentsWrapper
{
    public CommentThingWrapper[] comments;
    public string message;
}

[Serializable]
public class CommentThingWrapper
{
    public string kind;
    public CommentsData data;
}

[Serializable]
public class CommentsData
{
    public CommentWrapper[] children;
}

[Serializable]
public class CommentWrapper
{
    public string kind;
    public RedditComment data;
}

[Serializable]
public class RedditComment
{
    public string author;
    public string body;
    public int score;
    public string id;
}

