using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class SetText : MonoBehaviour
{
    public GameObject musicTitleText;
    public GameObject composerText;
    public GameObject bpmText;
    public GameObject JacketImage;
    public GameObject MaxScore;
    public GameObject Achievement;
    public GameObject Rank;

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
        UpdateHighScoreText();

        // Load Jacket Image
        StartCoroutine(LoadOrDownloadJacket(chart_name));
    }

    public void UpdateHighScoreText()
    {
        if (MaxScore == null) return;
        string chart_name = name;

        if (APIManager.Instance != null && APIManager.Instance.IsLoggedIn())
        {
            MaxScore.GetComponent<TextMeshProUGUI>().alpha = 1f;
            int highScore = APIManager.Instance.GetHighScore(chart_name);
            MaxScore.GetComponent<TextMeshProUGUI>().text = "Score: " + highScore.ToString();
            string rank = ((highScore > 990000) ? "SSS+" : (highScore > 980000) ? "SSS" : (highScore > 975000) ? "SS+" : (highScore > 950000) ? "SS" : (highScore > 925000) ? "S+" : (highScore > 900000) ? "S" : (highScore > 850000) ? "AAA" : (highScore > 800000) ? "AA" : (highScore > 750000) ? "A" : (highScore > 700000) ? "BBB" : (highScore > 650000) ? "BB" : (highScore > 600000) ? "B" : (highScore > 550000) ? "C" : "D");
            Rank.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255);
            if (rank == "D")
            {
                var c = new Color32(255, 255, 255, 255);
                Rank.GetComponent<TextMeshProUGUI>().enableVertexGradient = false;
                Rank.GetComponent<TextMeshProUGUI>().color = c;
            }
            else if (rank == "C")
            {
                var c = new Color32(0, 255, 0, 255);
                Rank.GetComponent<TextMeshProUGUI>().enableVertexGradient = false;
                Rank.GetComponent<TextMeshProUGUI>().color = c;
            }
            else if (rank == "B" || rank == "BB" || rank == "BBB")
            {
                var c = new Color32(0, 0, 255, 255);
                Rank.GetComponent<TextMeshProUGUI>().enableVertexGradient = false;
                Rank.GetComponent<TextMeshProUGUI>().color = c;
            }
            else if (rank == "A" || rank == "AA" || rank == "AAA")
            {
                var c_start = new Color32(255, 208, 0, 255);
                var c_end = new Color32(255, 254, 218, 255);
                var c = new VertexGradient(c_start, c_end, c_start, c_end);
                Rank.GetComponent<TextMeshProUGUI>().enableVertexGradient = true;
                Rank.GetComponent<TextMeshProUGUI>().colorGradient = c;
                Rank.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255);
            }
            else if (rank == "S" || rank == "S+" || rank == "SS" || rank == "SS+" || rank == "SSS" || rank == "SSS+")
            {
                var c_start = new Color32(255, 0, 250, 255);
                var c_end = new Color32(0, 238, 255, 255);
                var c = new VertexGradient(c_start, c_end, c_start, c_end);
                Rank.GetComponent<TextMeshProUGUI>().enableVertexGradient = true;
                Rank.GetComponent<TextMeshProUGUI>().colorGradient = c;
                Rank.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255);
            }
            Rank.GetComponent<TextMeshProUGUI>().text = rank;
            if (highScore == 0)
            {
                Rank.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 0);
                Rank.GetComponent<TextMeshProUGUI>().text = "";
            }
        }
        else
        {
            MaxScore.GetComponent<TextMeshProUGUI>().alpha = 0f;
        }
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
            // 処理落ちを防ぐため数フレームに分散して画像をロードする
            yield return null;
            byte[] fileData = File.ReadAllBytes(localImagePath);
            
            yield return null;
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
            }
            else
            {
                Debug.LogError($"[JacketCache] Failed to download jacket for {musicName}: {www.error}");
            }
            
            // 成功・失敗にかかわらずロード済みとしてカウントする（永遠にロードが終わらないバグを防止）
            adata.loaded++;
        }
    }

    void ApplyTexture(Texture2D texture)
    {
        if (JacketImage == null) return;
        texture.filterMode = FilterMode.Trilinear;
        texture.Apply();
        Sprite jacket = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        JacketImage.GetComponent<Image>().sprite = jacket;
    }
}