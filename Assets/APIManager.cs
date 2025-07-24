using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance { get; private set; }

    [Header("Debug Settings")]
    public bool useDebugToken = false;
    [TextArea]
    public string debugBearerToken = "";

    private const string BaseUrl = "https://keichankotaro.com/%e6%96%87%e5%8c%96%e7%a5%ad%e9%9f%b3%e3%82%b2%e3%83%bc/userauth/"; // TODO: Replace with your actual domain
    private static string _bearerToken;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (useDebugToken)
            {
                _bearerToken = debugBearerToken;
                Debug.Log("Using debug Bearer token.");
                Debug.LogError("This is not Error! Enable Login Mode.");
            }
            else
            {
                GetCommandLineArgs();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void GetCommandLineArgs()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--token" && i + 1 < args.Length)
                {
                    _bearerToken = args[i + 1];
                    Debug.Log("Bearer token successfully loaded from command line.");
                    Debug.LogError("This is not Error! Enable Login Mode.");
                    break;
                }
            }

            if (string.IsNullOrEmpty(_bearerToken))
            {
                Debug.LogWarning("Bearer token not found in command line arguments.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting command line arguments: " + e.Message);
        }
    }

    public bool IsLoggedIn()
    {
        if (adata.isOfflineMode)
        {
            return false;
        }
        else
        {
            return !string.IsNullOrEmpty(_bearerToken);
        }
    }

    private Dictionary<string, int> _highScores = new Dictionary<string, int>();
    private bool _highScoresFetched = false;

    public int GetHighScore(string songTitle)
    {
        _highScores.TryGetValue(songTitle, out int score);
        return score;
    }

    public void InvalidateHighScores()
    {
        _highScoresFetched = false;
        _highScores.Clear();
        Debug.Log("High score cache invalidated.");
    }

    public IEnumerator FetchHighScores(Action onComplete = null)
    {
        if (!IsLoggedIn() || _highScoresFetched)
        {
            onComplete?.Invoke();
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(BaseUrl + "get_my_high_scores.cgi"))
        {
            www.timeout = 30; // Set timeout to 30 seconds
            www.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    JObject response = JObject.Parse(www.downloadHandler.text);
                    if (response["status"].ToString() == "success")
                    {
                        JArray scores = (JArray)response["data"];
                        foreach (JObject scoreData in scores)
                        {
                            string songTitle = scoreData["song_title"].ToString();
                            int highScore = scoreData["high_score"].ToObject<int>();

                            if (_highScores.ContainsKey(songTitle))
                            {
                                if (highScore > _highScores[songTitle])
                                {
                                    _highScores[songTitle] = highScore;
                                }
                            }
                            else
                            {
                                _highScores.Add(songTitle, highScore);
                            }
                        }
                        _highScoresFetched = true;
                        Debug.Log("High scores fetched successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to fetch high scores: " + response["message"]);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing high scores response: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Error fetching high scores: " + www.error);
            }
        }
        onComplete?.Invoke();
    }

    public IEnumerator UploadResult(string songTitle, string difficulty, int score)
    {
        if (!IsLoggedIn())
        {
            Debug.LogWarning("Cannot upload result. User is not logged in.");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("song_title", songTitle);
        form.AddField("difficulty", difficulty);
        form.AddField("score", score);

        using (UnityWebRequest www = UnityWebRequest.Post(BaseUrl + "upload_result.cgi", form))
        {
            www.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error uploading result: " + www.error);
                Debug.LogError("Response: " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("Result uploaded successfully!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
}