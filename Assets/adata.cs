using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public class adata : MonoBehaviour
{
    public static string chart = "“ï’®Œn’jŽq‚ª“|‚¹‚È‚¢- ‹¾‰¹ƒŠƒ“ for LamazeP - Converted";
    public static float default_speed = 10.0f;
    public static float speed = 10.0f;
    public static float del_hantei = 0.75f;
    public static float visibleRange = 10.0f;
    public static float game_time = 0.0f;
    public static float tmptime = 0.0f;
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

    public static float auto = 0.001f;
    public static float perfect = 0.05f;
    public static float good = 0.10f;
    public static float bad = 0.18f;
    public static float miss = 0.22f;

    public static bool ready_to_start = false;

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

    /*
    public static float perfect = 10.0f;
    public static float good = 20.0f;
    public static float bad = 30.0f;
    public static float miss = 40.0f;
    */
}