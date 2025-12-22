using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowDetails : MonoBehaviour
{
    public static string Music;
    public static List<bool> LevelExists = new List<bool>();

    // �ω��m�F�p
    private string tMusic;

    // �e��I�u�W�F�N�g
    public GameObject Jacket;
    public GameObject MusicTitle;
    public GameObject Composer;
    public GameObject Charter;
    // ���x����
    public GameObject ELvl;
    public GameObject HLvl;
    public GameObject MLvl;
    public GameObject ALvl;
    // �{�^��
    public Button EBtn;
    public Button HBtn;
    public Button MBtn;
    public Button ABtn;
    public Button StartBtn;
    public Toggle AutoplayBtn;
    // Json�p�ϐ�
    private JObject jsonObj;
    private string loadjson;
    // ���̑��ϐ�
    private int currentDifficultyIndex = 0;
    private bool oo = false;
    private string[] difficulties = { "Another", "Master", "Hard", "Easy" };
    public Animator Anim;
    public GameObject canvas;
    public GameObject PlayerUI;
    private string jacketCachePath;

    // PlayerUI�p�f�[�^
    public GameObject PUIJacket;
    public GameObject PUILevel;
    public GameObject PUIMusic;
    public GameObject PUIComposer;

    // ���[�f�B���O�̂���
    public GameObject LoadingText;
    public static bool getted;
    private string nmusic;
    private string ttmusic;
    public static Sprite jacket;
    private bool start;
    private bool isAutoPlay;
    public static bool alreadyRequest = false;

    // ���ǉ�: UI�R���g���[���[�ւ̎Q��
    private UI uiController;

    private void Awake()
    {
        jacketCachePath = Path.Combine(Application.persistentDataPath, "JacketCache");
        if (!Directory.Exists(jacketCachePath))
        {
            Directory.CreateDirectory(jacketCachePath);
            Debug.Log($"[JacketCache] Created cache directory at: {jacketCachePath}");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // ���ǉ�: UI�R���g���[���[�̃C���X�^���X���擾
        uiController = FindObjectOfType<UI>();

        // �{�^���� onClick ���X�i�[
        EBtn.onClick.AddListener(() => { UpdateUI("Easy"); });
        HBtn.onClick.AddListener(() => { UpdateUI("Hard"); });
        MBtn.onClick.AddListener(() => { UpdateUI("Master"); });
        ABtn.onClick.AddListener(() => { UpdateUI("Another"); });

        UpdateUI(); // ������Ԃ� UI ���X�V

        UpdateButtonColor(EBtn, "Normal");
        UpdateButtonColor(HBtn, "Normal");
        UpdateButtonColor(MBtn, "Normal");
        UpdateButtonColor(ABtn, "Normal");
        Charter.GetComponent<TextMeshProUGUI>().text = "���ʐ���F";
        MusicTitle.GetComponent<TextMeshProUGUI>().text = Music;
    }

    void OnEBtnClick() { } // ��̃��\�b�h
    void OnHBtnClick() { }
    void OnMBtnClick() { }
    void OnABtnClick() { }

    private Dictionary<string, Color32> difficultyColors = new Dictionary<string, Color32>
    {
        { "Another", new Color32(0xb2, 0x00, 0x18, 0xff) }, // #B20018
        { "Master", new Color32(0xd2, 0x00, 0xff, 0xff) },   // #D200FF
        { "Hard", new Color32(0xff, 0x87, 0x00, 0xff) },     // #FF8700
        { "Easy", new Color32(0x00, 0xff, 0x02, 0xff) },     // #00FF02
        { "Normal", new Color32(0xff, 0xff, 0xff, 0xff) }
    };

    private void SetMusicInfo(bool isAutoPlay)
    {
        // ���ǉ�: �v���r���[�Đ����~
        if (uiController != null)
        {
            uiController.StopPreviewAudio();
        }

        string chart = difficulties[currentDifficultyIndex] + "/" + Music;
        adata.chart = chart;

        string diff = chart.Split('/')[0];
        string cn = chart.Split('/')[1];

        adata.Difficulty = diff;

        if (adata.isOfflineMode)
        {
            string jsonPath = Path.Combine(adata.chartsPath, diff, cn + ".json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSON file not found: {jsonPath}");
                return;
            }
            loadjson = File.ReadAllText(jsonPath, Encoding.UTF8);
            if (loadjson != null)
            {
                JObject jsonObj = JObject.Parse(loadjson);
                adata.jsonObj = jsonObj;
                adata.LoadChartData(jsonObj);
                JToken maindata = jsonObj["maindata"];
                if (jsonObj["maindata"] != null)
                {
                    PUIMusic.GetComponent<TextMeshProUGUI>().text = Music;
                    PUIComposer.GetComponent<TextMeshProUGUI>().text = jsonObj["maindata"]["composer"]?.ToString() ?? "Unknown";
                    PUIJacket.GetComponent<Image>().sprite = jacket;
                    PUILevel.GetComponent<TextMeshProUGUI>().text = jsonObj["maindata"]["level"] != null ? "Lv. " + jsonObj["maindata"]["level"].ToString() : "Lv. Unknown";
                }
            }
        }
        else
        {
            var client = new RestClient($"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getDuration/index.cgi?bgm={Uri.EscapeDataString(cn)}");
            RestRequest request = new RestRequest
            {
                Method = Method.Get
            };
            request.Timeout = TimeSpan.FromMinutes(10);
            var response = client.Execute(request);
            string httpLoadjson = response.Content;
            JObject jObj = JObject.Parse(httpLoadjson);
            adata.length = float.Parse(jObj["duration"] + "");

            client = new RestClient("https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getChart/index.cgi");
            request = new RestRequest
            {
                Method = Method.Get
            };
            request.Timeout = TimeSpan.FromMinutes(10);
            request.AddParameter("diff", diff);
            request.AddParameter("chart", cn);
            response = client.Execute(request);
            Debug.Log(response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string loadjson = response.Content;
                if (loadjson != null)
                {
                    JObject jsonObj = JObject.Parse(loadjson);
                    adata.jsonObj = jsonObj;
                    adata.LoadChartData(jsonObj);
                    JToken maindata = jsonObj["maindata"];

                    if (jsonObj["maindata"] != null)
                    {
                        PUIMusic.GetComponent<TextMeshProUGUI>().text = Music;
                        PUIComposer.GetComponent<TextMeshProUGUI>().text = jsonObj["maindata"]["composer"]?.ToString() ?? "Unknown";
                        PUIJacket.GetComponent<Image>().sprite = jacket;
                        PUILevel.GetComponent<TextMeshProUGUI>().text = jsonObj["maindata"]["level"] != null ? "Lv. " + jsonObj["maindata"]["level"].ToString() : "Lv. Unknown";
                    }
                }

                if (!isAutoPlay)
                {
                    StartBtn.interactable = true;
                }
            }
            else
            {
                Debug.LogError("���N�G�X�g�Ɏ��s���܂����B�Ď��s���Ă��܂��B");
                SetMusicInfo(isAutoPlay);
            }
        }
    }

    // Update is called once per frame
    async void Update()
    {
        if (!getted && LoadingText.activeSelf && !alreadyRequest)
        {
            if (adata.isOfflineMode || adata.keepDownloads)
            {
                byte[] fileData = null;
                // �_�E�����[�h�f�[�^����W���P�b�g��ǂݍ���
                if (adata.isOfflineMode)
                {
                    try
                    {
                        fileData = File.ReadAllBytes(Path.Combine(adata.jacketPath, Music + ".jpg"));
                    }
                    catch (Exception e)
                    {
                        fileData = File.ReadAllBytes(Path.Combine(adata.jacketPath, Music + ".png"));
                    }
                }
                else if (adata.keepDownloads)
                {
                    try
                    {
                        fileData = File.ReadAllBytes(Path.Combine(jacketCachePath, Music + ".jpg"));
                    }
                    catch (Exception e)
                    {
                        fileData = File.ReadAllBytes(Path.Combine(jacketCachePath, Music + ".png"));
                    }
                }
                if (fileData == null)
                {
                    Debug.LogWarning($"[SetText] Jacket for '{Music}' not found in offline mode or cache keep mode. No jacket will be displayed.");
                    return;
                }
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(fileData);
                texture.filterMode = FilterMode.Bilinear;
                texture.Apply();
                jacket = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Jacket.GetComponent<Image>().sprite = jacket;

                ABtn.interactable = LevelExists[0];
                MBtn.interactable = LevelExists[1];
                HBtn.interactable = LevelExists[2];
                EBtn.interactable = LevelExists[3];

                UpdateLevelDisplay("Another", ALvl, LevelExists[0]);
                UpdateLevelDisplay("Master", MLvl, LevelExists[1]);
                UpdateLevelDisplay("Hard", HLvl, LevelExists[2]);
                UpdateLevelDisplay("Easy", ELvl, LevelExists[3]);

                getted = true;
                LoadingText.SetActive(false);

                UpdateComposerDisplay();
                MusicTitle.GetComponent<TextMeshProUGUI>().text = Music;
            }
            else
            {
                Debug.Log("�W���P�b�g�摜�̎擾");
                //alreadyRequest = true;
                if (adata.isOfflineMode || adata.keepDownloads)
                {
                    Debug.LogWarning("[SetText] Offline mode or cache keep mode is enabled. Skipping jacket download.");
                    if (File.Exists(Path.Combine(jacketCachePath, Music + ".jpg")))
                    {
                        // �L���b�V������W���P�b�g��ǂݍ���
                        byte[] fileData = File.ReadAllBytes(Path.Combine(jacketCachePath, Music + ".jpg"));
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        texture.LoadImage(fileData);
                        if (Jacket == null) return;
                        texture.filterMode = FilterMode.Bilinear;
                        texture.Apply();
                        Sprite jacket = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        Jacket.GetComponent<Image>().sprite = jacket;
                    }
                    else if (File.Exists(Path.Combine(jacketCachePath, Music + ".png")))
                    {
                        // �L���b�V������W���P�b�g��ǂݍ���
                        byte[] fileData = File.ReadAllBytes(Path.Combine(jacketCachePath, Music + ".png"));
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        texture.LoadImage(fileData);
                        if (Jacket == null) return;
                        texture.filterMode = FilterMode.Bilinear;
                        texture.Apply();
                        Sprite jacket = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        Jacket.GetComponent<Image>().sprite = jacket;
                    }
                    else
                    {
                        Debug.LogWarning($"[SetText] Jacket for '{Music}' not found in cache. No jacket will be displayed.");
                    }
                }
                else
                {
                    var jacketClient = new RestClient($"https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/getJacket/index.cgi?chart={Uri.EscapeDataString(Music)}");
                    RestRequest jacketRequest = new RestRequest
                    {
                        Method = Method.Get
                    };

                    var jacketResponse = await jacketClient.ExecuteAsync(jacketRequest);

                    if (jacketResponse.RawBytes != null && jacketResponse.RawBytes.Length > 0)
                    {
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        texture.LoadImage(jacketResponse.RawBytes);
                        texture.filterMode = FilterMode.Bilinear;
                        texture.Apply();
                        jacket = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        Jacket.GetComponent<Image>().sprite = jacket;
                        Debug.Log("Update");

                        ABtn.interactable = LevelExists[0];
                        MBtn.interactable = LevelExists[1];
                        HBtn.interactable = LevelExists[2];
                        EBtn.interactable = LevelExists[3];

                        UpdateLevelDisplay("Another", ALvl, LevelExists[0]);
                        UpdateLevelDisplay("Master", MLvl, LevelExists[1]);
                        UpdateLevelDisplay("Hard", HLvl, LevelExists[2]);
                        UpdateLevelDisplay("Easy", ELvl, LevelExists[3]);

                        getted = true;
                        LoadingText.SetActive(false);

                        UpdateComposerDisplay();
                        MusicTitle.GetComponent<TextMeshProUGUI>().text = Music;
                    }
                    else
                    {
                        Debug.LogWarning("�W���P�b�g�摜���擾�ł��܂���ł����B");
                    }
                }
            }
        }

        if (Anim.GetBool("bOpen"))
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(adata.ControlerFX2) || Input.GetKeyDown(adata.ControlerFX1))
            {
                int direction = Input.GetKeyDown(KeyCode.RightArrow) ? -1 : Input.GetKeyDown(KeyCode.LeftArrow) ? 1 : Input.GetKeyDown(adata.ControlerFX2) ? -1 : 1;
                int nextIndex = (currentDifficultyIndex + direction + difficulties.Length) % difficulties.Length;

                int initialIndex = nextIndex;
                while (!LevelExists[nextIndex])
                {
                    nextIndex = (nextIndex + direction + difficulties.Length) % difficulties.Length;
                    if (nextIndex == initialIndex) break;
                }

                currentDifficultyIndex = nextIndex;
                UpdateUI();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UpdateButtonColor(EBtn, "Normal");
                UpdateButtonColor(HBtn, "Normal");
                UpdateButtonColor(MBtn, "Normal");
                UpdateButtonColor(ABtn, "Normal");

                Charter.GetComponent<TextMeshProUGUI>().text = "���ʐ���F";
                MusicTitle.GetComponent<TextMeshProUGUI>().text = Music;

                oo = false;
                getted = false;
                alreadyRequest = false;
            }

            if (oo)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(adata.ControlerStart))
                {
                    oo = false;
                    start = false;
                    Anim.SetBool("bOpen", false);

                    isAutoPlay = (AutoplayBtn.isOn || ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))));
                    if (isAutoPlay)
                    {
                        adata.auto_play = true;
                        SetMusicInfo(true);
                    }
                    else
                    {
                        adata.auto_play = false;
                        SetMusicInfo(false);
                    }
                    PlayerUI.SetActive(true);
                    canvas.GetComponent<CanvasGroup>().alpha = 0.0f;

                    UpdateButtonColor(EBtn, "Normal");
                    UpdateButtonColor(HBtn, "Normal");
                    UpdateButtonColor(MBtn, "Normal");
                    UpdateButtonColor(ABtn, "Normal");

                    Charter.GetComponent<TextMeshProUGUI>().text = "���ʐ���F";
                    MusicTitle.GetComponent<TextMeshProUGUI>().text = Music;

                    alreadyRequest = false;
                    adata.ready_to_start = false;
                    adata.start_dl = true;
                    PlayMusic.settedUp = false;
                    PlayMusic.audioDownloaded = false;
                    PlayMusic.audioFinished = false;
                    PlayMusic.time = 0;
                    PlayMusic.played = false;
                    getted = false;
                }
            }
        }
    }

    private void UpdateUI(string difficulty = null)
    {
        if (difficulty != null)
        {
            currentDifficultyIndex = System.Array.IndexOf(difficulties, difficulty);
        }

        if (currentDifficultyIndex == 3)
        {
            UpdateButtonColor(HBtn, "Normal");
            UpdateButtonColor(MBtn, "Normal");
            UpdateButtonColor(ABtn, "Normal");
            UpdateButtonColor(EBtn, "Easy");
        }
        else if (currentDifficultyIndex == 2)
        {
            UpdateButtonColor(EBtn, "Normal");
            UpdateButtonColor(MBtn, "Normal");
            UpdateButtonColor(ABtn, "Normal");
            UpdateButtonColor(HBtn, "Hard");
        }
        else if (currentDifficultyIndex == 1)
        {
            UpdateButtonColor(EBtn, "Normal");
            UpdateButtonColor(HBtn, "Normal");
            UpdateButtonColor(ABtn, "Normal");
            UpdateButtonColor(MBtn, "Master");
        }
        else if (currentDifficultyIndex == 0)
        {
            UpdateButtonColor(EBtn, "Normal");
            UpdateButtonColor(HBtn, "Normal");
            UpdateButtonColor(MBtn, "Normal");
            UpdateButtonColor(ABtn, "Another");
        }

        UpdateLevelDisplay(difficulties[currentDifficultyIndex], GetLevelObject(currentDifficultyIndex), LevelExists[currentDifficultyIndex]);
        UpdateCharterDisplay(difficulties[currentDifficultyIndex]);

        oo = true;
    }

    private void UpdateButtonColor(Button button, string difficulty)
    {
        // This method seems to have an issue in its original implementation.
        // It should probably compare against the *button's* associated difficulty, not the current global one.
        // However, for now, preserving the logic but making it safer.
        Color targetColor = difficultyColors.ContainsKey(difficulty) ? difficultyColors[difficulty] : Color.white;
        ColorBlock cb = button.colors;
        cb.normalColor = targetColor;
        button.colors = cb;
    }


    private GameObject GetLevelObject(int difficultyIndex)
    {
        switch (difficultyIndex)
        {
            case 0: return ALvl;
            case 1: return MLvl;
            case 2: return HLvl;
            case 3: return ELvl;
            default: return null;
        }
    }

    private void UpdateLevelDisplay(string difficulty, GameObject levelObject, bool levelExists)
    {
        if (levelExists)
        {
            var levels = new List<string> { "Another", "Master", "Hard", "Easy" };
            List<string> musics = (adata.musicsJson["charts"] as JArray).ToObject<List<string>>();
            levelObject.GetComponent<TextMeshProUGUI>().text = "Lv. " + (adata.musicsJson["levels"][musics.IndexOf(Music)][levels.IndexOf(difficulty)]);
        }
        else
        {
            levelObject.GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    private void UpdateComposerDisplay()
    {
        for (int i = 0; i < LevelExists.Count; i++)
        {
            if (LevelExists[i])
            {
                List<string> musics = (adata.musicsJson["charts"] as JArray).ToObject<List<string>>();
                Composer.GetComponent<TextMeshProUGUI>().text = adata.musicsJson["composers"][musics.IndexOf(Music)].ToString();
                return; // 1��������I��
            }
        }
    }

    private void UpdateCharterDisplay(string difficulty)
    {
        var levels = new List<string> { "Another", "Master", "Hard", "Easy" };
        List<string> musics = (adata.musicsJson["charts"] as JArray).ToObject<List<string>>();
        Charter.GetComponent<TextMeshProUGUI>().text = "���ʐ���: " + adata.musicsJson["charters"][musics.IndexOf(Music)][levels.IndexOf(difficulty)];
    }
}
