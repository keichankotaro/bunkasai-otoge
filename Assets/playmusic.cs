/*using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;

public class PlayMusic : MonoBehaviour
{
    private AudioSource audioSource;
    public static float time;
    public static bool played;
    private string bgm;
    public static float offset;
    private bool settedUp;
    private AudioClip audioClip;
    private AudioClip music;

    private float tmp;

    private void Start()
    {
        StartCoroutine(PlayMusicRoutine());
    }

    private IEnumerator PlayMusicRoutine()
    {
        while (true)
        {
            if (adata.ready_to_start)
            {
                if (!settedUp)
                {
                    yield return StartCoroutine(Setup());
                    settedUp = true;
                }

                time += Time.deltaTime;
                if (time >= 0.0f + offset && !played)
                {
                    audioSource.Play(0);
                    played = true;
                }

                if (time <= 8.0f + offset + adata.tlag && played)
                {
                    audioSource.time = 0.0f;
                }

if (audioSource != null && !audioSource.isPlaying && played && settedUp)
{
    if (tmp >= 3.0f)
    {
        played = false;
        StopSound();
                        ScoreManeger.combo = 0;
                        ScoreManeger.score = 0;
                        ScoreManeger.setupped = false;
                        adata.ready_to_start = false;
                        settedUp = false;
                        time = 0;
                        adata.now = 0;
                    }
                    else
                    {
                        tmp += Time.deltaTime;
                        if (tmp >= 3.0f)
                        {
                            Resources.UnloadAsset(audioClip);
                        }
                    }
                }
                else
                {
                    tmp = 0;
                }
            }

            yield return null;
        }
    }

    private IEnumerator Setup()
    {
        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
        JObject jsonObj = JObject.Parse(loadjson);
        bgm = jsonObj["maindata"]["music"].ToString();
        //offset = float.Parse(jsonObj["maindata"]["offset"] + "") / 1000;
        offset = 0;
        //StopSound();

        audioClip = Resources.Load<AudioClip>("Musics/" + bgm);
        if (audioClip != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            //audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClip;
            //music = (AudioClip)Resources.Load("Musics/" + bgm);
            adata.length = audioClip.length;
        }
        else
        {
            ////Debug.LogError("Failed to load audio clip: Musics/" + bgm);
        }

        yield return null; // 1フレーム待機
    }

    private void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            Destroy(audioSource);
        }
    }

    // 他のメソッドやイベントハンドラなど
}
*/

/*
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;
using CriAtomSource = ADX2LE.CriAtomSource;

public class PlayMusic : MonoBehaviour
{
    private CriAtomSource audioSource;
    public static float time;
    public static bool played;
    private string bgm;
    public static float offset;
    private bool settedUp;
    private AudioClip audioClip;
    private AudioClip music;

    private float tmp;

    private void Start()
    {
        StartCoroutine(PlayMusicRoutine());
    }

    private IEnumerator PlayMusicRoutine()
    {
        while (true)
        {
            if (adata.ready_to_start)
            {
                if (!settedUp)
                {
                    yield return StartCoroutine(Setup());
                    settedUp = true;
                }

                time += Time.deltaTime;
                if (time >= 0.0f + offset && !played)
                {
                    audioSource.Play();
                    played = true;
                }

                if (time <= 8.0f + offset + adata.tlag && played)
                {
                    audioSource.SetPosition(0);
                }

                if (audioSource != null && !audioSource.IsPlaying() && played && settedUp)
                {
                    if (tmp >= 3.0f)
                    {
                        played = false;
                        StopSound();
                        ScoreManeger.combo = 0;
                        ScoreManeger.score = 0;
                        ScoreManeger.setupped = false;
                        adata.ready_to_start = false;
                        settedUp = false;
                        time = 0;
                        adata.now = 0;
                    }
                    else
                    {
                        tmp += Time.deltaTime;
                        if (tmp >= 3.0f)
                        {
                            Resources.UnloadAsset(audioClip);
                        }
                    }
                }
                else
                {
                    tmp = 0;
                }
            }

            yield return null;
        }
    }

    private IEnumerator Setup()
    {
        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
        JObject jsonObj = JObject.Parse(loadjson);
        bgm = jsonObj["maindata"]["music"].ToString();
        offset = 0;

        audioClip = Resources.Load<AudioClip>("Musics/" + bgm);
        if (audioClip != null)
        {
            audioSource = gameObject.AddComponent<CriAtomSource>();
            audioSource.cueSheet = "CueSheet_0";
            audioSource.cueName = bgm; // Assuming the cue name is same as the music file name
            adata.length = audioClip.length;
        }
        else
        {
            ////Debug.LogError("Failed to load audio clip: Musics/" + bgm);
        }

        yield return null;
    }

    private void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            Destroy(audioSource);
        }
    }
}
*/

