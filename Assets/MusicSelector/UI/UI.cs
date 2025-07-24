using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameObject canvas;
    public GameObject MusicPrefab;
    public GameObject PlayerUI;
    public GameObject PageNo;
    private List<GameObject> MusicObj;
    private int NowPage = 0;
    public static bool Open = false;
    private List<string> Musics = new List<string>();
    private List<List<bool>> LevelExists = new List<List<bool>>();
    public TextMeshProUGUI level_text;
    public Button upButton;
    public Button downButton;
    public Button rightButton;
    public Button leftButton;
    private float last_tap;
    public GameObject MusicName;
    public GameObject Composer;
    public GameObject MusicJacket;
    public GameObject Level;

    public GameObject LevelSelector;
    public Animator Anim;
    public Button ABtn;
    public Button MBtn;
    public Button HBtn;
    public Button EBtn;

    public GameObject EasyText;
    public GameObject HardText;
    public GameObject MasterText;
    public GameObject AnotherText;

    private float ClickTime = 0.0f;
    private float LastMove = 0.0f;
    private int Moved = 0;
    private int currentDirection = 0;
    private float TempClickTime;

    private AudioSource previewAudioSource;
    private Coroutine previewAudioCoroutine;
    private Coroutine fadeCoroutine;
    private string previewAudioCachePath;

    private JObject httpJsonObj;

    public GameObject LoadingText;
    public static bool Entered = false;
    public int SetupProgress = 0;
    public GameObject SetupLoading;
    public GameObject SetupText;

    public TMP_Dropdown SortBy;
    public TMP_Dropdown OrderBy;
    public TMP_Dropdown Genre;
    private string[] SortModes = { "name", "date" };
    private string[] OrderModes = { "asc", "desc" };
    private string[] Genres = { "all", "ゲーム", "VOCALOID", "POPS%26ANIME" };

    public Toggle KeepDownloads;
    public Toggle OfflineMode;
    public Button reDownload;

    private string settingsFilePath;
    private static bool checksumManagerInitialized = false;

    void Awake()
    {
        if (!checksumManagerInitialized)
        {
            ChecksumManager.Initialize();
            checksumManagerInitialized = true;
        }

        previewAudioSource = gameObject.GetComponent<AudioSource>();
        if (previewAudioSource == null)
        {
            previewAudioSource = gameObject.AddComponent<AudioSource>();
        }
        previewAudioSource.playOnAwake = false;
        previewAudioSource.loop = true;

        previewAudioCachePath = Path.Combine(Application.persistentDataPath, "PreviewAudioCache");
        if (!Directory.Exists(previewAudioCachePath))
        {
            Directory.CreateDirectory(previewAudioCachePath);
        }

        settingsFilePath = Path.Combine(Application.persistentDataPath, "settings.json");
    }

    private void Start()
    {
        Anim = LevelSelector.GetComponent<Animator>();
        SortBy.onValueChanged.AddListener(delegate { OnSortChanged(); });
        OrderBy.onValueChanged.AddListener(delegate { OnSortChanged(); });
        Genre.onValueChanged.AddListener(delegate { OnSortChanged(); });

        // ★★★ 変更箇所 ★★★
        // reDownloadボタンにリスナーを追加 (具体的な処理は未実装)
        if (reDownload != null)
        {
            // reDownload.onClick.AddListener(OnReDownloadClicked);
        }

        // 設定の読み込みと、トグルへのリスナー設定
        if (KeepDownloads != null && OfflineMode != null)
        {
            LoadSettings();
            KeepDownloads.onValueChanged.AddListener(isOn => SaveSettings());
            OfflineMode.onValueChanged.AddListener(isOn => SaveSettings());
        }
        OfflineMode.onValueChanged.AddListener(
        delegate
        {
            if (OfflineMode.isOn)
                StartCoroutine(DownloadAll());
            reDownload.interactable = OfflineMode.isOn;
            SortBy.interactable = !OfflineMode.isOn;
            OrderBy.interactable = !OfflineMode.isOn;
            Genre.interactable = !OfflineMode.isOn;
            KeepDownloads.interactable = !OfflineMode.isOn;
            KeepDownloads.isOn = OfflineMode.isOn;
            SaveSettings();
        });

        reDownload.onClick.AddListener(() => StartCoroutine(DownloadAll()));
    }

    /// <summary>
    /// settings.jsonから設定を読み込み、UIに反映します。
    /// </summary>
    private void LoadSettings()
    {
        if (File.Exists(settingsFilePath))
        {
            try
            {
                string json = File.ReadAllText(settingsFilePath);
                JObject settings = JObject.Parse(json);
                KeepDownloads.isOn = settings["keepDownloads"]?.Value<bool>() ?? false;
                OfflineMode.isOn = settings["offlineMode"]?.Value<bool>() ?? false;
            }
            catch (Exception e)
            {
                Debug.LogError($"設定ファイルの読み込みに失敗: {e.Message}。デフォルト値を使用します。");
                KeepDownloads.isOn = false;
                OfflineMode.isOn = false;
            }
        }
        adata.isOfflineMode = OfflineMode.isOn;
        reDownload.interactable = OfflineMode.isOn;
        SortBy.interactable = !OfflineMode.isOn;
        OrderBy.interactable = !OfflineMode.isOn;
        Genre.interactable = !OfflineMode.isOn;
        KeepDownloads.interactable = !OfflineMode.isOn;
        if (OfflineMode.isOn) KeepDownloads.isOn = OfflineMode.isOn;
    }

    // ★★★ 追加メソッド ★★★
    /// <summary>
    /// 現在のトグルの状態をsettings.jsonに保存します。
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            JObject settings = File.Exists(settingsFilePath)
                ? JObject.Parse(File.ReadAllText(settingsFilePath))
                : new JObject();

            settings["keepDownloads"] = KeepDownloads.isOn;
            settings["offlineMode"] = OfflineMode.isOn;

            File.WriteAllText(settingsFilePath, settings.ToString(Newtonsoft.Json.Formatting.Indented));
            Debug.Log($"設定を保存しました: {settingsFilePath}");

            adata.isOfflineMode = OfflineMode.isOn;
        }
        catch (Exception e)
        {
            Debug.LogError($"設定ファイルの保存に失敗: {e.Message}");
        }
    }


    private void OnSortChanged()
    {
        adata.sort = SortModes[SortBy.value];
        adata.order = OrderModes[OrderBy.value];
        adata.genre = Genres[Genre.value];
        Setup();
        Anim.SetBool("bOpen", false);
    }

    // ★★★ 修正箇所 ★★★
    private void Setup()
    {
        if (SetupLoading != null) SetupLoading.SetActive(true);

        if (MusicObj != null)
        {
            foreach (var o in MusicObj) Destroy(o);
        }

        canvas.GetComponent<CanvasGroup>().alpha = 1.0f;
        PlayerUI.SetActive(false);
        adata.ready_to_start = false;
        MusicObj = new List<GameObject>();

        // オフラインモードの場合、ローカルから読み込む
        if (OfflineMode.isOn)
        {
            string musicListCachePath = Path.Combine(Application.persistentDataPath, "music_list.json");
            if (File.Exists(musicListCachePath))
            {
                Debug.Log("[Setup] Offline mode: Loading music list from cache.");
                string jsonContent = File.ReadAllText(musicListCachePath);
                httpJsonObj = JObject.Parse(jsonContent);
                adata.musicsJson = httpJsonObj;
                Musics = (httpJsonObj["charts"] as JArray).ToObject<List<string>>();
                adata.checksumsJson = null; // オフラインでは不要
                adata.audioChecksumsJson = null; // オフラインでは不要
                StartCoroutine(SetupProcessCoroutine());
            }
            else
            {
                SetupText.GetComponent<TextMeshProUGUI>().text = "オフラインデータがありません。オンラインで接続してください。";
                if (SetupLoading != null) SetupLoading.SetActive(false);
                Debug.LogError("[Setup] Offline mode is on, but no local music list found.");
            }
            return;
        }

        // --- オンラインモードの処理 ---
        Debug.Log("[Setup] Online mode: Fetching data from server.");
        try
        {
            var client = new RestClient($"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getMusicList/index.cgi?sort={adata.sort}&order={adata.order}&genre={adata.genre}");
            var request = new RestRequest { Method = Method.Get, Timeout = TimeSpan.FromMinutes(10) };
            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Server returned error: {response.StatusCode}");
            }

            // 楽曲リストをローカルにキャッシュとして保存
            string musicListCachePath = Path.Combine(Application.persistentDataPath, "music_list.json");
            File.WriteAllText(musicListCachePath, response.Content);

            httpJsonObj = JObject.Parse(response.Content);
            adata.musicsJson = httpJsonObj;
            Musics = (httpJsonObj["charts"] as JArray).ToObject<List<string>>();

            var checksumClient = new RestClient("https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getJacket/getChecksum/?command=all");
            var checksumResponse = checksumClient.Execute(new RestRequest { Method = Method.Get, Timeout = TimeSpan.FromMinutes(10) });
            adata.checksumsJson = JObject.Parse(checksumResponse.Content);

            var audioChecksumClient = new RestClient("https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getPreviewAudio/getChecksum/?command=all");
            var audioChecksumResponse = audioChecksumClient.Execute(new RestRequest { Method = Method.Get, Timeout = TimeSpan.FromMinutes(10) });
            adata.audioChecksumsJson = JObject.Parse(audioChecksumResponse.Content);

            StartCoroutine(SetupProcessCoroutine());
        }
        catch (Exception e)
        {
            SetupText.GetComponent<TextMeshProUGUI>().text = $"サーバーに接続できませんでした。{e.Message}";
            if (SetupLoading != null) SetupLoading.SetActive(false);
            Debug.LogError($"[Setup] Failed to fetch data from server. Error: {e.Message}");
        }
    }


    private IEnumerator SetupProcessCoroutine()
    {
        NowPage = 0;
        LevelExists = (httpJsonObj["diffs"] as JArray).ToObject<List<List<bool>>>();
        PageNo.GetComponent<TextMeshProUGUI>().text = $"1/{Musics.Count}";
        UpdateLevelTexts();

        for (int i = 0; i < Musics.Count; i++)
        {
            MusicObj.Add(Instantiate(MusicPrefab));
            MusicObj[i].transform.parent = canvas.transform;
            Vector3 pos = MusicObj[i].transform.position;
            pos.x = 0.0f + 500.0f * i;
            pos.y = 0.0f;
            pos.z = 0.0f;
            MusicObj[i].transform.position = pos;
            MusicObj[i].name = (string)Musics[i];
            MusicObj[i].transform.SetSiblingIndex(1);
        }

        adata.loaded = 0;
        while (adata.loaded < Musics.Count)
        {
            SetupProgress = (int)(((float)adata.loaded / Musics.Count) * 100);
            SetupText.GetComponent<TextMeshProUGUI>().text = $"Loading Jackets... (Step 1/2)  {SetupProgress}%";
            yield return null;
        }

        adata.audioLoaded = 0;
        foreach (string musicName in Musics)
        {
            //Debug.Log($"[Setup] Loading audio preview for {musicName}");
            StartCoroutine(LoadOrDownloadAudio(musicName, (clip) =>
            {
                if (clip != null)
                {
                    adata.previewAudioCache[musicName] = clip;
                }
                adata.audioLoaded++;
            }));
        }

        while (adata.audioLoaded < Musics.Count)
        {
            SetupProgress = (int)(((float)adata.audioLoaded / Musics.Count) * 100);
            SetupText.GetComponent<TextMeshProUGUI>().text = $"Loading Previews... (Step 2/2)  {SetupProgress}%";
            yield return null;
        }

        SetupLoading.SetActive(false);
        Moved = MusicObj.Count;

        if (Musics.Count > 0)
        {
            if (previewAudioCoroutine != null) StopCoroutine(previewAudioCoroutine);
            previewAudioCoroutine = StartCoroutine(HandlePreviewAudioChange());
        }
    }

    private void UpdateLevelTexts()
    {
        if (httpJsonObj == null || NowPage < 0 || NowPage >= Musics.Count) return;
        AnotherText.GetComponent<TextMeshProUGUI>().text = httpJsonObj["levels"][NowPage][0] + "";
        MasterText.GetComponent<TextMeshProUGUI>().text = httpJsonObj["levels"][NowPage][1] + "";
        HardText.GetComponent<TextMeshProUGUI>().text = httpJsonObj["levels"][NowPage][2] + "";
        EasyText.GetComponent<TextMeshProUGUI>().text = httpJsonObj["levels"][NowPage][3] + "";
    }

    private void MoveMusicSelection(int direction)
    {
        if (MusicObj == null || MusicObj.Count == 0) return;

        if (NowPage + direction >= 0 && NowPage + direction < MusicObj.Count)
        {
            NowPage += direction;
            Moved = 0;
            for (int i = 0; i < MusicObj.Count; i++)
            {
                StartCoroutine(MoveMusic(MusicObj[i], direction, 0.05f));
            }

            PageNo.GetComponent<TextMeshProUGUI>().text = $"{NowPage + 1}/{MusicObj.Count}";
            UpdateLevelTexts();

            if (previewAudioCoroutine != null) StopCoroutine(previewAudioCoroutine);
            previewAudioCoroutine = StartCoroutine(HandlePreviewAudioChange());
        }
    }

    private void ProcessMove(int direction)
    {
        if (MusicObj == null || MusicObj.Count == 0) return;

        if (direction == 1)
        {
            if (NowPage == MusicObj.Count - 1) MoveMusicSelection(-NowPage);
            else MoveMusicSelection(1);
        }
        else if (direction == -1)
        {
            if (NowPage == 0) MoveMusicSelection(MusicObj.Count - 1);
            else MoveMusicSelection(-1);
        }
    }

    private IEnumerator MoveMusic(GameObject music, int direction, float duration)
    {
        Vector3 startPosition = music.transform.position;
        Vector3 endPosition = new Vector3(startPosition.x - 500f * direction, startPosition.y, startPosition.z);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            music.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        music.transform.position = endPosition;
        Moved++;
    }

    private IEnumerator HandlePreviewAudioChange()
    {
        if (previewAudioSource.isPlaying)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOut(adata.previewFadeDuration));
            yield return fadeCoroutine;
        }

        string musicName = MusicObj[NowPage].name;

        if (adata.previewAudioCache.ContainsKey(musicName))
        {
            PlayPreview(adata.previewAudioCache[musicName]);
        }
        else
        {
            yield return StartCoroutine(LoadOrDownloadAudio(musicName, (loadedClip) =>
            {
                if (loadedClip != null)
                {
                    adata.previewAudioCache[musicName] = loadedClip;
                    PlayPreview(loadedClip);
                }
            }));
        }
    }

    private void PlayPreview(AudioClip clip)
    {
        previewAudioSource.clip = clip;
        previewAudioSource.Play();
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIn(adata.previewFadeDuration));
    }

    // ★★★ 修正箇所 ★★★
    private IEnumerator LoadOrDownloadAudio(string musicName, Action<AudioClip> onLoaded)
    {
        string safeFileName = string.Join("_", musicName.Split(Path.GetInvalidFileNameChars()));
        string localAudioPath = Path.Combine(previewAudioCachePath, safeFileName + ".wav");

        // オフラインモードの場合、ローカルキャッシュのみ確認
        if (OfflineMode.isOn)
        {
            if (File.Exists(localAudioPath))
            {
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + localAudioPath.Replace("+", "%2B"), AudioType.WAV))
                {
                    yield return www.SendWebRequest();
                    onLoaded(www.result == UnityWebRequest.Result.Success ? DownloadHandlerAudioClip.GetContent(www) : null);
                }
            }
            else
            {
                onLoaded(null); // オフラインでは利用不可
            }
            yield break;
        }

        // --- オンラインモードの処理 ---
        string encodedMusicName = Uri.EscapeDataString(musicName);
        string audioUrl = $"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getPreviewAudio/?chart={encodedMusicName}";

        string serverChecksum = "";
        if (adata.audioChecksumsJson != null)
        {
            try
            {
                var charts = adata.audioChecksumsJson["charts"].ToObject<List<string>>();
                var checksums = adata.audioChecksumsJson["checksums"].ToObject<List<string>>();
                int index = charts.IndexOf(musicName);
                if (index != -1) serverChecksum = checksums[index];
            }
            catch (Exception e) { Debug.LogError($"[PreviewAudio] Error getting server checksum: {e.Message}"); }
        }

        string localChecksum = ChecksumManager.GetAudioChecksum(musicName);

        if (File.Exists(localAudioPath) && !string.IsNullOrEmpty(serverChecksum) && serverChecksum == localChecksum)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + localAudioPath.Replace("+", "%2B"), AudioType.WAV))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    onLoaded(DownloadHandlerAudioClip.GetContent(www));
                    yield break;
                }
            }
        }

        using (UnityWebRequest www = UnityWebRequest.Get(audioUrl))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(localAudioPath, www.downloadHandler.data);
                if (!string.IsNullOrEmpty(serverChecksum))
                {
                    ChecksumManager.UpdateAudioChecksum(musicName, serverChecksum);
                }

                using (UnityWebRequest loadedWww = UnityWebRequestMultimedia.GetAudioClip("file://" + localAudioPath.Replace("+", "%2B"), AudioType.WAV))
                {
                    yield return loadedWww.SendWebRequest();
                    onLoaded(loadedWww.result == UnityWebRequest.Result.Success ? DownloadHandlerAudioClip.GetContent(loadedWww) : null);
                }
            }
            else
            {
                Debug.LogError($"[PreviewAudio] Failed to download {musicName}: {www.error}");
                onLoaded(null);
            }
        }
    }

    /// <summary>
    /// オフラインモード用に全ての楽曲データをダウンロードします。
    /// プレビュー音源と本体音源のダウンロードは全曲一括並列で行います。
    /// </summary>
    private IEnumerator DownloadAll()
    {
        Debug.Log("[DownloadAll] Starting download of all music data for offline mode...");
        if (SetupLoading != null) SetupLoading.SetActive(true);
        if (SetupText != null)
        {
            SetupText.GetComponent<TextMeshProUGUI>().text = "オフラインデータの準備中...";
        }
        yield return null;

        // 保存先ディレクトリのパスを定義
        string basePath = Application.persistentDataPath;
        string musicPath = Path.Combine(basePath, "Musics");
        string chartsPath = Path.Combine(basePath, "Charts");
        string anotherPath = Path.Combine(chartsPath, "Another");
        string masterPath = Path.Combine(chartsPath, "Master");
        string hardPath = Path.Combine(chartsPath, "Hard");
        string easyPath = Path.Combine(chartsPath, "Easy");
        string jacketsPath = Path.Combine(basePath, "Jackets");
        string previewsPath = Path.Combine(basePath, "Previews");

        if (Directory.Exists(musicPath)) Directory.Delete(musicPath, true);
        if (Directory.Exists(chartsPath)) Directory.Delete(chartsPath, true);
        if (Directory.Exists(jacketsPath)) Directory.Delete(jacketsPath, true);
        if (Directory.Exists(previewsPath)) Directory.Delete(previewsPath, true);

        // ディレクトリが存在しない場合は作成
        Directory.CreateDirectory(musicPath);
        Directory.CreateDirectory(anotherPath);
        Directory.CreateDirectory(masterPath);
        Directory.CreateDirectory(hardPath);
        Directory.CreateDirectory(easyPath);
        Directory.CreateDirectory(jacketsPath);
        Directory.CreateDirectory(previewsPath);

        // APIのベースURL
        const string baseUrl = "https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api";
        string[] diffs = { "Another", "Master", "Hard", "Easy" };

        if (Musics == null || Musics.Count == 0)
        {
            Debug.LogError("[DownloadAll] Music list is empty. Cannot start download.");
            if (SetupText != null) SetupText.GetComponent<TextMeshProUGUI>().text = "楽曲リストが空です。";
            yield return new WaitForSeconds(2);
            if (SetupLoading != null) SetupLoading.SetActive(false);
            yield break;
        }

        // 1. 全部の譜面
        for (int i = 0; i < Musics.Count; i++)
        {
            string musicName = Musics[i];
            string encodedMusicName = Uri.EscapeDataString(musicName);
            string safeFileName = string.Join("_", musicName.Split(Path.GetInvalidFileNameChars()));
            for (int j = 0; j < diffs.Length; j++)
            {
                if (LevelExists[i][j])
                {
                    string diff = diffs[j];
                    string chartUrl = $"{baseUrl}/getChart/?chart={encodedMusicName}&diff={diff}";
                    string savePath = Path.Combine(Path.Combine(chartsPath, diff), safeFileName + ".json");
                    if (SetupText != null)
                        SetupText.GetComponent<TextMeshProUGUI>().text = $"譜面ダウンロード中... ({i + 1}/{Musics.Count})\n{musicName} [{diff}]";
                    yield return StartCoroutine(DownloadAndSaveFileCoroutineWithSpeed(chartUrl, savePath, SetupText));
                }
            }
        }

        // 2. 全部のジャケット
        for (int i = 0; i < Musics.Count; i++)
        {
            string musicName = Musics[i];
            string encodedMusicName = Uri.EscapeDataString(musicName);
            string safeFileName = string.Join("_", musicName.Split(Path.GetInvalidFileNameChars()));
            string jacketUrl = $"{baseUrl}/getJacket/?chart={encodedMusicName}";
            string jacketSavePath = Path.Combine(jacketsPath, safeFileName + ".jpg");
            if (SetupText != null)
                SetupText.GetComponent<TextMeshProUGUI>().text = $"ジャケットダウンロード中... ({i + 1}/{Musics.Count})\n{musicName}";
            yield return StartCoroutine(DownloadAndSaveFileCoroutineWithSpeed(jacketUrl, jacketSavePath, SetupText));
        }

        // 3&4. プレビュー音源と本体音源を全曲一括並列ダウンロード
        int total = Musics.Count;
        int completed = 0;
        bool[] previewDone = new bool[total];
        bool[] audioDone = new bool[total];
        List<Coroutine> running = new List<Coroutine>();

        // プレビュー音源ダウンロード用コルーチン
        IEnumerator DownloadPreview(int idx)
        {
            string musicName = Musics[idx];
            string encodedMusicName = Uri.EscapeDataString(musicName);
            string safeFileName = string.Join("_", musicName.Split(Path.GetInvalidFileNameChars()));
            string previewUrl = $"{baseUrl}/getPreviewAudio/?chart={encodedMusicName}";
            string previewSavePath = Path.Combine(previewsPath, safeFileName + ".wav");
            if (SetupText != null)
                SetupText.GetComponent<TextMeshProUGUI>().text = $"プレビュー音源ダウンロード中... ({idx + 1}/{total})\n{musicName}";
            yield return StartCoroutine(DownloadAndSaveFileCoroutineWithSpeed(previewUrl, previewSavePath, SetupText));
            previewDone[idx] = true;
            completed++;
        }

        // 本体音源ダウンロード用コルーチン
        IEnumerator DownloadAudio(int idx)
        {
            string musicName = Musics[idx];
            string encodedMusicName = Uri.EscapeDataString(musicName);
            string safeFileName = string.Join("_", musicName.Split(Path.GetInvalidFileNameChars()));
            string audioUrl = $"{baseUrl}/getAudio/?chart={encodedMusicName}";
            string audioSavePath = Path.Combine(musicPath, safeFileName + ".wav");
            if (SetupText != null)
                SetupText.GetComponent<TextMeshProUGUI>().text = $"音源ダウンロード中... ({idx + 1}/{total})\n{musicName}";
            yield return StartCoroutine(DownloadAndSaveFileCoroutineWithSpeed(audioUrl, audioSavePath, SetupText));
            audioDone[idx] = true;
            completed++;
        }

        // 全曲一括でコルーチン起動
        for (int i = 0; i < total; i++)
        {
            running.Add(StartCoroutine(DownloadPreview(i)));
            running.Add(StartCoroutine(DownloadAudio(i)));
        }

        // 全てのダウンロードが終わるまで待機
        while (previewDone.Any(x => !x) || audioDone.Any(x => !x))
        {
            if (SetupText != null)
            {
                SetupText.GetComponent<TextMeshProUGUI>().text = $"音源・プレビュー同時ダウンロード中... ({completed}/{total * 2})";
            }
            yield return null;
        }

        if (SetupText != null)
        {
            SetupText.GetComponent<TextMeshProUGUI>().text = "全てのダウンロードが完了しました。";
        }
        Debug.Log("[DownloadAll] Finished downloading all music data.");
        yield return new WaitForSeconds(2); // 完了メッセージを2秒表示
        if (SetupLoading != null) SetupLoading.SetActive(false);
    }

    /// <summary>
    /// 指定されたURLからファイルをダウンロードし、指定されたパスに保存するコルーチン。
    /// </summary>
    /// <param name="url">ダウンロード元のURL</param>
    /// <param name="savePath">保存先のフルパス</param>
    private IEnumerator DownloadAndSaveFileCoroutine(string url, string savePath)
    {
        // 既にファイルが存在する場合はスキップ（再ダウンロードは別途実装）
        if (File.Exists(savePath))
        {
            // Debug.Log($"File already exists, skipping: {savePath}");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            // タイムアウトを長めに設定（大きいファイル対策）
            www.timeout = 120;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // バイナリデータとして保存
                    File.WriteAllBytes(savePath, www.downloadHandler.data);
                    Debug.Log($"Successfully downloaded and saved to {savePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to save file to {savePath}: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Failed to download from {url}: {www.error}");
            }
        }
    }

    private IEnumerator FadeIn(float duration)
    {
        previewAudioSource.volume = 0;
        float timer = 0f;
        while (timer < duration)
        {
            previewAudioSource.volume = Mathf.Lerp(0, 1, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        previewAudioSource.volume = 1;
    }

    private IEnumerator FadeOut(float duration)
    {
        float startVolume = previewAudioSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            previewAudioSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        previewAudioSource.volume = 0;
        previewAudioSource.Stop();
        previewAudioSource.clip = null;
    }

    public void StopPreviewAudio()
    {
        if (previewAudioSource != null && previewAudioSource.isPlaying)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOut(adata.previewFadeDuration));
        }
    }

    void Update()
    {
        if (!adata.ready_to_start && adata.Activated)
        {
            last_tap += Time.deltaTime;
            if (!Open && !adata.showResult)
            {
                Setup();
                Anim.SetBool("bOpen", false);
                Open = true;
            }

            if (SetupLoading != null && SetupLoading.activeSelf)
            {
                // ローディング中の処理はSetupProcessCoroutineに集約
            }
            else if (Anim.GetBool("bOpen"))
            {
                // レベル選択画面が開いている時の処理
            }
            else
            {
                bool isMoving = (Moved < MusicObj.Count);
                if (isMoving)
                {
                    ClickTime = 0f;
                    currentDirection = 0;
                    return;
                }

                if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(adata.ControlerFX2)))
                {
                    ProcessMove(1);
                    ClickTime = 0f; currentDirection = 1; return;
                }

                if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(adata.ControlerFX1)))
                {
                    ProcessMove(-1);
                    ClickTime = 0f; currentDirection = -1; return;
                }

                int newDirection = 0;
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(adata.ControlerFX2)) newDirection = 1;
                else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(adata.ControlerFX1)) newDirection = -1;

                if (newDirection != currentDirection)
                {
                    ClickTime = 0f;
                    currentDirection = newDirection;
                }

                if (currentDirection != 0)
                {
                    TempClickTime += Time.deltaTime;
                    LastMove += Time.deltaTime;

                    const float longPressStartThreshold = 0.5f;
                    const float longPressInterval = 0.1f;
                    if (TempClickTime > longPressStartThreshold && LastMove > longPressInterval)
                    {
                        LastMove = 0f;
                        ProcessMove(currentDirection);
                    }
                }
                else
                {
                    TempClickTime = 0f;
                }
            }

            if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(adata.ControlerStart)))
            {
                if (SetupLoading.activeSelf) return;

                if (Anim.GetBool("bOpen"))
                {
                    // レベル選択後の処理
                }
                else
                {
                    if (MusicObj == null || MusicObj.Count == 0) return;
                    ShowDetails.Music = MusicObj[NowPage].name;
                    ShowDetails.LevelExists = LevelExists[NowPage];
                    LoadingText.SetActive(true);
                    Entered = true;
                }
            }

            if (!LoadingText.activeSelf && Entered)
            {
                Anim.SetBool("bOpen", true);
                Entered = false;
            }

            if ((Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(adata.ControlerFX2) && Input.GetKey(adata.ControlerFX1)) || (Input.GetKeyDown(adata.ControlerFX1) && Input.GetKey(adata.ControlerFX2))))
            {
                if (SetupLoading.activeSelf) return;

                if (Anim.GetBool("bOpen"))
                {
                    Anim.SetBool("bOpen", false);
                }
            }
        }
        else
        {
            Open = false;
        }
    }

    private void OnApplicationQuit()
    {
        ChecksumManager.SaveAll();

        if (KeepDownloads != null && KeepDownloads.isOn)
        {
            Debug.Log("ダウンロードデータを保持する設定のため、キャッシュを削除しません。");
            return;
        }

        Debug.Log("キャッシュを削除します。");
        string jacketCachePath = Path.Combine(Application.persistentDataPath, "JacketCache");
        if (Directory.Exists(jacketCachePath))
        {
            try { Directory.Delete(jacketCachePath, true); } catch (Exception e) { Debug.LogError($"Failed to delete JacketCache: {e.Message}"); }
        }

        if (Directory.Exists(previewAudioCachePath))
        {
            try { Directory.Delete(previewAudioCachePath, true); } catch (Exception e) { Debug.LogError($"Failed to delete PreviewAudioCache: {e.Message}"); }
        }

        string audioChecksumPath = Path.Combine(Application.persistentDataPath, "audio_checksums.json");
        if (File.Exists(audioChecksumPath))
        {
            try { File.Delete(audioChecksumPath); } catch (Exception e) { Debug.LogError($"Failed to delete audio_checksums.json: {e.Message}"); }
        }
        string jacketChecksumPath = Path.Combine(Application.persistentDataPath, "jacket_checksums.json");
        if (File.Exists(jacketChecksumPath))
        {
            try { File.Delete(jacketChecksumPath); } catch (Exception e) { Debug.LogError($"Failed to delete jacket_checksums.json: {e.Message}"); }
        }
        //楽曲リストのキャッシュも削除
        string musicListCachePath = Path.Combine(Application.persistentDataPath, "music_list.json");
        if (File.Exists(musicListCachePath))
        {
            try { File.Delete(musicListCachePath); } catch (Exception e) { Debug.LogError($"Failed to delete music_list.json: {e.Message}"); }
        }


        adata.previewAudioCache.Clear();
    }

    // --- 追加: ダウンロード進捗・速度・ETA・%表示付きコルーチン ---
    private IEnumerator DownloadAndSaveFileCoroutineWithSpeed(string url, string savePath, GameObject setupTextObj)
    {
        if (File.Exists(savePath))
            yield break;

        // 楽曲名をURLから抽出（API仕様に依存）
        string songName = null;
        if (url.Contains("/getAudio/?chart="))
        {
            var uri = new Uri(url);
            var query = WebUtility.UrlDecode(uri.Query);
            songName = query.Substring(query.IndexOf("chart=") + 6);
        }

        long totalLength = 0;
        if (!string.IsNullOrEmpty(songName))
        {
            string durationUrl = $"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getDuration/?bgm={Uri.EscapeDataString(songName)}";
            using (UnityWebRequest durReq = UnityWebRequest.Get(durationUrl))
            {
                durReq.timeout = 10;
                yield return durReq.SendWebRequest();
                if (durReq.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var json = Newtonsoft.Json.Linq.JObject.Parse(durReq.downloadHandler.text);
                        totalLength = json["length"]?.Value<long>() ?? 0;
                    }
                    catch { totalLength = 0; }
                }
            }
        }

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.timeout = 120;
            float startTime = Time.realtimeSinceStartup;
            ulong lastDownloaded = 0;
            float lastTime = startTime;
            float speed = 0f;
            float eta = 0f;
            float percent = 0f;

            www.SendWebRequest();

            while (!www.isDone)
            {
                float now = Time.realtimeSinceStartup;
                ulong downloaded = www.downloadedBytes;
                float deltaTime = now - lastTime;
                if (deltaTime > 0.2f)
                {
                    ulong deltaBytes = downloaded - lastDownloaded;
                    speed = deltaBytes / deltaTime; // bytes/sec
                    lastDownloaded = downloaded;
                    lastTime = now;

                    float progress = www.downloadProgress;
                    if (progress > 0f && speed > 0f)
                    {
                        ulong remaining = (ulong)((1f - progress) * (downloaded / progress));
                        eta = remaining / speed;
                    }

                    if (totalLength > 0)
                    {
                        percent = Mathf.Clamp01((float)downloaded / totalLength) * 100f;
                    }
                    else
                    {
                        percent = progress * 100f;
                    }

                    if (setupTextObj != null)
                    {
                        var text = setupTextObj.GetComponent<TextMeshProUGUI>();
                        if (text != null)
                        {
                            string[] lines = text.text.Split('\n');
                            string baseMsg = lines.Length > 1 ? lines[0] + "\n" + lines[1] : text.text;
                            if (!url.Contains("/getChart"))
                            {
                                text.text = $"{baseMsg}\n{percent:F1}% 速度: {FormatBytes(speed)}/s  ETA: {eta:F1}秒";
                            }
                        }
                    }
                }
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    File.WriteAllBytes(savePath, www.downloadHandler.data);
                    Debug.Log($"Successfully downloaded and saved to {savePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to save file to {savePath}: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Failed to download from {url}: {www.error}");
            }
        }
    }

    // --- 追加: バイト数を人間が読みやすい単位に変換 ---
    private string FormatBytes(float bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        while (bytes >= 1024f && order < sizes.Length - 1)
        {
            order++;
            bytes = bytes / 1024f;
        }
        return $"{bytes:F1} {sizes[order]}";
    }
}