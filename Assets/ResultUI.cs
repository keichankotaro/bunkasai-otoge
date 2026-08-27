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
    [SerializeField] public GameObject PerfectPlusText;
    [SerializeField] public GameObject PerfectText;
    [SerializeField] public GameObject GreatText;
    [SerializeField] public GameObject GoodText;
    //[SerializeField] public GameObject BadText;
    [SerializeField] public GameObject MissText;
    [SerializeField] public GameObject RankText;
    [SerializeField] public GameObject MusicText;
    [SerializeField] public GameObject LevelText;
    [SerializeField] public GameObject ComposerText;
    [SerializeField] public GameObject Jacket;

    [Header("Counts Detail UI")]
    public GameObject MaxComboText;
    public GameObject DiffText;
    public GameObject CountsPanel;
    public GameObject CountsDetailPanel;
    public UnityEngine.UI.Button GoCountsDetailButton;
    public UnityEngine.UI.Button BackButton;

    [Header("Fast/Late Texts")]
    public TextMeshProUGUI PerfectPlusFastText;
    public TextMeshProUGUI PerfectPlusLateText;
    public TextMeshProUGUI PerfectFastText;
    public TextMeshProUGUI PerfectLateText;
    public TextMeshProUGUI GreatFastText;
    public TextMeshProUGUI GreatLateText;
    public TextMeshProUGUI GoodFastText;
    public TextMeshProUGUI GoodLateText;
    //public TextMeshProUGUI BadFastText;
    //public TextMeshProUGUI BadLateText;

    // Fast/Late Counters
    public static int PerfectPlusFast, PerfectPlusLate;
    public static int PerfectFast, PerfectLate;
    public static int GreatFast, GreatLate;
    public static int GoodFast, GoodLate;
    public static int BadFast, BadLate;

    public static int PerfectPlus;
    public static int Perfect;
    public static int Great;
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

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel == null) return;
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = active ? 1f : 0f;
            cg.interactable = active;
            cg.blocksRaycasts = active;
        }
    }

    void Start()
    {
        // Ensure APIManager instance exists
        if (APIManager.Instance == null)
        {
            gameObject.AddComponent<APIManager>();
        }

        if (GoCountsDetailButton != null && CountsDetailPanel != null && CountsPanel != null)
        {
            GoCountsDetailButton.onClick.AddListener(() => { 
                SetPanelActive(CountsPanel, false);
                SetPanelActive(CountsDetailPanel, true);
            });
        }
        if (BackButton != null && CountsDetailPanel != null && CountsPanel != null)
        {
            BackButton.onClick.AddListener(() => { 
                SetPanelActive(CountsDetailPanel, false);
                SetPanelActive(CountsPanel, true);
            });
        }
    }

    void Update()
    {
        if (show && !showed)
        {
            SetPanelActive(CountsDetailPanel, false);
            SetPanelActive(CountsPanel, true);
            // Set result data to UI
            ScoreText.GetComponent<TextMeshProUGUI>().text = score.ToString();

            if (PerfectPlusFastText != null) PerfectPlusFastText.text = "Fast: " + PerfectPlusFast.ToString();
            if (PerfectPlusLateText != null) PerfectPlusLateText.text = "Late: " + PerfectPlusLate.ToString();
            if (PerfectFastText != null) PerfectFastText.text = "Fast: " + PerfectFast.ToString();
            if (PerfectLateText != null) PerfectLateText.text = "Late: " + PerfectLate.ToString();
            if (GreatFastText != null) GreatFastText.text = "Fast: " + GreatFast.ToString();
            if (GreatLateText != null) GreatLateText.text = "Late: " + GreatLate.ToString();
            if (GoodFastText != null) GoodFastText.text = "Fast: " + GoodFast.ToString();
            if (GoodLateText != null) GoodLateText.text = "Late: " + GoodLate.ToString();
            //if (BadFastText != null) BadFastText.text = "Fast: " + BadFast.ToString();
            //if (BadLateText != null) BadLateText.text = "Late: " + BadLate.ToString();
            //PerfectPlusText.GetComponent<TextMeshProUGUI>().text = "Perfect+: " + PerfectPlus;
            PerfectText.GetComponent<TextMeshProUGUI>().text = "Perfect: " + (perfect + PerfectPlus);
            GreatText.GetComponent<TextMeshProUGUI>().text = "Great: " + Great;
            GoodText.GetComponent<TextMeshProUGUI>().text = "Good: " + good;
            //BadText.GetComponent<TextMeshProUGUI>().text = "Bad: " + bad;
            MissText.GetComponent<TextMeshProUGUI>().text = "Miss: " + miss;
            Rank = adata.auto_play ? "Auto" : (score > 990000) ? "SSS+" : (score > 980000) ? "SSS" : (score > 975000) ? "SS+" : (score > 950000) ? "SS" : (score > 925000) ? "S+" : (score > 900000) ? "S" : (score > 850000) ? "AAA" : (score > 800000) ? "AA" : (score > 750000) ? "A" : (score > 700000) ? "BBB" : (score > 650000) ? "BB" : (score > 600000) ? "B" : (score > 550000) ? "C" : "D";
            RankText.GetComponent<TextMeshProUGUI>().text = Rank;
            MusicText.GetComponent<TextMeshProUGUI>().text = music;
            LevelText.GetComponent<TextMeshProUGUI>().text = level;
            ComposerText.GetComponent<TextMeshProUGUI>().text = composer;
            Jacket.GetComponent<Image>().sprite = jacket;

            // Upload result if logged in and not in autoplay
            if (APIManager.Instance != null && APIManager.Instance.IsLoggedIn() && !adata.auto_play)
            {
                StartCoroutine(APIManager.Instance.UploadResult(music, adata.Difficulty, score));
                
                if (DiffText != null)
                {
                    int oldHighScore = APIManager.Instance.GetHighScore(music, adata.Difficulty);
                    if (oldHighScore > 0)
                    {
                        int diff = score - oldHighScore;
                        if (diff > 0)
                        {
                            DiffText.GetComponent<TextMeshProUGUI>().text = "+" + diff.ToString("N0");
                        }
                        else if (diff == 0)
                        {
                            DiffText.GetComponent<TextMeshProUGUI>().text = "±0";
                        }
                        else
                        {
                            DiffText.GetComponent<TextMeshProUGUI>().text = diff.ToString("N0");
                        }
                    }
                    else
                    {
                        DiffText.GetComponent<TextMeshProUGUI>().text = "New Record!";
                    }
                }
            }
            else
            {
                if (DiffText != null) DiffText.GetComponent<TextMeshProUGUI>().text = "";
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
                
                if (APIManager.Instance != null && APIManager.Instance.IsLoggedIn())
                {
                    StartCoroutine(APIManager.Instance.FetchHighScores());
                }
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
                PerfectPlus = 0;
                Perfect = 0;
                Great = 0;
                Good = 0;
                Bad = 0;
                Miss = 0;

                PerfectPlusFast = 0; PerfectPlusLate = 0;
                PerfectFast = 0; PerfectLate = 0;
                GreatFast = 0; GreatLate = 0;
                GoodFast = 0; GoodLate = 0;
                BadFast = 0; BadLate = 0;
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