using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text;

public class DebugText : MonoBehaviour
{
    // Start is called before the first frame update
    public static string max_combo = ""; // 最大コンボ
    public static string combo = ""; // コンボ数
    public static string max_score = ""; // 最大スコア
    public static string score = ""; // スコア
    public static string ratioscore = ""; // 比率スコア
    public static string max_score_l1 = ""; // 最大スコア
    public static string max_score_l2 = ""; // 最大スコア
    public static string max_score_l3 = ""; // 最大スコア
    public static string max_score_l4 = ""; // 最大スコア
    public static int score_l1 = 0; // スコア
    public static int score_l2 = 0; // スコア
    public static int score_l3 = 0; // スコア
    public static int score_l4 = 0; // スコア
    public static string max_l1 = ""; // 最大値
    public static string max_l2 = ""; // 最大値
    public static string max_l3 = ""; // 最大値
    public static string max_l4 = ""; // 最大値
    public static int judged_l1 = 0; // 判定数
    public static int judged_l2 = 0; // 判定数
    public static int judged_l3 = 0; // 判定数
    public static int judged_l4 = 0; // 判定数
    public static int perfect_l1 = 0; // Perfect
    public static int perfect_l2 = 0; // Perfect
    public static int perfect_l3 = 0; // Perfect
    public static int perfect_l4 = 0; // Perfect
    public static int good_l1 = 0; // Good
    public static int good_l2 = 0; // Good
    public static int good_l3 = 0; // Good
    public static int good_l4 = 0; // Good
    public static int bad_l1 = 0; // Bad
    public static int bad_l2 = 0; // Bad
    public static int bad_l3 = 0; // Bad
    public static int bad_l4 = 0; // Bad
    public static int miss_l1 = 0; // Miss
    public static int miss_l2 = 0; // Miss
    public static int miss_l3 = 0; // Miss
    public static int miss_l4 = 0; // Miss

    [SerializeField] GameObject dtxt;
    private TextMeshProUGUI debugOutput;
    [SerializeField] private float refreshInterval = 0.1f;
    private float refreshTimer = 0f;

    public static bool isDebugMode = false;

    public string getDebugText()
    {
        return debugOutput.text;
    }

    void Start()
    {
        debugOutput = dtxt.GetComponent<TextMeshProUGUI>();
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
            if (!string.IsNullOrEmpty(debugOutput.text))
            {
                debugOutput.text = "";
            }
            return;
        }

        refreshTimer += Time.unscaledDeltaTime;
        if (refreshTimer < refreshInterval)
        {
            return;
        }
        refreshTimer = 0f;

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
            var builder = new StringBuilder();
            builder.AppendLine("Max Combo = " + max_combo);
            builder.AppendLine("Combo = " + combo);
            builder.AppendLine("Max Score = " + max_score);
            builder.AppendLine("Score = " + score);
            builder.AppendLine("Ratio Score = " + ratioscore);
            builder.AppendLine("Max Score L1 = " + max_score_l1);
            builder.AppendLine("Max Score L2 = " + max_score_l2);
            builder.AppendLine("Max Score L3 = " + max_score_l3);
            builder.AppendLine("Max Score L4 = " + max_score_l4);
            builder.AppendLine("Score L1 = " + score_l1);
            builder.AppendLine("Score L2 = " + score_l2);
            builder.AppendLine("Score L3 = " + score_l3);
            builder.AppendLine("Score L4 = " + score_l4);
            builder.AppendLine("Max L1 = " + max_l1);
            builder.AppendLine("Max L2 = " + max_l2);
            builder.AppendLine("Max L3 = " + max_l3);
            builder.AppendLine("Max L4 = " + max_l4);
            builder.AppendLine("Judged L1 = " + judged_l1);
            builder.AppendLine("Judged L2 = " + judged_l2);
            builder.AppendLine("Judged L3 = " + judged_l3);
            builder.AppendLine("Judged L4 = " + judged_l4);
            builder.AppendLine("Perfect L1 = " + perfect_l1);
            builder.AppendLine("Perfect L2 = " + perfect_l2);
            builder.AppendLine("Perfect L3 = " + perfect_l3);
            builder.AppendLine("Perfect L4 = " + perfect_l4);
            builder.AppendLine("Good L1 = " + good_l1);
            builder.AppendLine("Good L2 = " + good_l2);
            builder.AppendLine("Good L3 = " + good_l3);
            builder.AppendLine("Good L4 = " + good_l4);
            builder.AppendLine("Bad L1 = " + bad_l1);
            builder.AppendLine("Bad L2 = " + bad_l2);
            builder.AppendLine("Bad L3 = " + bad_l3);
            builder.AppendLine("Bad L4 = " + bad_l4);
            builder.AppendLine("Miss L1 = " + miss_l1);
            builder.AppendLine("Miss L2 = " + miss_l2);
            builder.AppendLine("Miss L3 = " + miss_l3);
            builder.AppendLine("Miss L4 = " + miss_l4);

            debugOutput.text = builder.ToString();
        }
    }
}
