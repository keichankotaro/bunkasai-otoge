using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        notes = 0;
        int l1_notes = 0;
        int l2_notes = 0;
        int l3_notes = 0;
        int l4_notes = 0;
        maxscore = 0;
        ratioscore = 0;

        JObject jsonObj = adata.jsonObj;
        JObject chartdata = (JObject)jsonObj["chartdata"];

        // L1
        JObject l1_chart = (JObject)chartdata["L1"];
        for (int i = 1; i <= l1_chart.Properties().Count() -1; i++)
        {
            if (l1_chart[i.ToString()]["type"] + "" == "tap")
            {
                notes++;
                l1_notes++;
            }
            else if (l1_chart[i.ToString()]["type"] + "" == "long")
            {
                notes += 2;
                l1_notes += 2;
            }
        }

        // L2
        JObject l2_chart = (JObject)chartdata["L2"];
        for (int i = 1; i <= l2_chart.Properties().Count() - 1; i++)
        {
            if (l2_chart[i.ToString()]["type"] + "" == "tap")
            {
                notes++;
                l2_notes++;
            }
            else if (l2_chart[i.ToString()]["type"] + "" == "long")
            {
                notes += 2;
                l2_notes += 2;
            }
        }

        // L3
        JObject l3_chart = (JObject)chartdata["L3"];
        for (int i = 1; i <= l3_chart.Properties().Count() - 1; i++)
        {
            if (l3_chart[i.ToString()]["type"] + "" == "tap")
            {
                notes++;
                l3_notes++;
            }
            else if (l3_chart[i.ToString()]["type"] + "" == "long")
            {
                notes += 2;
                l3_notes += 2;
            }
        }

        // L4
        JObject l4_chart = (JObject)chartdata["L4"];
        for (int i = 1; i <= l4_chart.Properties().Count() - 1; i++)
        {
            if (l4_chart[i.ToString()]["type"] + "" == "tap")
            {
                notes++;
                l4_notes++;
            }
            else if (l4_chart[i.ToString()]["type"] + "" == "long")
            {
                notes += 2;
                l4_notes += 2;
            }
        }
        maxscore = notes * 5;
        Debug.Log("MaxCombo: " + notes);

        if (DebugText.isDebugMode)
        {
            DebugText.max_score = maxscore + "";
            DebugText.max_combo = notes + "";
            DebugText.max_l1 = l1_notes + "";
            DebugText.max_l2 = l2_notes + "";
            DebugText.max_l3 = l3_notes + "";
            DebugText.max_l4 = l4_notes + "";
            DebugText.max_score_l1 = (l1_notes * 5) + "";
            DebugText.max_score_l2 = (l2_notes * 5) + "";
            DebugText.max_score_l3 = (l3_notes * 5) + "";
            DebugText.max_score_l4 = (l4_notes * 5) + "";
        }
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
                ratioscore = (int)Math.Round(1000000 * Math.Floor((double)score / maxscore * 1000000) / 1000000);
                if (ratioscore > 1000000)
                {
                    ratioscore = 1000000;
                }

                string s;
                if (ratioscore.ToString().Length <= 7)
                {
                    s = new string('0', 7 - ratioscore.ToString().Length) + (ratioscore + "");
                }
                else
                {
                    s = ratioscore.ToString();
                }

                if (adata.auto_play)
                {
                    score_t.GetComponent<TextMeshProUGUI>().text = "Auto " + s;
                    combo_t.GetComponent<TextMeshProUGUI>().text = "Auto " + combo;
                }
                else
                {
                    score_t.GetComponent<TextMeshProUGUI>().text = s;
                    combo_t.GetComponent<TextMeshProUGUI>().text = combo + "";
                }

                if (DebugText.isDebugMode)
                {
                    DebugText.ratioscore = ratioscore + "";
                    DebugText.score = score + "";
                    DebugText.combo = combo + "";
                }
            }
        }
        else
        {
            setupped = false;
        }
    }
}