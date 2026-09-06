using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
//using UnityEditorInternal;
using UnityEngine;

public class adata : MonoBehaviour
{
    // ここから基本設定
    public static string chart = "njq|Ȃ-  for LamazeP - Converted";
    public static float default_speed = 12.0f;
    public static float speed = 12.0f;
    public static float del_hantei = 0.75f;
    public static float visibleRange = 10.0f;
    // ゲーム経過時間
    public static float game_time = 0.0f;
    public static float tmptime = 0.0f;
    public static float offset_diameter = 100000f;
    // 判定エフェクトのプレハブ
    public static GameObject p = (GameObject)Resources.Load("Prefab/Perfect");
    public static GameObject gr = (GameObject)Resources.Load("Prefab/Great");
    public static GameObject g = (GameObject)Resources.Load("Prefab/Good");
    public static GameObject b = (GameObject)Resources.Load("Prefab/Bad");
    public static GameObject m = (GameObject)Resources.Load("Prefab/Miss");
    // 各種スコア
    public static int perfect_plus_score = 100;
    public static int perfect_score = 90;
    public static int great_score = 50;
    public static int good_score = 10;
    public static int bad_score = 0; // bad廃止の為未使用
    public static int miss_score = 0;
    // デバッグ用プレハブ
    public static GameObject Debug_L1 = (GameObject)Resources.Load("Prefab/debug_L1");

    // 各レーンのノーツ数
    public static int l1notes;
    public static int l2notes;
    public static int l3notes;
    public static int l4notes;
    // 各レーンでクリックされた回数
    public static int clicked_L1;
    public static int clicked_L2;
    public static int clicked_L3;
    public static int clicked_L4;

    // 同時押し判定バグ防止用
    public static int lastJudgedFrame_L1 = -1;
    public static int lastJudgedFrame_L2 = -1;
    public static int lastJudgedFrame_L3 = -1;
    public static int lastJudgedFrame_L4 = -1;

    // 判定幅の設定
    public static float auto = 0.001f;
    public static float perfect_plus = 0.03f;
    public static float perfect = 0.05f;
    public static float great = 0.07f;
    public static float good = 0.1f;
    public static float bad = 0.15f; // bad廃止の為未使用
    public static float miss = 0.18f;
    // ロングノーツの終端判定ラグ
    public static float longEndTimeLag = 0.07f;
    // ここまで基本設定

    // ここからシステムフラグ
    public static bool ready_to_start = false;
    // ダウンロード開始フラグ
    public static bool start_dl = false;

    // チャートパス
    public static string path = @"Assets/Resources/Charts";

    // 1レーンあたりの最大同時表示ノーツ数
    public static int maximum_notes = 50;

    // 曲の長さ
    public static float length = 0.0f;
    // 現在位置
    public static float now = 0.0f;
    // タイムラグ調整
    public static float tlag = 0.0f;

    // オートプレイフラグ
    public static bool auto_play = false;

    // 進行度
    public static float progress = 0.0f;

    // オールパーフェクトフラグ
    public static bool ap = true;
    // フルコンボフラグ
    public static bool fc = true;

    // 総ノーツ数
    public static int notes = 0;

    // フルコン演出終了フラグ
    public static bool fcap_ed = false;

    // FPS表示フラグ
    public static bool showFPS = false;

    // アクティベーションコード
    public static string ActivationCode = "";
    // 有効化フラグ
    public static bool Activated = true;

    // 各レーンの最後のノーツID
    public static int LastID_L1;
    public static int LastID_L2;
    public static int LastID_L3;
    public static int LastID_L4;
    // ここまでシステムフラグ

    // ここからデータ関連
    // 読み込んだJSON
    public static string loadjson;
    // パース済みチャート
    public static JObject jsonObj;
    // 曲リストJSON
    public static JObject musicsJson;

    public static int loaded = 0;

    // ソート順の設定
    public static string sort = "name";
    public static string order = "asc";
    public static string genre = "all";

    // UI関連のデータ
    public static JObject checksumsJson;
    public static JObject audioChecksumsJson; // プレビュー音源のチェックサム
    public static Dictionary<string, AudioClip> previewAudioCache = new Dictionary<string, AudioClip>(); // プレビュー音源のキャッシュ
    public static float previewFadeDuration = 0.5f; // フェードイン・アウトの時間
    public static int audioLoaded = 0;

