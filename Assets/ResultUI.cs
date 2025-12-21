using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField] public GameObject Result;
    [SerializeField] public GameObject ScoreText;
    [SerializeField] public GameObject PerfectText;
    [SerializeField] public GameObject GoodText;
    [SerializeField] public GameObject BadText;
    [SerializeField] public GameObject MissText;
    [SerializeField] public GameObject RankText;
    [SerializeField] public GameObject MusicText;
    [SerializeField] public GameObject LevelText;
    [SerializeField] public GameObject ComposerText;
    [SerializeField] public GameObject Jacket;
    public static int Perfect;
    public static int Good;
    public static int Bad;
    public static int Miss;
    public static int Score;

    public static bool showed = false;
    private bool reseted = true;
    public static string Rank;
    public static bool setup = false;
    public static bool show = false;

    // Result data
    private string music;
    private string composer;
    private string level;
    private int score;
    private int perfect;
    private int good;
    private int bad;
    private int miss;
    private Sprite jacket;

    void Start()
    {
        // Ensure APIManager instance exists
        if (APIManager.Instance == null)
        {
            gameObject.AddComponent<APIManager>();
        }
    }

    void Update()
    {
        if (show && !showed)
        {
            // Set result data to UI
            ScoreText.GetComponent<TextMeshProUGUI>().text = score.ToString();
            PerfectText.GetComponent<TextMeshProUGUI>().text = "Perfect: " + perfect;
            GoodText.GetComponent<TextMeshProUGUI>().text = "Good: " + good;
            BadText.GetComponent<TextMeshProUGUI>().text = "Bad: " + bad;
            MissText.GetComponent<TextMeshProUGUI>().text = "Miss: " + miss;
            Rank = adata.auto_play ? "Auto" : (score > 990000) ? "SSS+" : (score > 980000) ? "SSS" : (score > 950000) ? "SS" : (score > 900000) ? "S" : (score > 850000) ? "AAA" : (score > 800000) ? "AA" : (score > 750000) ? "A" : (score > 700000) ? "BBB" : (score > 650000) ? "BB" : (score > 600000) ? "B" : (score > 550000) ? "C" : "D";
            RankText.GetComponent<TextMeshProUGUI>().text = Rank;
            MusicText.GetComponent<TextMeshProUGUI>().text = music;
            LevelText.GetComponent<TextMeshProUGUI>().text = level;
            ComposerText.GetComponent<TextMeshProUGUI>().text = composer;
            Jacket.GetComponent<Image>().sprite = jacket;

            // Upload result if logged in and not in autoplay
            if (APIManager.Instance != null && APIManager.Instance.IsLoggedIn() && !adata.auto_play)
            {
                StartCoroutine(APIManager.Instance.UploadResult(music, adata.Difficulty, score));
            }

            // Update result display flags
            showed = true;
            adata.fcap_ed = true;
            Result.GetComponent<Canvas>().sortingOrder = 1;
            Result.GetComponent<CanvasGroup>().alpha = 1;
            Debug.Log("Result UI is shown");
        }

        if (showed)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Reset when leaving the result screen
                Result.GetComponent<CanvasGroup>().alpha = 0;
                Result.GetComponent<Canvas>().sortingOrder = -1;
                ScoreManeger.combo = 0;
                ScoreManeger.score = 0;
                ScoreManeger.ratioscore = 0;
                ScoreManeger.setupped = false;
                adata.ready_to_start = false;
                adata.start_dl = false;
                adata.fcap_ed = false;
                adata.showResult = false;
                ShowDetails.Music = "";
                ShowDetails.LevelExists = new List<bool> { false, false, false, false };
                ShowDetails.getted = false;
                ShowDetails.alreadyRequest = false;
                adata.now = 0;
                adata.game_time = 0;
                DebugText.perfect_l1 = DebugText.perfect_l2 = DebugText.perfect_l3 = DebugText.perfect_l4 = DebugText.good_l1 = DebugText.good_l2 = DebugText.good_l3 = DebugText.good_l4 = DebugText.bad_l1 = DebugText.bad_l2 = DebugText.bad_l3 = DebugText.bad_l4 = DebugText.miss_l1 = DebugText.miss_l2 = DebugText.miss_l3 = DebugText.miss_l4 = 0;
                DebugText.judged_l1 = DebugText.judged_l2 = DebugText.judged_l3 = DebugText.judged_l4 = 0;
                DebugText.score_l1 = DebugText.score_l2 = DebugText.score_l3 = DebugText.score_l4 = 0;
                DebugText.max_combo = "";
                DebugText.max_score = "";
                DebugText.max_l1 = DebugText.max_l2 = DebugText.max_l3 = DebugText.max_l4 = "";
                DebugText.max_score_l1 = DebugText.max_score_l2 = DebugText.max_score_l3 = DebugText.max_score_l4 = "";
                DebugText.ratioscore = "";
                DebugText.score = "";

                if (APIManager.Instance != null)
                {
                    APIManager.Instance.InvalidateHighScores();
                    StartCoroutine(APIManager.Instance.FetchHighScores());
                }

                showed = false;
                show = false;
                reseted = true;
                PlayMusic.started_dl = false;
                Perfect = 0;
                Good = 0;
                Bad = 0;
                Miss = 0;
            }
        }
    }

    public void SetResultData(string music, string composer, string level, int score, int perfect, int good, int bad, int miss, Sprite jacket)
    {
        this.music = music;
        this.composer = composer;
        this.level = level;
        this.score = score;
        this.perfect = perfect;
        this.good = good;
        this.bad = bad;
        this.miss = miss;
        this.jacket = jacket;

        // Normalize difficulty string for API
        if (!string.IsNullOrEmpty(this.level))
        {
            this.level = char.ToUpper(this.level[0]) + this.level.Substring(1).ToLower();
        }

        // Set flag to show result
        show = true;
    }
}