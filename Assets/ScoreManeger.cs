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
    [SerializeField] GameObject perfect_t;
    [SerializeField] GameObject great_t;
    [SerializeField] GameObject good_t;
    [SerializeField] GameObject miss_t;
    [SerializeField] GameObject fast_t;
    [SerializeField] GameObject late_t;
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

    // キャッシュ済みTMPコンポーネント（毎フレームGetComponent回避）
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI comboText;
    private TextMeshProUGUI perfectText;
    private TextMeshProUGUI greatText;
    private TextMeshProUGUI goodText;
    private TextMeshProUGUI missText;
    private TextMeshProUGUI fastText;
    private TextMeshProUGUI lateText;

    // 前回値キャッシュ（変化時のみUI更新でGC削減）
    private int prevRatioScore = -1;
    private int prevCombo = -1;
    private int prevPerfect = -1;
    private int prevGreat = -1;
    private int prevGood = -1;
    private int prevMiss = -1;
    private int prevFast = -1;
    private int prevLate = -1;
    private bool prevAutoPlay = false;

    void Start()
    {
        // GetComponentをStart()で1回だけ呼んでキャッシュ
        scoreText = score_t.GetComponent<TextMeshProUGUI>();
        comboText = combo_t.GetComponent<TextMeshProUGUI>();
        perfectText = perfect_t.GetComponent<TextMeshProUGUI>();
        greatText = great_t.GetComponent<TextMeshProUGUI>();
        goodText = good_t.GetComponent<TextMeshProUGUI>();
        missText = miss_t.GetComponent<TextMeshProUGUI>();
        fastText = fast_t.GetComponent<TextMeshProUGUI>();
        lateText = late_t.GetComponent<TextMeshProUGUI>();
    }

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
        maxscore = notes * adata.perfect_plus_score;
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

        // 前回値キャッシュをリセット
        prevRatioScore = -1;
        prevCombo = -1;
        prevPerfect = -1;
        prevGreat = -1;
        prevGood = -1;
        prevMiss = -1;
        prevFast = -1;
        prevLate = -1;

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

                // オートプレイ切替時のみ更新
                if (adata.auto_play != prevAutoPlay)
                {
                    prevAutoPlay = adata.auto_play;
                    if (adata.auto_play)
                    {
                        scoreText.text = "Auto";
                        comboText.text = "Auto";
                    }
                    prevRatioScore = -1; // 強制更新
                    prevCombo = -1;
                }

                // 値が変わった時だけUI更新（GCアロケーション削減）
                if (!adata.auto_play)
                {
                    if (ratioscore != prevRatioScore)
                    {
                        prevRatioScore = ratioscore;
                        scoreText.text = ratioscore.ToString("D7");
                    }
                    if (combo != prevCombo)
                    {
                        prevCombo = combo;
                        comboText.SetText("{0}", combo);
                    }
                }

                int curPerfect = ResultUI.PerfectPlus + ResultUI.Perfect;
                if (curPerfect != prevPerfect)
                {
                    prevPerfect = curPerfect;
                    perfectText.SetText("Perfect: {0}", curPerfect);
                }

                if (ResultUI.Great != prevGreat)
                {
                    prevGreat = ResultUI.Great;
                    greatText.SetText("Great: {0}", ResultUI.Great);
                }

                if (ResultUI.Good != prevGood)
                {
                    prevGood = ResultUI.Good;
                    goodText.SetText("Good: {0}", ResultUI.Good);
                }

                if (ResultUI.Miss != prevMiss)
                {
                    prevMiss = ResultUI.Miss;
                    missText.SetText("Miss: {0}", ResultUI.Miss);
                }

                int curFast = ResultUI.PerfectFast + ResultUI.GreatFast + ResultUI.GoodFast;
                if (curFast != prevFast)
                {
                    prevFast = curFast;
                    fastText.SetText("Fast: {0}", curFast);
                }

                int curLate = ResultUI.PerfectLate + ResultUI.GreatLate + ResultUI.GoodLate;
                if (curLate != prevLate)
                {
                    prevLate = curLate;
                    lateText.SetText("Late: {0}", curLate);
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