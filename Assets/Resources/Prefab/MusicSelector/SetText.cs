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
        try
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

            if (musicTitleText != null) musicTitleText.GetComponent<TextMeshProUGUI>().text = chart_name;
            if (composerText != null) composerText.GetComponent<TextMeshProUGUI>().text = composer;
            if (bpmText != null) bpmText.GetComponent<TextMeshProUGUI>().text = bpm;

            // Update High Score Text
            UpdateHighScoreText();

            // Load Jacket Image
            StartCoroutine(LoadOrDownloadJacket(chart_name));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SetText] Error in Start for {name}: {e.Message}\n{e.StackTrace}");
            // エラー時も無限ロードを回避するためにカウントを進める
            adata.loaded++;
        }
    }

    public void UpdateHighScoreText()
    {
        try
        {
            if (MaxScore == null) return;
            string chart_name = name;

            if (Rank != null) Rank.SetActive(true);
            if (Achievement != null) Achievement.SetActive(true);

            var maxScoreTmp = MaxScore.GetComponentInChildren<TextMeshProUGUI>(true);
            
            var rankImage = Rank != null ? Rank.GetComponent<Image>() : null;
            var rankTmp = Rank != null ? Rank.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            
            var achievementImage = Achievement != null ? Achievement.GetComponent<Image>() : null;
            var achievementTmp = Achievement != null ? Achievement.GetComponentInChildren<TextMeshProUGUI>(true) : null;

            if (APIManager.Instance != null && APIManager.Instance.IsLoggedIn())
            {
                if (maxScoreTmp != null) maxScoreTmp.alpha = 1f;

                string[] diffs = { "Master", "Another", "Hard", "Easy" };
                APIManager.ScoreRecord bestRecord = null;

                foreach (var d in diffs)
                {
                    bestRecord = APIManager.Instance.GetHighScoreRecord(chart_name, d);
                    if (bestRecord != null && bestRecord.Score > 0)
                    {
                        break;
                    }
                }

                if (bestRecord != null && bestRecord.Score > 0)
                {
                    int highScore = bestRecord.Score;
                    if (maxScoreTmp != null) maxScoreTmp.text = "Score: " + highScore.ToString();

                    string rank = ((highScore > 990000) ? "SSS+" : (highScore > 980000) ? "SSS" : (highScore > 975000) ? "SS+" : (highScore > 950000) ? "SS" : (highScore > 925000) ? "S+" : (highScore > 900000) ? "S" : (highScore > 850000) ? "AAA" : (highScore > 800000) ? "AA" : (highScore > 750000) ? "A" : (highScore > 700000) ? "BBB" : (highScore > 650000) ? "BB" : (highScore > 600000) ? "B" : (highScore > 550000) ? "C" : "D");

                    string achievement = "Failed";
                    if (highScore >= 1000000) achievement = "AP+";
                    else if (bestRecord.PerfectPlus + bestRecord.Perfect >= bestRecord.MaxNotes && bestRecord.MaxNotes > 0) achievement = "AP";
                    else if (bestRecord.Miss == 0 && (bestRecord.PerfectPlus + bestRecord.Perfect + bestRecord.Great + bestRecord.Good == bestRecord.MaxNotes) && bestRecord.MaxNotes > 0) achievement = "FC";
                    else if (highScore >= 750000) achievement = "Clear";
                    else if (highScore > 0) achievement = "Played";

                    Color32 colorApp = new Color32(0, 255, 255, 255); // Cyan
                    Color32 colorAp = new Color32(255, 100, 255, 255); // Pink
                    Color32 colorFc = new Color32(255, 255, 0, 255); // Yellow
                    Color32 colorClear = new Color32(0, 255, 0, 255); // Green
                    Color32 colorGray = new Color32(150, 150, 150, 255); // Gray

                    Color32 rankColor = colorGray;
                    if (rank.StartsWith("S")) rankColor = colorAp;
                    else if (rank.StartsWith("A")) rankColor = colorFc;
                    else if (rank.StartsWith("B")) rankColor = colorClear;

                    Color32 achievementColor = colorGray;
                    if (achievement == "AP+") achievementColor = colorApp;
                    else if (achievement == "AP") achievementColor = colorAp;
                    else if (achievement == "FC") achievementColor = colorFc;
                    else if (achievement == "Clear") achievementColor = colorClear;

                    if (Rank != null) Rank.SetActive(true);
                    if (rankImage != null) rankImage.color = rankColor;
                    if (rankTmp != null)
                    {
                        rankTmp.text = rank;
                    }

                    if (achievement == "Failed" || achievement == "Played") {
                        if (Achievement != null) Achievement.SetActive(false);
                    } else {
                        if (Achievement != null) Achievement.SetActive(true);
                        if (achievementImage != null) achievementImage.color = achievementColor;
                        if (achievementTmp != null) achievementTmp.text = achievement;
                    }
                }
                else
                {
                    if (maxScoreTmp != null) maxScoreTmp.text = "Score: 0";
                    if (Rank != null) Rank.SetActive(false);
                    if (Achievement != null) Achievement.SetActive(false);
                }
            }
            else
            {
                if (maxScoreTmp != null) maxScoreTmp.alpha = 0f;
                if (Rank != null) Rank.SetActive(false);
                if (Achievement != null) Achievement.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SetText] Error in UpdateHighScoreText for {name}: {e.Message}");
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