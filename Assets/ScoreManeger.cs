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
    // スコア
    public static int score = 0;
    // コンボ数
    public static int combo = 0;
    // 総ノーツ数
    public static int notes;
    private int maxscore;
    public static int ratioscore;
    public static bool setupped = false;
    private bool mode = false;
    private double prievous = 0;

    // Start is called before the first frame update
    private void Setup()
    {
        // ここからスコア初期化
        string chart = adata.chart;
        notes = 0;
        int l1_notes = 0;
        int l2_notes = 0;
        int l3_notes = 0;
        int l4_notes = 0;
        maxscore = 0;
        ratioscore = 0;

        foreach (var note in adata.L1Notes.Values) {
            int cnt = note.type == "long" ? 2 : 1;
            notes += cnt;
            l1_notes += cnt;
        }

        // L2
        foreach (var note in adata.L2Notes.Values) {
            int cnt = note.type == "long" ? 2 : 1;
            notes += cnt;
            l2_notes += cnt;
        }

        // L3
        foreach (var note in adata.L3Notes.Values) {
            int cnt = note.type == "long" ? 2 : 1;
            notes += cnt;
            l3_notes += cnt;
        }

        // L4
        foreach (var note in adata.L4Notes.Values) {
            int cnt = note.type == "long" ? 2 : 1;
            notes += cnt;
            l4_notes += cnt;
        }
        maxscore = notes * 21;
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
        // ここまでスコア初期化
    }

    // Update is called once per frame
    void Update()
    {
        // ここからスコア更新
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
        // ここまでスコア更新
    }
}