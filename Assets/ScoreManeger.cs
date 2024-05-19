using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;   

public class ScoreManeger : MonoBehaviour
{
    [SerializeField] GameObject score_t;
    [SerializeField] GameObject combo_t;
    public static int score = 0;
    public static int combo = 0;
    public static int notes;
    private int maxscore;
    public static int ratioscore;
    public static bool setupped = false;
    private bool mode = false;
    private double prievous = 0;

    // Start is called before the first frame update
    private void Setup()
    {
        string chart = adata.chart;
        string loadjson = Resources.Load<TextAsset>("Charts/" + chart).ToString();
        notes = 0;
        maxscore = 0;
        ratioscore = 0;

        JObject jsonObj = JObject.Parse(loadjson);
        for (int i = 1; i <= int.Parse(jsonObj["chartdata"]["L1"]["L1data"]["notes"] + ""); i++)
        {
            if (jsonObj["chartdata"]["L1"][i.ToString()]["type"] + "" == "tap")
            {
                notes++;
            }
            else if (jsonObj["chartdata"]["L1"][i.ToString()]["type"] + "" == "long")
            {
                notes++;
                notes++;
            }
        }
        for (int i = 1; i <= int.Parse(jsonObj["chartdata"]["L2"]["L2data"]["notes"] + ""); i++)
        {
            if (jsonObj["chartdata"]["L2"][i.ToString()]["type"] + "" == "tap")
            {
                notes++;
            }
            else if (jsonObj["chartdata"]["L2"][i.ToString()]["type"] + "" == "long")
            {
                notes++;
                notes++;
            }
        }
        for (int i = 1; i <= int.Parse(jsonObj["chartdata"]["L3"]["L3data"]["notes"] + ""); i++)
        {
            if (jsonObj["chartdata"]["L3"][i.ToString()]["type"] + "" == "tap")
            {
                notes++;
            }
            else if (jsonObj["chartdata"]["L3"][i.ToString()]["type"] + "" == "long")
            {
                notes++;
                notes++;
            }
        }
        for (int i = 1; i <= int.Parse(jsonObj["chartdata"]["L4"]["L4data"]["notes"] + ""); i++)
        {
            if (jsonObj["chartdata"]["L4"][i.ToString()]["type"] + "" == "tap")
            {
                notes++;
            }
            else if (jsonObj["chartdata"]["L4"][i.ToString()]["type"] + "" == "long")
            {
                notes++;
                notes++;
            }
        }
        maxscore = notes * 5;
        Debug.Log("MaxCombo: " + notes);
        setupped = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (adata.ready_to_start)
        {
            if (!setupped)
            {
                Setup();
            }
            else
            {
                if (adata.auto_play)
                {
                    /*
                    if (prievous != Math.Floor(adata.game_time))
                    {
                        var t = Math.Floor(adata.game_time);
                        if (t % 5 == 0)
                        {
                            if (mode)
                            {
                                mode = false;
                            }
                            else
                            {
                                mode = true;
                            }
                            prievous = t;
                        }
                    }
                    */
                    ratioscore = (int)Math.Round(1000000 * Math.Floor((double)score / maxscore * 1000000) / 1000000);
                    string s;
                    if (ratioscore.ToString().Length <= 7)
                    {
                        s = new string('0', 7 - ratioscore.ToString().Length) + (ratioscore + "");
                    }
                    else
                    {
                        s = ratioscore.ToString();
                    }

                    /*
                    if (mode)
                    {
                        score_t.GetComponent<TextMeshProUGUI>().text = "AutoPlay";
                        combo_t.GetComponent<TextMeshProUGUI>().text = "AutoPlay";
                    }
                    else
                    {
                        score_t.GetComponent<TextMeshProUGUI>().text = s;
                        combo_t.GetComponent<TextMeshProUGUI>().text = combo + "";
                    }
                    */

                    score_t.GetComponent<TextMeshProUGUI>().text = "Auto " + s;
                    combo_t.GetComponent<TextMeshProUGUI>().text = "Auto " + combo;
                }
                else
                {
                    ratioscore = (int)Math.Round(1000000 * Math.Floor((double)score / maxscore * 1000000) / 1000000);
                    string s;
                    if (ratioscore.ToString().Length <= 7)
                    {
                        s = new string('0', 7 - ratioscore.ToString().Length) + (ratioscore + "");
                    }
                    else
                    {
                        s = ratioscore.ToString();
                    }
                    score_t.GetComponent<TextMeshProUGUI>().text = s;
                    combo_t.GetComponent<TextMeshProUGUI>().text = combo + "";
                }
            }
        }
        else
        {
            setupped = false;
        }
    }
}