    // 強制終了とかの状態管理
    public static bool ForceFinish = false;
    public static bool MusicStarted = false;
    public static bool Process = false;

    // オフラインモードフラグ
    public static bool isOfflineMode = false;
    public static bool keepDownloads = false;
    // 各種パス
    public static string jacketPath = Path.Combine(Application.persistentDataPath, "Jackets");
    public static string musicPath = Path.Combine(Application.persistentDataPath, "Musics");
    public static string previewsPath = Path.Combine(Application.persistentDataPath, "Previews");
    public static string chartsPath = Path.Combine(Application.persistentDataPath, "Charts");

    // リザルト画面用
    public static bool showResult = false;
    public static string Difficulty = "";
    // ここまでデータ関連

    // ここから入力設定
    // コントローラーのキー
    public static KeyCode ControlerStart = KeyCode.Joystick1Button0;
    public static KeyCode ControlerLine1 = KeyCode.Joystick1Button1;
    public static KeyCode ControlerLine2 = KeyCode.Joystick1Button2;
    public static KeyCode ControlerLine3 = KeyCode.Joystick1Button3;
    public static KeyCode ControlerLine4 = KeyCode.Joystick1Button4;
    public static KeyCode ControlerFX1 = KeyCode.Joystick1Button5;
    public static KeyCode ControlerFX2 = KeyCode.Joystick1Button6;
    // ここまで入力設定

    public static Dictionary<int, NoteInfo> L1Notes = new Dictionary<int, NoteInfo>();
    public static Dictionary<int, NoteInfo> L2Notes = new Dictionary<int, NoteInfo>();
    public static Dictionary<int, NoteInfo> L3Notes = new Dictionary<int, NoteInfo>();
    public static Dictionary<int, NoteInfo> L4Notes = new Dictionary<int, NoteInfo>();

    public static void LoadChartData(JObject json)
    {
        // Clear previous data
        L1Notes.Clear();
        L2Notes.Clear();
        L3Notes.Clear();
        L4Notes.Clear();

        if (json["chartdata"] == null) return;

        // Helper to parse a lane
        void ParseLane(string laneKey, Dictionary<int, NoteInfo> targetDict)
        {
            if (json["chartdata"][laneKey] == null) return;

            JToken laneData = json["chartdata"][laneKey];
            foreach (JToken child in laneData.Children())
            {
                if (child is JProperty prop)
                {
                    // Skip metadata keys like "L1data" or "notes" if they are mixed in
                    if (int.TryParse(prop.Name, out int id))
                    {
                        JToken noteJson = prop.Value;
                        NoteInfo info = new NoteInfo();
                        info.aid = (int?)noteJson["aid"] ?? 0;
                        info.speed = (float?)noteJson["speed"] ?? 0f;
                        info.time = (float?)noteJson["time"] ?? 0f;
                        info.type = noteJson["type"]?.ToString() ?? "tap";
                        info.endtime = (float?)noteJson["endtime"] ?? 0f;
                        info.s_change = (bool?)noteJson["s_change"] ?? false;
                        if (info.s_change)
                        {
                            info.changes = noteJson["changes"] as JArray;
                        }
                        info.rawData = noteJson as JObject;

                        targetDict[id] = info;
                    }
                }
            }
        }

        ParseLane("L1", L1Notes);
        ParseLane("L2", L2Notes);
        ParseLane("L3", L3Notes);
        ParseLane("L4", L4Notes);
        
        Debug.Log($"[adata] Cached Chart Data. L1:{L1Notes.Count}, L2:{L2Notes.Count}, L3:{L3Notes.Count}, L4:{L4Notes.Count}");
    }

    public static string GetSafeFileName(string name)
    {
        return string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
    }

    public class NoteInfo
    {
        public int aid;
        public float speed;
        public float time;
        public string type;
        public float endtime;
        public bool s_change;
        public JArray changes;

        // Raw JObject for fallback/debugging or sending to debugger
        public JObject rawData;
    }
}