using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;
using CriWare;

public class PlayMusic : MonoBehaviour
{
    public string CueSheetName = "CueSheet_0";
    public string AcbFilePath = "CueSheet_0.acb";
    private CriAtomExAcb acb;
    private CriAtomExPlayer atomExPlayer;

    public static float time;
    public static bool played;
    public string bgm;
    public static float offset;
    public bool settedUp;
    private AudioClip audioClip;
    private float tmp;

    private void Awake()
    {
        atomExPlayer = new CriAtomExPlayer();
    }

    private IEnumerator Start()
    {
        var cueSheet = CriAtom.AddCueSheet(CueSheetName, AcbFilePath, "");
        while (cueSheet.IsLoading)
        {
            yield return null;
        }

        acb = cueSheet.acb;
        StartCoroutine(PlayMusicRoutine());
    }

    private IEnumerator PlayMusicRoutine()
    {
        while (true)
        {
            if (adata.ready_to_start)
            {
                if (!settedUp)
                {
                    yield return StartCoroutine(Setup());
                    settedUp = true;
                }

                time += Time.deltaTime;
                /*if (time >= 0.0f + offset && !played)
                {
                    PlayBgm(bgm);
                    played = true;
                }*/

                if (time >= (8.0f + offset + adata.tlag) && !played)
                {
                    PlayBgm(bgm);
                    played = true;
                    Debug.Log("Play...");
                    time = atomExPlayer.length() / 1000;
                }

                if (atomExPlayer.GetStatus()+"" != "Playing" && played && settedUp)
                {
                    if (tmp >= 3.0f)
                    {
                        played = false;
                        StopBgm();
                        ResultUI.Score = ScoreManeger.ratioscore;
                        ScoreManeger.combo = 0;
                        ScoreManeger.score = 0;
                        ScoreManeger.setupped = false;
                        adata.ready_to_start = false;
                        settedUp = false;
                        time = 0;
                        adata.now = 0;
                    }
                    else
                    {
                        tmp += Time.deltaTime;
                        if (tmp >= 3.0f)
                        {
                            StopBgm();
                        }
                    }
                }
                else
                {
                    tmp = 0;
                }
            }

            yield return null;
        }
    }

    private IEnumerator Setup()
    {
        string loadjson = Resources.Load<TextAsset>("Charts/" + adata.chart).ToString();
        JObject jsonObj = JObject.Parse(loadjson);
        bgm = jsonObj["maindata"]["music"].ToString();
        offset = 0;

        var cueSheet = CriAtom.AddCueSheet(CueSheetName, AcbFilePath, "");
        while (cueSheet.IsLoading)
        {
            yield return null;
        }

        acb = cueSheet.acb;

        yield return null; // 1フレーム待機
    }

    public void PlayBgm(string bgmCueName)
    {
        if (!acb.Exists(bgmCueName)) return;

        atomExPlayer.SetCue(acb, bgmCueName);
        adata.length = Resources.Load<AudioClip>("Musics/" + bgm).length;
        atomExPlayer.Start();
    }

    public void StopBgm()
    {
        atomExPlayer.Stop();
    }

    // 他のメソッドやイベントハンドラなど
}
