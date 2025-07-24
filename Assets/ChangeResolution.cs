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
        // ���݂̉𑜓x��ۑ�
        originalWidth = Screen.currentResolution.width;
        originalHeight = Screen.currentResolution.height;
        originalFullScreen = Screen.fullScreen;

        // �𑜓x��1920x1080�ɐݒ�
        Screen.SetResolution(1920, 1080, true);
        Debug.Log($"Resolution changed to 1920x1080. Original resolution: {originalWidth}x{originalHeight}");
    }

    void OnEnable()
    {
        // �A�v���P�[�V�����I�����̃C�x���g�ɓo�^
        Application.quitting += RestoreOriginalResolution;
    }

    void OnDisable()
    {
        // �A�v���P�[�V�����I�����̃C�x���g�������
        Application.quitting -= RestoreOriginalResolution;
    }

    private void RestoreOriginalResolution()
    {
        // ���̉𑜓x�ɖ߂�
        Screen.SetResolution(originalWidth, originalHeight, originalFullScreen);
        Debug.Log($"Resolution restored to original: {originalWidth}x{originalHeight}");
    }
}
