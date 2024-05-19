using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectScene : MonoBehaviour
{
    public Text chartNameText;
    public TextMeshProUGUI songNameText;

    private int _currentChartIndex = 0;

    private string[] chartFileNames;

    // 初期化
    void Awake()
    {
        // 譜面データのディレクトリを取得する
        string chartDirectory = "Assets/Charts";

        // 譜面データのファイル名を取得する
        chartFileNames = Directory.GetFiles(chartDirectory, "*.json");

        // 曲名を取得する
        songNameText.text = chartFileNames[_currentChartIndex].Split(".json")[0];

        // 曲名をUIに表示する
        chartNameText.text = songNameText.text;
    }

    // 上下キーで曲を選択
    public void OnKeyDown(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow:
                _currentChartIndex--;
                break;
            case KeyCode.DownArrow:
                _currentChartIndex++;
                break;
        }

        // 曲名を取得する
        songNameText.text = chartFileNames[_currentChartIndex].Split(".json")[0];

        // 曲名をUIに表示する
        chartNameText.text = songNameText.text;
    }

    // Enterキーでゲーム開始
    public void OnKeyUp(KeyCode key)
    {
        if (key == KeyCode.Return)
        {
            // 選曲画面を透明にする
            GetComponent<Canvas>().enabled = false;

            // 譜面データを設定する
            adata.ready_to_start = true;
            ScoreManeger.score = 0;
            ScoreManeger.combo = 0;
            adata.chart = chartFileNames[_currentChartIndex].Split(".json")[0];

            // ゲーム画面に遷移する
            Application.LoadLevel("GameScene");
        }
    }
}
