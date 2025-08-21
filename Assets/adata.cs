
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

    // ����
    public static float auto = 0.001f;
    public static float perfect = 0.05f;
    public static float good = 0.08f;
    public static float bad = 0.13f;
    public static float miss = 0.20f;
    // �����O�I�_����
    public static float longEndTimeLag = 0.1f;

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
}
