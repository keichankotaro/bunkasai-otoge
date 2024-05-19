using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;

public class UI : MonoBehaviour
{
    public GameObject canvas;
    public GameObject MusicPrefab;
    public GameObject PlayerUI;
    private List<GameObject> another_Musics;
    private List<GameObject> master_Musics;
    private List<GameObject> hard_Musics;
    private List<GameObject> easy_Musics;
    private int easy_now = 0;
    private int hard_now = 0;
    private int master_now = 0;
    private int another_now = 0;
    private bool Open = false;
    private bool changed = false;
    private string[] another_files = {};
    private string[] master_files = {};
    private string[] hard_files = {};
    private string[] easy_files = {};
    private int level = 0;
    public TextMeshProUGUI level_text;
    public Button upButton;
    public Button downButton;
    public Button rightButton;
    public Button leftButton;
    public Button startButton;
    public Toggle autoplayButton;
    private bool autoplay = false;
    private float last_tap;
    private float tap_chattering = 0.01f;
    private string composer;
    private string music;
    private string level_label;
    // 曲名とかを表示させるための変数宣言
    public GameObject MusicName;
    public GameObject Composer;
    public GameObject MusicJacket;
    public GameObject Level;

    private void OnUpButtonClick()
    {
        if (last_tap >= tap_chattering)
        {
            if (level < 3)
            {
                level++;
                level_text.text = (level == 0) ? "Easy" : (level == 1) ? "Hard" : (level == 2) ? "Master" : "Another";
                level_text.color = (level == 0) ? new Color32(0, 255, 0, 255) : (level == 1) ? new Color32(255, 165, 0, 255) : (level == 2) ? new Color32(255, 0, 255, 255) : new Color32(0, 0, 0, 255);
                UpdateLevelsPositions();
            }
        }
        last_tap = 0.0f;
    }

    private void OnDownButtonClick()
    {
        if (last_tap >= tap_chattering)
        {
            if (level > 0)
            {
                level--;
                level_text.text = (level == 0) ? "Easy" : (level == 1) ? "Hard" : (level == 2) ? "Master" : "Another";
                level_text.color = (level == 0) ? new Color32(0, 255, 0, 255) : (level == 1) ? new Color32(255, 165, 0, 255) : (level == 2) ? new Color32(255, 0, 255, 255) : new Color32(0, 0, 0, 255);
                UpdateLevelsPositions();
            }
        }
        last_tap = 0.0f;
    }

    private void OnRightButtonClick()
    {
        if (last_tap >= tap_chattering)
        {
            MoveMusicSelection(1);
        }
        last_tap = 0.0f;
    }

    private void OnLeftButtonClick()
    {
        if (last_tap >= tap_chattering) {
            MoveMusicSelection(-1);
        }
        last_tap = 0.0f;
    }

