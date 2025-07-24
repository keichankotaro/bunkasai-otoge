using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeResolution : MonoBehaviour
{
    private int originalWidth;
    private int originalHeight;
    private bool originalFullScreen;

    // Start is called before the first frame update
    void Start()
    {
        // 現在の解像度を保存
        originalWidth = Screen.currentResolution.width;
        originalHeight = Screen.currentResolution.height;
        originalFullScreen = Screen.fullScreen;

        // 解像度を1920x1080に設定
        Screen.SetResolution(1920, 1080, true);
        Debug.Log($"Resolution changed to 1920x1080. Original resolution: {originalWidth}x{originalHeight}");
    }

    void OnEnable()
    {
        // アプリケーション終了時のイベントに登録
        Application.quitting += RestoreOriginalResolution;
    }

    void OnDisable()
    {
        // アプリケーション終了時のイベントから解除
        Application.quitting -= RestoreOriginalResolution;
    }

    private void RestoreOriginalResolution()
    {
        // 元の解像度に戻す
        Screen.SetResolution(originalWidth, originalHeight, originalFullScreen);
        Debug.Log($"Resolution restored to original: {originalWidth}x{originalHeight}");
    }
}
