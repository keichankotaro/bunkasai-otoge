using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    // Start is called before the first frame update
    public static string max_combo = ""; // 記述済み
    public static string combo = ""; // 記述済み
    public static string max_score = ""; // 記述済み
    public static string score = ""; // 記述済み
    public static string ratioscore = ""; // 記述済み
    public static string max_score_l1 = ""; // 記述済み
    public static string max_score_l2 = ""; // 記述済み
    public static string max_score_l3 = ""; // 記述済み
    public static string max_score_l4 = ""; // 記述済み
    public static int score_l1 = 0; // 記述済み
    public static int score_l2 = 0; // 記述済み
    public static int score_l3 = 0; // 記述済み
    public static int score_l4 = 0; // 記述済み
    public static string max_l1 = ""; // 記述済み
    public static string max_l2 = ""; // 記述済み
    public static string max_l3 = ""; // 記述済み
    public static string max_l4 = ""; // 記述済み
    public static int judged_l1 = 0; // 記述済み
    public static int judged_l2 = 0; // 記述済み
    public static int judged_l3 = 0; // 記述済み
    public static int judged_l4 = 0; // 記述済み
    public static int perfect_l1 = 0; // 記述済み
    public static int perfect_l2 = 0; // 記述済み
    public static int perfect_l3 = 0; // 記述済み
    public static int perfect_l4 = 0; // 記述済み
    public static int good_l1 = 0; // 記述済み
    public static int good_l2 = 0; // 記述済み
    public static int good_l3 = 0; // 記述済み
    public static int good_l4 = 0; // 記述済み
    public static int bad_l1 = 0; // 記述済み
    public static int bad_l2 = 0; // 記述済み
    public static int bad_l3 = 0; // 記述済み
    public static int bad_l4 = 0; // 記述済み
    public static int miss_l1 = 0; // 記述済み
    public static int miss_l2 = 0; // 記述済み
    public static int miss_l3 = 0; // 記述済み
    public static int miss_l4 = 0; // 記述済み

    [SerializeField] GameObject dtxt;

    public static bool isDebugMode = false;

    public string getDebugText()
    {
        return dtxt.GetComponent<TextMeshProUGUI>().text;
    }

    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg == "--debug")
            {
                isDebugMode = true;
                adata.del_hantei = 0.0f;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDebugMode)
        {
            dtxt.GetComponent<TextMeshProUGUI>().text = "";
            return;
        }

        if (!adata.ready_to_start && isDebugMode)
        {
            max_combo = "";
            combo = "";
            max_score = "";
            score = "";
            ratioscore = "";
            max_score_l1 = "";
            max_score_l2 = "";
            max_score_l3 = "";
            max_score_l4 = "";
            max_l1 = "";
            max_l2 = "";
            max_l3 = "";
            max_l4 = "";
            score_l1 = 0;
            score_l2 = 0;
            score_l3 = 0;
            score_l4 = 0;
            judged_l1 = 0;
            judged_l2 = 0;
            judged_l3 = 0;
            judged_l4 = 0;
            perfect_l1 = 0;
            perfect_l2 = 0;
            perfect_l3 = 0;
            perfect_l4 = 0;
            good_l1 = 0;
            good_l2 = 0;
            good_l3 = 0;
            good_l4 = 0;
            bad_l1 = 0;
            bad_l2 = 0;
            bad_l3 = 0;
            bad_l4 = 0;
            miss_l1 = 0;
            miss_l2 = 0;
            miss_l3 = 0;
            miss_l4 = 0;
        }

        if (isDebugMode && adata.ready_to_start)
        {
            dtxt.GetComponent<TextMeshProUGUI>().text = "Max Combo = " + max_combo + "\n" +
                "Combo = " + combo + "\n" +
                "Max Score = " + max_score + "\n" +
                "Score = " + score + "\n" +
                "Ratio Score = " + ratioscore + "\n" +
                "Max Score L1 = " + max_score_l1 + "\n" +
                "Max Score L2 = " + max_score_l2 + "\n" +
                "Max Score L3 = " + max_score_l3 + "\n" +
                "Max Score L4 = " + max_score_l4 + "\n" +
                "Score L1 = " + score_l1 + "\n" +
                "Score L2 = " + score_l2 + "\n" +
                "Score L3 = " + score_l3 + "\n" +
                "Score L4 = " + score_l4 + "\n" +
                "Max L1 = " + max_l1 + "\n" +
                "Max L2 = " + max_l2 + "\n" +
                "Max L3 = " + max_l3 + "\n" +
                "Max L4 = " + max_l4 + "\n" +
                "Judged L1 = " + judged_l1 + "\n" +
                "Judged L2 = " + judged_l2 + "\n" +
                "Judged L3 = " + judged_l3 + "\n" +
                "Judged L4 = " + judged_l4 + "\n" +
                "Perfect L1 = " + perfect_l1 + "\n" +
                "Perfect L2 = " + perfect_l2 + "\n" +
                "Perfect L3 = " + perfect_l3 + "\n" +
                "Perfect L4 = " + perfect_l4 + "\n" +
                "Good L1 = " + good_l1 + "\n" +
                "Good L2 = " + good_l2 + "\n" +
                "Good L3 = " + good_l3 + "\n" +
                "Good L4 = " + good_l4 + "\n" +
                "Bad L1 = " + bad_l1 + "\n" +
                "Bad L2 = " + bad_l2 + "\n" +
                "Bad L3 = " + bad_l3 + "\n" +
                "Bad L4 = " + bad_l4 + "\n" +
                "Miss L1 = " + miss_l1 + "\n" +
                "Miss L2 = " + miss_l2 + "\n" +
                "Miss L3 = " + miss_l3 + "\n" +
                "Miss L4 = " + miss_l4 + "\n";
        }
    }
}
