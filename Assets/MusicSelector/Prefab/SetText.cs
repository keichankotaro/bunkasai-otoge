using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;
using System.Collections;

public class SetText : MonoBehaviour
{
    public GameObject musicTitleText;
    public GameObject composerText;
    public GameObject bpmText;
    public GameObject JacketImage;
    public GameObject MaxScore;

    private string jacketCachePath;

    void Awake()
    {
        jacketCachePath = Path.Combine(Application.persistentDataPath, "JacketCache");
        if (!Directory.Exists(jacketCachePath))
        {
            Directory.CreateDirectory(jacketCachePath);
        }
    }

    void Start()
    {
        string chart_name = name;
        if (adata.musicsJson == null)
        {
            Debug.LogError("adata.musicsJson is not initialized!");
            adata.loaded++;
            return;
        }

        List<string> musics = (adata.musicsJson["charts"] as JArray).ToObject<List<string>>();
        int musicIndex = musics.IndexOf(chart_name);

        if (musicIndex == -1)
        {
            Debug.LogError($"Music '{chart_name}' not found in the list.");
            adata.loaded++;
            return;
        }

        string composer = adata.musicsJson["composers"][musicIndex].ToString();
        string bpm = "BPM: " + adata.musicsJson["bpms"][musicIndex];

        musicTitleText.GetComponent<TextMeshProUGUI>().text = chart_name;
        composerText.GetComponent<TextMeshProUGUI>().text = composer;
        bpmText.GetComponent<TextMeshProUGUI>().text = bpm;

        // Update High Score Text
        UpdateHighScoreText(chart_name);

        // Load Jacket Image
        StartCoroutine(LoadOrDownloadJacket(chart_name));
    }

    private void UpdateHighScoreText(string chart_name)
    {
        /*
        if (APIManager.Instance != null && APIManager.Instance.IsLoggedIn())
        {
            MaxScore.GetComponent<TextMeshProUGUI>().alpha = 1f;
            int highScore = APIManager.Instance.GetHighScore(chart_name);
            MaxScore.GetComponent<TextMeshProUGUI>().text = "Score: " + highScore.ToString("N0");
        }
        else
        {
            MaxScore.GetComponent<TextMeshProUGUI>().alpha = 0f;
        }
        */
        MaxScore.GetComponent<TextMeshProUGUI>().alpha = 0f;
    }

    private IEnumerator LoadOrDownloadJacket(string musicName)
    {
        // This part remains mostly the same, but we remove the adata.loaded++ from here
        // as the main loading process is now handled differently.
        string encodedMusicName = Uri.EscapeDataString(musicName);
        string imageUrl = $"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getJacket/index.cgi?chart={encodedMusicName}";
        string safeFileName = string.Join("_", musicName.Split(Path.GetInvalidFileNameChars()));
        string localImagePath = Path.Combine(jacketCachePath, safeFileName + ".jpg");

        if (File.Exists(localImagePath))
        {
            byte[] fileData = File.ReadAllBytes(localImagePath);
            Texture2D   texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(fileData);
            ApplyTexture(texture);
            adata.loaded++;
            yield break;
        }

        using (var www = new UnityEngine.Networking.UnityWebRequest(imageUrl, UnityEngine.Networking.UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                byte[] imageData = www.downloadHandler.data;
                File.WriteAllBytes(localImagePath, imageData);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(imageData);
                ApplyTexture(texture);
                adata.loaded++;
            }
            else
            {
                Debug.LogError($"[JacketCache] Failed to download jacket for {musicName}: {www.error}");
            }
        }
    }

    void ApplyTexture(Texture2D texture)
    {
        if (JacketImage == null) return;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();
        Sprite jacket = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        JacketImage.GetComponent<Image>().sprite = jacket;
    }
}