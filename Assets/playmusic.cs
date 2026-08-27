using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayMusic : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audioClip;

    public static float time;
    public static bool played;
    public string bgm;
    public static float offset;
    public static bool settedUp;
    private float tmp;
    private VideoClip videoClip;
    public GameObject BackGround;
    public GameObject Plane;
    private UnityEngine.Vector3 bgPos;
    private Transform bgTransform;
    private VideoPlayer videoPlayer;
    private bool videoExists = false;

    // Progress tracking
    public float downloadProgress = 0f; // Percentage of download progress
    private float audioDuration = 0f;
    public static bool audioDownloaded = false;
    public static bool started_dl = false;
    public TextMeshProUGUI progressText;
    public GameObject LoadingText;
    public Slider ProgressBar;
    public Animator Anim;
    public Image ProgressBarFill;

    // �Đ��Ǘ��p�̃t���O
    public static bool audioFinished = false;

    private void Awake()
    {
        // ���łɃA�^�b�`����Ă���ꍇ�͐V�����ǉ����Ȃ�
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private IEnumerator Start()
    {
        StartCoroutine(PlayMusicRoutine());
        ProgressBar.maxValue = 100.0f;
        yield return null;
    }

    private bool isVideoExists(string bgm)
    {
        VideoClip video;
        try
        {
            video = Resources.Load<VideoClip>("Videos/" + bgm);
            if (video == null)
            {
                video = null;
                return false;
            }
            else
            {
                video = null;
                return true;
            }
        }
        catch (NullReferenceException e)
        {
            video = null;
            return false;
        }
    }

    private void Update()
    {
        
    }

    private IEnumerator PlayMusicRoutine()
    {
        while (true)
        {

            if (adata.start_dl && !started_dl)
            {
                // �I�[�f�B�I�t�@�C���̃_�E�����[�h�J�n
                JObject jsonObj = adata.jsonObj;
                bgm = jsonObj["maindata"]["music"].ToString();
                if (adata.isOfflineMode)
                {
                    string safeFileName = adata.GetSafeFileName(bgm);
                    string videoPath = Path.Combine(adata.musicPath, safeFileName + ".mp4");
                    videoExists = File.Exists(videoPath);
                    audioDownloaded = true;
                    adata.ready_to_start = true;
                }
                else
                {
                    StartCoroutine(DownloadAudioData(bgm));
                }
                started_dl = true;
                adata.Process = false;
            }

            if (adata.ready_to_start)
            {
                if (!settedUp)
                {
                    Debug.Log("Setting up...");
                    // �_�E�����[�h�����������Ɉړ�
                    JObject jsonObj = adata.jsonObj;
                    bgm = jsonObj["maindata"]["music"].ToString();
                    //offset = float.Parse(jsonObj["maindata"]["offset"] + "") / adata.offset_diameter;
                    offset = float.Parse(jsonObj["maindata"]["offset"] + "");
                    bgTransform = BackGround.transform;
                    bgPos = bgTransform.position;

                    if (videoExists)
                    {
                        videoPlayer = Plane.GetComponent<VideoPlayer>();
                        
                        // 背景としてカメラの最奥に直接描画する設定
                        videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
                        videoPlayer.targetCamera = Camera.main;
                        videoPlayer.aspectRatio = VideoAspectRatio.FitOutside; // 画面全体にフィットさせる
                        
                        // 3Dの板(Plane)自体が描画されて貫通するのを防ぐため、メッシュ描画をオフにする
                        MeshRenderer mesh = Plane.GetComponent<MeshRenderer>();
                        if (mesh != null) mesh.enabled = false;

                        videoPlayer.source = VideoSource.Url;
                        if (adata.isOfflineMode)
                        {
                            string safeFileName = adata.GetSafeFileName(bgm);
                            videoPlayer.url = Path.Combine(adata.musicPath, safeFileName + ".mp4");
                        }
                        else
                        {
                            videoPlayer.url = Path.Combine(Application.streamingAssetsPath, "video.mp4");
                        }
                        bgPos.y = -3.89f;
                        videoPlayer.Play();
                    }
                    else
                    {
                        bgPos.y = -3.87f;
                    }

                    settedUp = true;
                    audioFinished = false;

                    // Send music info to the debugger
                    JObject musicInfoPayload = new JObject();
                    musicInfoPayload["title"] = bgm ?? "Unknown Title";
                    //musicInfoPayload["artist"] = jsonObj["maindata"]["composer"]?.ToString() ?? "Unknown Artist";
                    musicInfoPayload["composer"] = jsonObj["maindata"]["composer"]?.ToString() ?? "Unknown Composer";
                    musicInfoPayload["level"] = jsonObj["maindata"]["level"]?.ToString() ?? "N/A";
                    //musicInfoPayload["difficulty"] = jsonObj["maindata"]["difficulty"]?.ToString() ?? "N/A";

                    JObject root = new JObject();
                    root["source"] = "MusicInfo";
                    root["payload"] = musicInfoPayload;
                    DebugSocketClient.Instance.SendData(root.ToString());
                }

                time += Time.deltaTime;

                // �_�E�����[�h���������Ă��āA�^�C�~���O�����𖞂������ꍇ�ɍĐ�����
                if (time >= (8.0f + adata.tlag) && !played && audioDownloaded)
                {
                    adata.MusicStarted = true;
                    //PlayBgm();
                    Debug.Log("Playing...");
                    if (adata.isOfflineMode)
                    {
                        string safeFileName = adata.GetSafeFileName(bgm);
                        string path = Path.Combine(adata.musicPath, safeFileName + ".wav");
                        if (!File.Exists(path))
                        {
                            Debug.LogError("Audio file not found: " + path);
                            yield break;
                        }

                        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + path.Replace("+", "%2B").Replace(" ", "+").Replace("?", "%3F").Replace("&", "%26"), AudioType.WAV))
                        {
                            yield return audioRequest.SendWebRequest();

                            if (audioRequest.result != UnityWebRequest.Result.Success)
                            {
                                Debug.LogError($"Failed to load audio: {audioRequest.error}");
                                yield break;
                            }

                            audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                            if (audioClip == null)
                            {
                                Debug.LogError("Failed to create AudioClip from downloaded data!");
                                yield break;
                            }

                            audioSource.clip = audioClip;
                            audioSource.volume = 1.0f;
                            audioSource.loop = false;
                            adata.length = audioClip.length;
                            audioSource.Play();
                            Debug.Log("Audio started playing on offline mode: " + audioClip.name);
                        }

                        played = true;
                        if (videoExists)
                        {
                            bgPos.y = -3.89f;
                            bgTransform.position = bgPos;
                        }
                        Debug.Log("Play... Audio Length: " + audioClip.length);
                    }
                    else
                    {
                        string path = Path.Combine(Application.streamingAssetsPath, "audio.wav");

                        if (!File.Exists(path))
                        {
                            Debug.LogError("Audio file not found in StreamingAssets!");
                            yield break;
                        }

                        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + path.Replace("+", "%2B").Replace(" ", "+").Replace("?", "%3F").Replace("&", "%26"), AudioType.WAV))
                        {
                            yield return audioRequest.SendWebRequest();

                            if (audioRequest.result != UnityWebRequest.Result.Success)
                            {
                                Debug.LogError($"Failed to load audio: {audioRequest.error}");
                                yield break;
                            }

                            audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                            if (audioClip == null)
                            {
                                Debug.LogError("Failed to create AudioClip from downloaded data!");
                                yield break;
                            }

                            audioSource.clip = audioClip;
                            audioSource.volume = 1.0f;
                            audioSource.loop = false;
                            adata.length = audioClip.length;
                            audioSource.Play();
                            Debug.Log("Audio started playing: " + audioClip.name);
                        }

                        played = true;
                        if (videoExists)
                        {
                            bgPos.y = -3.89f;
                            bgTransform.position = bgPos;
                        }
                        Debug.Log("Play... Audio Length: " + audioClip.length);
                    }
                }
                else if (!played && videoExists)
                {
                    videoPlayer.time = 0;
                }

                if (played)
                {
                    adata.game_time = audioSource.time - (offset / audioSource.clip.frequency) + adata.tlag;
                }
                else
                {
                    // 曲が始まる前(待機時間中)も、game_timeをマイナス値からスムーズに進行させる
                    // これにより、UIのオフセット(tlag)を設定した際もワープせずにスムーズに落下するようになります
                    float freq = (audioSource.clip != null) ? audioSource.clip.frequency : 44100f;
                    adata.game_time = time - 8.0f - (offset / freq);
                }

                // �I�[�f�B�I�̍Đ���Ԃ��`�F�b�N
                if (played && !audioSource.isPlaying && !audioFinished)
                {
                    audioFinished = true;
                    Debug.Log("Audio playback finished");
                }

                // �Đ����I�����Ă���I���������s��
                if (audioFinished || adata.ForceFinish)
                {
                    if (adata.ForceFinish)
                    {
                        tmp = 100f;
                    }

                    if (tmp >= 3.0f)
                    {
                        Debug.Log("Stop...");
                        played = false;
                        StopBgm();
                        audioSource.volume = 0f;
                        audioSource.loop = false;
                        if (videoExists)
                        {
                            videoPlayer.Stop();
                            videoPlayer.url = ""; // ファイルのロックを解除する
                        }
                        bgPos.y = -3.87f;
                        bgTransform.position = bgPos;
                        Anim.SetBool("bOpen", false);
                        ResultUI.Score = ScoreManeger.ratioscore;
                        LoadingText.SetActive(false);
                        if (DebugText.isDebugMode)
                        {
                            DebugText debugText = FindObjectOfType<DebugText>();
                            string debug = debugText.getDebugText();
                            GUIUtility.systemCopyBuffer = debug;
                            //EditorUtility.DisplayDialog("Debug", "Debug data copied to clipboard", "OK");
                        }
                        settedUp = false;
                        audioDownloaded = false;
                        audioFinished = false;
                        time = 0;
                        played = false;
                        adata.ForceFinish = false;
                        adata.ready_to_start = false;
                        adata.showResult = true;
                        ResultUI resultUI = FindObjectOfType<ResultUI>();
                        resultUI.SetResultData(
                            bgm,
                            adata.jsonObj["maindata"]["composer"].ToString(),
                            adata.jsonObj["maindata"]["level"].ToString(),
                            ScoreManeger.ratioscore,
                            DebugText.perfect_l1 + DebugText.perfect_l2 + DebugText.perfect_l3 + DebugText.perfect_l4,
                            DebugText.good_l1 + DebugText.good_l2 + DebugText.good_l3 + DebugText.good_l4,
                            DebugText.bad_l1 + DebugText.bad_l2 + DebugText.bad_l3 + DebugText.bad_l4,
                            DebugText.miss_l1 + DebugText.miss_l2 + DebugText.miss_l3 + DebugText.miss_l4,
                            ShowDetails.jacket
                        );
                    }
                    else
                    {
                        tmp += Time.deltaTime;
                        audioFinished = true;
                        /*
                        if (tmp >= 3.0f)
                        {
                            StopBgm();
                            if (videoExists)
                            {
                                videoPlayer.Stop();
                            }
                            bgPos.y = -3.87f;
                            bgTransform.position = bgPos;
                        }
                        */
                    }
                }
                else
                {
                    tmp = 0;
                }
            }

            yield return null;
        }
    }

    private IEnumerator DownloadAudioData(string songName)
    {
        // First get the duration and file size
        string durationUrl = $"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getDuration/?bgm={Uri.EscapeDataString(songName)}";
        float expectedContentLength = 0f;
        Debug.Log($"Requesting duration info from: {durationUrl}");
        using (UnityWebRequest durationRequest = UnityWebRequest.Get(durationUrl))
        {
            yield return durationRequest.SendWebRequest();

            if (durationRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string durationJson = durationRequest.downloadHandler.text;
                    JObject durationData = JObject.Parse(durationJson);
                    audioDuration = float.Parse(durationData["duration"].ToString());
                    expectedContentLength = durationData["length"].ToObject<float>();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to parse duration info: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Failed to get duration info (ignoring): {durationRequest.error}");
            }
        }

        // Now download the actual audio file
        string audioUrl = $"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getAudio/?chart={Uri.EscapeDataString(songName)}";
        Debug.Log($"Downloading audio from: {audioUrl}");
        ProgressBarFill.color = new Color32(255, 165, 0, 120);
        using (UnityWebRequest audioRequest = UnityWebRequest.Get(audioUrl))
        {
            audioRequest.SendWebRequest();

            // Track download progress
            while (!audioRequest.isDone)
            {
                if (expectedContentLength > 0)
                {
                    downloadProgress = audioRequest.downloadedBytes / expectedContentLength * 100f;
                }
                else
                {
                    // Fallback progress if size is unknown
                    downloadProgress = Mathf.Clamp(audioRequest.downloadedBytes / 5000000f * 100f, 0f, 99f);
                }

                if (progressText != null)
                {
                    progressText.text = $"ダウンロード中... {downloadProgress:F0}%";
                    ProgressBar.value = downloadProgress;
                }
                yield return null;
            }

            if (audioRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download audio: {audioRequest.error}");
                started_dl = false;
                yield break;
            }

            // Get the downloaded audio data as a byte array
            byte[] audioData = audioRequest.downloadHandler.data;
            if (audioData != null && audioData.Length > 0)
            {
                // Save the downloaded data to StreamingAssets/audio.wav
                string path = Path.Combine(Application.streamingAssetsPath, "audio.wav");
                File.WriteAllBytes(path, audioData);

                // Update progress and flags for audio
                Debug.Log($"Audio downloaded and saved successfully to {path}.");
            }
            else
            {
                Debug.LogError("Downloaded audio data is null or empty!");
                started_dl = false;
                yield break;
            }
        }

        // Check if video exists
        string videoExistsUrl = $"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/videoExists?name={Uri.EscapeDataString(songName)}";
        using (UnityWebRequest existsReq = UnityWebRequest.Get(videoExistsUrl))
        {
            yield return existsReq.SendWebRequest();
            if (existsReq.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    JObject existsData = JObject.Parse(existsReq.downloadHandler.text);
                    videoExists = existsData["exists"].ToObject<bool>();
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse videoExists response: " + e.Message);
                    videoExists = false;
                }
            }
            else
            {
                videoExists = false;
            }
        }

        if (videoExists)
        {
            string videoUrl = $"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getVideo?chart={Uri.EscapeDataString(songName)}";
            Debug.Log($"Downloading video from: {videoUrl}");
            using (UnityWebRequest videoReq = UnityWebRequest.Get(videoUrl))
            {
                videoReq.SendWebRequest();

                while (!videoReq.isDone)
                {
                    downloadProgress = videoReq.downloadProgress * 100f;
                    if (progressText != null)
                    {
                        progressText.text = $"動画をダウンロード中... {downloadProgress:F0}%";
                        ProgressBar.value = downloadProgress;
                    }
                    yield return null;
                }

                if (videoReq.result == UnityWebRequest.Result.Success)
                {
                    byte[] videoData = videoReq.downloadHandler.data;
                    if (videoData != null && videoData.Length > 0)
                    {
                        string videoPath = Path.Combine(Application.streamingAssetsPath, "video.mp4");
                        try
                        {
                            File.WriteAllBytes(videoPath, videoData);
                            Debug.Log($"Video downloaded and saved successfully to {videoPath}.");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to save video file (it might be locked): {e.Message}");
                            videoExists = false;
                        }
                    }
                    else
                    {
                        Debug.LogError("Downloaded video data is null or empty!");
                        videoExists = false;
                    }
                }
                else
                {
                    Debug.LogError($"Failed to download video: {videoReq.error}");
                    videoExists = false;
                }
            }
        }

        // Update final progress and flags
        downloadProgress = 100f;
        audioDownloaded = true;
        adata.ready_to_start = true;

        if (progressText != null)
        {
            progressText.text = "";
            ProgressBar.value = 0f;
            ProgressBarFill.color = new Color32(0, 0, 0, 107);
        }
    }

    public void PlayBgm()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "audio.wav");
        if (adata.isOfflineMode)
        {
            if (adata.jsonObj != null && adata.jsonObj["maindata"] != null)
            {
                string bgm = adata.jsonObj["maindata"]["music"].ToString();
                string safeFileName = adata.GetSafeFileName(bgm);
                path = Path.Combine(adata.musicPath, safeFileName + ".wav");
            }
        }

        if (!File.Exists(path))
        {
            Debug.LogError($"Audio file not found: {path}");
            return;
        }

        StartCoroutine(LoadAndPlayAudio(path));
    }

    private IEnumerator LoadAndPlayAudio(string path)
    {
        string fileUri = new System.Uri(path).AbsoluteUri;
        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.WAV))
        {
            yield return audioRequest.SendWebRequest();

            if (audioRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load audio: {audioRequest.error}");
                yield break;
            }

            audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
            if (audioClip == null)
            {
                Debug.LogError("Failed to create AudioClip from downloaded data!");
                yield break;
            }

            audioSource.clip = audioClip;
            audioSource.volume = 1.0f;
            adata.length = audioClip.length;
            audioSource.Play();
            Debug.Log("Audio started playing: " + audioClip.name);
        }
    }

    public void StopBgm()
    {
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
            audioSource.clip = null;
            //string path = Path.Combine(Application.dataPath, "Resources/audio.wav");
            //File.Delete(path);
        }
    }
}