    private void OnStartButtonClick()
    {
        if (last_tap >= tap_chattering)
        {
            if (autoplay)
            {
                adata.auto_play = true;
                canvas.SetActive(false);
                PlayerUI.SetActive(true);
                ScoreManeger.score = 0;
                ScoreManeger.combo = 0;
                if (level == 0)
                {
                    adata.chart = easy_files[easy_now];
                    string chart_name = name;
                    JObject jsonObj;
                    string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                    try
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label= "Lv. Unknown";
                        }
                    }
                    catch
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    MusicName.GetComponent<TextMeshProUGUI>().text = music;
                    Composer.GetComponent<TextMeshProUGUI>().text = composer;
                    Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                    MusicJacket.GetComponent<Image>().sprite = Jacket;
                    Level.GetComponent<TextMeshProUGUI>().text = level_label;
                }
                else if (level == 1)
                {
                    adata.chart = hard_files[hard_now];
                    string chart_name = name;
                    JObject jsonObj;
                    string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                    try
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    catch
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    MusicName.GetComponent<TextMeshProUGUI>().text = music;
                    Composer.GetComponent<TextMeshProUGUI>().text = composer;
                    Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                    MusicJacket.GetComponent<Image>().sprite = Jacket;
                    Level.GetComponent<TextMeshProUGUI>().text = level_label;
                }
                else if (level == 2)
                {
                    adata.chart = master_files[master_now];
                    string chart_name = name;
                    JObject jsonObj;
                    string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                    try
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    catch
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    MusicName.GetComponent<TextMeshProUGUI>().text = music;
                    Composer.GetComponent<TextMeshProUGUI>().text = composer;
                    Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                    MusicJacket.GetComponent<Image>().sprite = Jacket;
                    Level.GetComponent<TextMeshProUGUI>().text = level_label;
                }
                else if (level == 3)
                {
                    adata.chart = another_files[another_now];
                    string chart_name = name;
                    JObject jsonObj;
                    string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                    try
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    catch
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    MusicName.GetComponent<TextMeshProUGUI>().text = music;
                    Composer.GetComponent<TextMeshProUGUI>().text = composer;
                    Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                    MusicJacket.GetComponent<Image>().sprite = Jacket;
                    Level.GetComponent<TextMeshProUGUI>().text = level_label;
                }

                for (int i = 0; i < easy_Musics.Count; i++)
                {
                    Destroy(easy_Musics[i]);
                }
                for (int i = 0; i < hard_Musics.Count; i++)
                {
                    Destroy(hard_Musics[i]);
                }
                for (int i = 0; i < master_Musics.Count; i++)
                {
                    Destroy(master_Musics[i]);
                }
                for (int i = 0; i < another_Musics.Count; i++)
                {
                    Destroy(another_Musics[i]);
                }
                easy_now = 0;
                hard_now = 0;
                master_now = 0;
                another_now = 0;
                adata.ready_to_start = true;
            }
            else
            {
                adata.auto_play = false;
                canvas.SetActive(false);
                PlayerUI.SetActive(true);
                ScoreManeger.score = 0;
                ScoreManeger.combo = 0;
                if (level == 0)
                {
                    adata.chart = easy_files[easy_now];
                    string chart_name = name;
                    JObject jsonObj;
                    string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                    try
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    catch
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    MusicName.GetComponent<TextMeshProUGUI>().text = music;
                    Composer.GetComponent<TextMeshProUGUI>().text = composer;
                    Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                    MusicJacket.GetComponent<Image>().sprite = Jacket;
                    Level.GetComponent<TextMeshProUGUI>().text = level_label;
                }
                else if (level == 1)
                {
                    adata.chart = hard_files[hard_now];
                    string chart_name = name;
                    JObject jsonObj;
                    string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                    try
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    catch
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    MusicName.GetComponent<TextMeshProUGUI>().text = music;
                    Composer.GetComponent<TextMeshProUGUI>().text = composer;
                    Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                    MusicJacket.GetComponent<Image>().sprite = Jacket;
                    Level.GetComponent<TextMeshProUGUI>().text = level_label;
                }
                else if (level == 2)
                {
                    adata.chart = master_files[master_now];
                    string chart_name = name;
                    JObject jsonObj;
                    string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                    try
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    catch
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    MusicName.GetComponent<TextMeshProUGUI>().text = music;
                    Composer.GetComponent<TextMeshProUGUI>().text = composer;
                    Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                    MusicJacket.GetComponent<Image>().sprite = Jacket;
                    Level.GetComponent<TextMeshProUGUI>().text = level_label;
                }
                else if (level == 3)
                {
                    adata.chart = another_files[another_now];
                    string chart_name = name;
                    JObject jsonObj;
                    string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                    try
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    catch
                    {
                        jsonObj = JObject.Parse(loadjson);

                        if (jsonObj["maindata"]["composer"] + "" != "")
                        {
                            composer = jsonObj["maindata"]["composer"] + "";
                        }
                        else
                        {
                            composer = "Unknown";
                        }

                        music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                        if (jsonObj["maindata"]["level"] + "" != "")
                        {
                            level_label = "Lv. " + jsonObj["maindata"]["level"];
                        }
                        else
                        {
                            level_label = "Lv. Unknown";
                        }
                    }
                    MusicName.GetComponent<TextMeshProUGUI>().text = music;
                    Composer.GetComponent<TextMeshProUGUI>().text = composer;
                    Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                    MusicJacket.GetComponent<Image>().sprite = Jacket;
                    Level.GetComponent<TextMeshProUGUI>().text = level_label;
                }

                for (int i = 0; i < easy_Musics.Count; i++)
                {
                    Destroy(easy_Musics[i]);
                }
                for (int i = 0; i < hard_Musics.Count; i++)
                {
                    Destroy(hard_Musics[i]);
                }
                for (int i = 0; i < master_Musics.Count; i++)
                {
                    Destroy(master_Musics[i]);
                }
                for (int i = 0; i < another_Musics.Count; i++)
                {
                    Destroy(another_Musics[i]);
                }
                easy_now = 0;
                hard_now = 0;
                master_now = 0;
                another_now = 0;
                adata.ready_to_start = true;
            }
        }
        last_tap = 0.0f;
    }

    private void OnAutoPlayButtonChange(bool mode)
    {
        autoplay = mode;
    }

    
    private void Setup()
    {
        canvas.SetActive(true);
        PlayerUI.SetActive(false);
        adata.ready_to_start = false;
        another_files = new string[] { };
        master_files = new string[] { };
        hard_files = new string[] { };
        easy_files = new string[] { };
        another_Musics = new List<GameObject>();
        master_Musics = new List<GameObject>();
        hard_Musics = new List<GameObject>();
        easy_Musics = new List<GameObject>();

        var ano_obj = Resources.LoadAll("Charts/Another");
        var mas_obj = Resources.LoadAll("Charts/Master");
        var hrd_obj = Resources.LoadAll("Charts/Hard");
        var eas_obj = Resources.LoadAll("Charts/Easy");

        foreach (var m in ano_obj)
        {
            Array.Resize(ref another_files, another_files.Length+1);
            another_files[another_files.Length - 1] = "Another/"+m.name;
        }
        
        foreach (var o in mas_obj)
        {
            //////Debug.LogError(o.name);
            Array.Resize(ref master_files, master_files.Length + 1);
            master_files[master_files.Length-1] = "Master/"+o.name;
        }

        foreach (var p in hrd_obj)
        {
            Array.Resize(ref hard_files, hard_files.Length + 1);
            hard_files[hard_files.Length - 1] = "Hard/"+p.name;
        }

        foreach (var q in eas_obj)
        {
            Array.Resize(ref easy_files, easy_files.Length + 1);
            easy_files[easy_files.Length - 1] = "Easy/"+q.name;
        }

        for (int i = 0; i < another_files.Length; i++)
        {
            another_Musics.Add((GameObject)Instantiate(MusicPrefab));
            another_Musics[i].transform.parent = canvas.transform;
            //Musics[i].transform.SetParent(canvas.transform, false);
            Vector3 pos = another_Musics[i].transform.position;
            pos.x = 0.0f + 500.0f * i;
            pos.y = 3000.0f;
            pos.z = -0.1f;
            another_Musics[i].transform.position = pos;
            another_Musics[i].name = another_files[i];
            another_Musics[i].transform.SetSiblingIndex(1);
        }

        for (int i = 0; i < master_files.Length; i++)
        {
            master_Musics.Add((GameObject)Instantiate(MusicPrefab));
            master_Musics[i].transform.parent = canvas.transform;
            //Musics[i].transform.SetParent(canvas.transform, false);
            Vector3 pos = master_Musics[i].transform.position;
            pos.x = 0.0f + 500.0f * i;
            pos.y = 2000.0f;
            pos.z = -0.1f;
            master_Musics[i].transform.position = pos;
            master_Musics[i].name = master_files[i];
            master_Musics[i].transform.SetSiblingIndex(1);
        }

        for (int i = 0; i < hard_files.Length; i++)
        {
            hard_Musics.Add((GameObject)Instantiate(MusicPrefab));
            hard_Musics[i].transform.parent = canvas.transform;
            //Musics[i].transform.SetParent(canvas.transform, false);
            Vector3 pos = hard_Musics[i].transform.position;
            pos.x = 0.0f + 500.0f * i;
            pos.y = 1000.0f;
            pos.z = -0.1f;
            hard_Musics[i].transform.position = pos;
            hard_Musics[i].name = hard_files[i];
            hard_Musics[i].transform.SetSiblingIndex(1);
        }

        for (int i = 0; i < easy_files.Length; i++)
        {
            easy_Musics.Add((GameObject)Instantiate(MusicPrefab));
            easy_Musics[i].transform.parent = canvas.transform;
            //Musics[i].transform.SetParent(canvas.transform, false);
            Vector3 pos = easy_Musics[i].transform.position;
            pos.x = 0.0f + 500.0f * i;
            pos.y = 0.0f;
            pos.z = -0.1f;
            easy_Musics[i].transform.position = pos;
            easy_Musics[i].name = easy_files[i];
            easy_Musics[i].transform.SetSiblingIndex(1);
        }

        UpdateLevelsPositions();

        // ボタンにクリックイベントを追加
        upButton.onClick.AddListener(OnUpButtonClick);
        downButton.onClick.AddListener(OnDownButtonClick);
        rightButton.onClick.AddListener(OnRightButtonClick);
        leftButton.onClick.AddListener(OnLeftButtonClick);
        startButton.onClick.AddListener(OnStartButtonClick);
        autoplayButton.onValueChanged.AddListener(OnAutoPlayButtonChange);
    }

    private void UpdateLevelsPositions()
    {
        for (int i = 0; i < easy_Musics.Count; i++)
        {
            Vector3 pos = easy_Musics[i].transform.position;
            pos.y = (level == 3) ? -3000.0f : (level == 2) ? -2000.0f : (level == 1) ? -1000.0f : 0.0f;
            easy_Musics[i].transform.position = pos;
            
        }

        for (int i = 0; i < hard_Musics.Count; i++)
        {
            Vector3 pos = hard_Musics[i].transform.position;
            pos.y = (level == 3) ? -2000.0f : (level == 2) ? -1000.0f : (level == 1) ? 0.0f : 1000.0f;
            hard_Musics[i].transform.position = pos;
            //hard_Musics[i].transform.SetSiblingIndex(1);
        }

        for (int i = 0; i < master_Musics.Count; i++)
        {
            Vector3 pos = master_Musics[i].transform.position;
            pos.y = (level == 3) ? -1000.0f : (level == 2) ? 0.0f : (level == 1) ? 1000.0f : 2000.0f;
            master_Musics[i].transform.position = pos;
            //master_Musics[i].transform.SetSiblingIndex(1);
        }

        for (int i = 0; i < another_Musics.Count; i++)
        {
            Vector3 pos = another_Musics[i].transform.position;
            pos.y = (level == 3) ? 0.0f : (level == 2) ? 1000.0f : (level == 1) ? 2000.0f : 3000.0f;
            another_Musics[i].transform.position = pos;
            //another_Musics[i].transform.SetSiblingIndex(1);
        }
    }

    private void MoveMusicSelection(int direction)
    {
        List<GameObject> selectedMusics;

        if (level == 0)
        {
            selectedMusics = easy_Musics;
            if (easy_now + direction >= 0 && easy_now + direction < easy_Musics.Count)
            {
                easy_now += direction;
                ChangeMusicPosition(selectedMusics, direction);
            }
        }
        else if (level == 1)
        {
            selectedMusics = hard_Musics;
            if (hard_now + direction >= 0 && hard_now + direction < hard_Musics.Count)
            {
                hard_now += direction;
                ChangeMusicPosition(selectedMusics, direction);
            }
        }
        else if (level == 2)
        {
            selectedMusics = master_Musics;
            if (master_now + direction >= 0 && master_now + direction < master_Musics.Count)
            {
                master_now += direction;
                ChangeMusicPosition(selectedMusics, direction);
            }
        }
        else if (level == 3)
        {
            selectedMusics = another_Musics;
            if (another_now + direction >= 0 && another_now + direction < another_Musics.Count)
            {
                another_now += direction;
                ChangeMusicPosition(selectedMusics, direction);
            }
        }
    }

    private void ChangeMusicPosition(List<GameObject> musics, int direction)
    {
        for (int i = 0; i < musics.Count; i++)
        {
            Vector3 pos = musics[i].transform.position;
            pos.x = pos.x - 500.0f * direction;
            musics[i].transform.position = pos;
        }
    }

    void Update()
    {
        if (!adata.ready_to_start)
        {
            last_tap += Time.deltaTime;
            if (!Open)
            {
                Setup();
                Open = true;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Joystick1Button10))
            {
                MoveMusicSelection(1);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Joystick1Button11))
            {
                MoveMusicSelection(-1);
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Joystick1Button5))
            {
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    adata.auto_play = true;
                    canvas.SetActive(false);
                    PlayerUI.SetActive(true);

                    if (level == 0)
                    {
                        adata.chart = easy_files[easy_now];
                        string chart_name = name;
                        JObject jsonObj;
                        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                        try
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        catch
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        MusicName.GetComponent<TextMeshProUGUI>().text = music;
                        Composer.GetComponent<TextMeshProUGUI>().text = composer;
                        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                        MusicJacket.GetComponent<Image>().sprite = Jacket;
                        Level.GetComponent<TextMeshProUGUI>().text = level_label;
                    }
                    else if (level == 1)
                    {
                        adata.chart = hard_files[hard_now];
                        string chart_name = name;
                        JObject jsonObj;
                        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                        try
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        catch
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        MusicName.GetComponent<TextMeshProUGUI>().text = music;
                        Composer.GetComponent<TextMeshProUGUI>().text = composer;
                        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                        MusicJacket.GetComponent<Image>().sprite = Jacket;
                        Level.GetComponent<TextMeshProUGUI>().text = level_label;
                    }
                    else if (level == 2)
                    {
                        adata.chart = master_files[master_now];
                        string chart_name = name;
                        JObject jsonObj;
                        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                        try
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        catch
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        MusicName.GetComponent<TextMeshProUGUI>().text = music;
                        Composer.GetComponent<TextMeshProUGUI>().text = composer;
                        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                        MusicJacket.GetComponent<Image>().sprite = Jacket;
                        Level.GetComponent<TextMeshProUGUI>().text = level_label;
                    }
                    else if (level == 3)
                    {
                        adata.chart = another_files[another_now];
                        string chart_name = name;
                        JObject jsonObj;
                        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                        try
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        catch
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        MusicName.GetComponent<TextMeshProUGUI>().text = music;
                        Composer.GetComponent<TextMeshProUGUI>().text = composer;
                        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                        MusicJacket.GetComponent<Image>().sprite = Jacket;
                        Level.GetComponent<TextMeshProUGUI>().text = level_label;
                    }

                    for (int i = 0; i < easy_Musics.Count; i++)
                    {
                        Destroy(easy_Musics[i]);
                    }
                    for (int i = 0; i < hard_Musics.Count; i++)
                    {
                        Destroy(hard_Musics[i]);
                    }
                    for (int i = 0; i < master_Musics.Count; i++)
                    {
                        Destroy(master_Musics[i]);
                    }
                    for (int i = 0; i < another_Musics.Count; i++)
                    {
                        Destroy(another_Musics[i]);
                    }
                    easy_now = 0;
                    hard_now = 0;
                    master_now = 0;
                    another_now = 0;
                    adata.ready_to_start = true;
                }
                else
                {
                    adata.auto_play = false;
                    canvas.SetActive(false);
                    PlayerUI.SetActive(true);

                    if (level == 0)
                    {
                        adata.chart = easy_files[easy_now];
                        string chart_name = name;
                        JObject jsonObj;
                        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                        try
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        catch
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        MusicName.GetComponent<TextMeshProUGUI>().text = music;
                        Composer.GetComponent<TextMeshProUGUI>().text = composer;
                        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                        MusicJacket.GetComponent<Image>().sprite = Jacket;
                        Level.GetComponent<TextMeshProUGUI>().text = level_label;
                    }
                    else if (level == 1)
                    {
                        adata.chart = hard_files[hard_now];
                        string chart_name = name;
                        JObject jsonObj;
                        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                        try
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        catch
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        MusicName.GetComponent<TextMeshProUGUI>().text = music;
                        Composer.GetComponent<TextMeshProUGUI>().text = composer;
                        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                        MusicJacket.GetComponent<Image>().sprite = Jacket;
                        Level.GetComponent<TextMeshProUGUI>().text = level_label;
                    }
                    else if (level == 2)
                    {
                        adata.chart = master_files[master_now];
                        string chart_name = name;
                        JObject jsonObj;
                        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                        try
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        catch
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        MusicName.GetComponent<TextMeshProUGUI>().text = music;
                        Composer.GetComponent<TextMeshProUGUI>().text = composer;
                        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                        MusicJacket.GetComponent<Image>().sprite = Jacket;
                        Level.GetComponent<TextMeshProUGUI>().text = level_label;
                    }
                    else if (level == 3)
                    {
                        adata.chart = another_files[another_now];
                        string chart_name = name;
                        JObject jsonObj;
                        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
                        try
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");

                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        catch
                        {
                            jsonObj = JObject.Parse(loadjson);

                            if (jsonObj["maindata"]["composer"] + "" != "")
                            {
                                composer = jsonObj["maindata"]["composer"] + "";
                            }
                            else
                            {
                                composer = "Unknown";
                            }

                            music = adata.chart.Replace("Easy/", "").Replace("Hard/", "").Replace("Master/", "").Replace("Another/", "");
                            
                            if (jsonObj["maindata"]["level"] + "" != "")
                            {
                                level_label = "Lv. " + jsonObj["maindata"]["level"];
                            }
                            else
                            {
                                level_label = "Lv. Unknown";
                            }
                        }
                        MusicName.GetComponent<TextMeshProUGUI>().text = music;
                        Composer.GetComponent<TextMeshProUGUI>().text = composer;
                        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + music);
                        MusicJacket.GetComponent<Image>().sprite = Jacket;
                        Level.GetComponent<TextMeshProUGUI>().text = level_label;
                    }

                    for (int i = 0; i < easy_Musics.Count; i++)
                    {
                        Destroy(easy_Musics[i]);
                    }
                    for (int i = 0; i < hard_Musics.Count; i++)
                    {
                        Destroy(hard_Musics[i]);
                    }
                    for (int i = 0; i < master_Musics.Count; i++)
                    {
                        Destroy(master_Musics[i]);
                    }
                    for (int i = 0; i < another_Musics.Count; i++)
                    {
                        Destroy(another_Musics[i]);
                    }
                    easy_now = 0;
                    hard_now = 0;
                    master_now = 0;
                    another_now = 0;
                    adata.ready_to_start = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (level > 0)
                {
                    level--;
                    level_text.text = (level == 0) ? "Easy" : (level == 1) ? "Hard" : (level == 2) ? "Master" : "Another";
                    level_text.color = (level == 0) ? new Color32(0, 255, 0, 255) : (level == 1) ? new Color32(255, 165, 0, 255) : (level == 2) ? new Color32(255, 0, 255, 255) : new Color32(0, 0, 0, 255);
                    UpdateLevelsPositions();
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (level < 3)
                {
                    level++;
                    level_text.text = (level == 0) ? "Easy" : (level == 1) ? "Hard" : (level == 2) ? "Master" : "Another";
                    level_text.color = (level == 0) ? new Color32(0, 255, 0, 255) : (level == 1) ? new Color32(255, 165, 0, 255) : (level == 2) ? new Color32(255, 0, 255, 255) : new Color32(0, 0, 0, 255);
                    UpdateLevelsPositions();
                }
            }
        }
        else
        {
            Open = false;
        }
    }

    void OnApplicationQuit()
    {
        //
    }
}