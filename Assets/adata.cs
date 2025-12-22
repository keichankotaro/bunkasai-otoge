
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
//using UnityEditorInternal;
using UnityEngine;
public class adata : MonoBehaviour
{
    public static string chart = "��n�j�q���|���Ȃ�- �������� for LamazeP - Converted";
    public static float default_speed = 12.0f;
    public static float speed = 12.0f;
    public static float del_hantei = 0.75f;
    public static float visibleRange = 10.0f;
    public static float game_time = 0.0f;
    public static float tmptime = 0.0f;
    public static float offset_diameter = 100000f;
    public static GameObject p = (GameObject)Resources.Load("Prefab/Perfect");
    public static GameObject g = (GameObject)Resources.Load("Prefab/Good");
    public static GameObject b = (GameObject)Resources.Load("Prefab/Bad");
    public static GameObject m = (GameObject)Resources.Load("Prefab/Miss");
    public static GameObject Debug_L1 = (GameObject)Resources.Load("Prefab/debug_L1");

    public static int l1notes;
    public static int l2notes;
    public static int l3notes;
    public static int l4notes;
    public static int clicked_L1;
    public static int clicked_L2;
    public static int clicked_L3;
    public static int clicked_L4;

    // Prevention of double judgement in the same frame (Anti-Ghosting)
    public static int lastJudgedFrame_L1 = -1;
    public static int lastJudgedFrame_L2 = -1;
    public static int lastJudgedFrame_L3 = -1;
    public static int lastJudgedFrame_L4 = -1;

    // ����
    public static float auto = 0.001f;
    public static float perfect = 0.04f;
    public static float good = 0.08f;
    public static float bad = 0.15f;
    public static float miss = 0.22f;
    // �����O�I�_����
    public static float longEndTimeLag = 0.07f;

    public static bool ready_to_start = false;
    public static bool start_dl = false;

    public static string path = @"Assets/Resources/Charts";
    //public static string path = @"Assets\Resources\Charts";

    public static int maximum_notes = 50;

    public static float length = 0.0f;
    public static float now = 0.0f;
    public static float tlag = 0.0f;

    public static bool auto_play = false;

    public static float progress = 0.0f;

    public static bool ap = true;
    public static bool fc = true;

    public static int notes = 0;

    public static bool fcap_ed = false;

    public static bool showFPS = false;

    public static string ActivationCode = "";
    public static bool Activated = true;

    // �Ō��ID
    public static int LastID_L1;
    public static int LastID_L2;
    public static int LastID_L3;
    public static int LastID_L4;

    // json(���ʃf�[�^)��
    public static string loadjson;
    public static JObject jsonObj;
    public static JObject musicsJson;

    // ���[�f�B���O�̂���
    public static int loaded = 0;

    // �\�[�g�E�i�荞��
    public static string sort = "name";
    public static string order = "asc";
    public static string genre = "all";

    // UI�֘A
    public static JObject checksumsJson;
    public static JObject audioChecksumsJson; // �v���r���[�����̃`�F�b�N�T�����ꊇ�ŕۑ����܂�
    public static Dictionary<string, AudioClip> previewAudioCache = new Dictionary<string, AudioClip>(); // ���[�h�����������������ɃL���b�V�����܂�
    public static float previewFadeDuration = 0.5f; // �t�F�[�h�C���E�A�E�g�̎��Ԃł�
    public static int audioLoaded = 0;

    // �����I��
    public static bool ForceFinish = false;
    public static bool MusicStarted = false;
    public static bool Process = false;

    // �I�t���C�����[�h
    public static bool isOfflineMode = false;
    public static bool keepDownloads = false;
    public static string jacketPath = Path.Combine(Application.persistentDataPath, "Jackets");
    public static string musicPath = Path.Combine(Application.persistentDataPath, "Musics");
    public static string previewsPath = Path.Combine(Application.persistentDataPath, "Previews");
    public static string chartsPath = Path.Combine(Application.persistentDataPath, "Charts");

    // ���U���g
    public static bool showResult = false;
    public static string Difficulty = "";

    // SE
    /*
    public static CriAtomExPlayer SECriPlayer = new CriAtomExPlayer();
    public static CriAtomCueSheet SECueSheet = CriAtom.AddCueSheet("SE_Cue", "SE_Cue.acb", "");
    public static CriAtomExAcb SEAcb = SECueSheet.acb;
    */

    /*
    public static float perfect = 10.0f;
    public static float good = 20.0f;
    public static float bad = 30.0f;
    public static float miss = 40.0f;
    */

    // �Q�[���R���g���[���[��
    public static KeyCode ControlerStart = KeyCode.Joystick1Button0;
    public static KeyCode ControlerLine1 = KeyCode.Joystick1Button1;
    public static KeyCode ControlerLine2 = KeyCode.Joystick1Button2;
    public static KeyCode ControlerLine3 = KeyCode.Joystick1Button3;
    public static KeyCode ControlerLine4 = KeyCode.Joystick1Button4;
    public static KeyCode ControlerFX1 = KeyCode.Joystick1Button5;
    public static KeyCode ControlerFX2 = KeyCode.Joystick1Button6;

    // --- High Performance Data Cache ---
    public static Dictionary<int, NoteInfo> L1Notes = new Dictionary<int, NoteInfo>();
    public static Dictionary<int, NoteInfo> L2Notes = new Dictionary<int, NoteInfo>();
    public static Dictionary<int, NoteInfo> L3Notes = new Dictionary<int, NoteInfo>();
    public static Dictionary<int, NoteInfo> L4Notes = new Dictionary<int, NoteInfo>();

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
}