using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleCall : MonoBehaviour
{
    public AudioSource titleAudioSource;
    public TextMeshProUGUI titleText;
    private bool played = false;
    private float duration = 2.0f;
    private bool isChangingScene = false;
    private float time = 0f;
    private bool show = true;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("タイトルの読み込み");
        if (titleAudioSource != null)
        {
            AudioClip bgmClip = Resources.Load<AudioClip>("BGM/Pixel Glitch");
            titleAudioSource.clip = bgmClip;
            titleAudioSource.volume = 0.0f; // 0からスタート
            titleAudioSource.loop = true;
            //titleAudioSource.Play();

            // フェードインを開始
            //StartCoroutine(FadeIn(duration));
            played = true; // これでUpdate内の処理は実行されなくなる
        }
        else
        {
            Debug.LogError("Main Cameraに AudioSource コンポーネントがありません！");
        }
    }

    private IEnumerator FadeIn(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            titleAudioSource.volume = Mathf.Lerp(0, 1, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        titleAudioSource.volume = 1;
    }

    private IEnumerator FadeOut(float duration)
    {
        float startVolume = titleAudioSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            titleAudioSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        titleAudioSource.volume = 0;
        titleAudioSource.Stop();
        titleAudioSource.clip = null;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (Input.anyKeyDown && played && !isChangingScene)
        {
            Debug.Log("シーン遷移開始");
            isChangingScene = true; // 遷移処理を開始 (連打防止)
            StartCoroutine(DoSceneChange());
        }

        if (time >= 0.5f && !show)
        {
            titleText.text = "文化祭音ゲー（仮称）\r\n\r\n\r\n\r\n\r\n- Press any key to start -";
            time = 0f;
            show = true;
        }
        if (time >= 2f && show)
        {
            titleText.text = "文化祭音ゲー（仮称）\r\n\r\n\r\n\r\n\r\n\r\n";
            time = 0f;
            show = false;
        }
    }

    private IEnumerator DoSceneChange()
    {
        // まずFadeOutコルーチンを呼び出し、終わるまで待つ (yield return)
        //yield return StartCoroutine(FadeOut(duration));
        yield return null;

        // FadeOutが終わったらシーンをロードする
        SceneManager.LoadScene("Otoge-Scene");
    }
}
