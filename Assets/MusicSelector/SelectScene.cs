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

    // ������
    void Awake()
    {
        // ���ʃf�[�^�̃f�B���N�g�����擾����
        string chartDirectory = "Assets/Charts";

        // ���ʃf�[�^�̃t�@�C�������擾����
        chartFileNames = Directory.GetFiles(chartDirectory, "*.json");

        // �Ȗ����擾����
        songNameText.text = chartFileNames[_currentChartIndex].Split(".json")[0];

        // �Ȗ���UI�ɕ\������
        chartNameText.text = songNameText.text;
    }

    // �㉺�L�[�ŋȂ�I��
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

        // �Ȗ����擾����
        songNameText.text = chartFileNames[_currentChartIndex].Split(".json")[0];

        // �Ȗ���UI�ɕ\������
        chartNameText.text = songNameText.text;
    }

    // Enter�L�[�ŃQ�[���J�n
    public void OnKeyUp(KeyCode key)
    {
        if (key == KeyCode.Return)
        {
            // �I�ȉ�ʂ𓧖��ɂ���
            GetComponent<Canvas>().enabled = false;

            // ���ʃf�[�^��ݒ肷��
            adata.ready_to_start = true;
            ScoreManeger.score = 0;
            ScoreManeger.combo = 0;
            adata.chart = chartFileNames[_currentChartIndex].Split(".json")[0];

            // �Q�[����ʂɑJ�ڂ���
            Application.LoadLevel("GameScene");
        }
    }
}
