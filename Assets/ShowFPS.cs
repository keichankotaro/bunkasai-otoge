using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class ShowFPS : MonoBehaviour
{
    private float fps;
    public GameObject FPSText;
    public GameObject graphObject; // LineRenderer���A�^�b�`����I�u�W�F�N�g
    private LineRenderer lineRenderer;
    private int maxDataPoints = 100; // �O���t�ɕ\������ő�f�[�^�_��
    private List<float> fpsData = new List<float>(); // FPS�f�[�^���i�[���郊�X�g
    private float ax = 3f; // �O���t��X���̃I�t�Z�b�g
    private float by = -2.4f; // �O���t��Y���̃I�t�Z�b�g

    // Start is called before the first frame update
    void Start()
    {
        // LineRenderer�̏����ݒ�
        lineRenderer = graphObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.02f;
        lineRenderer.positionCount = maxDataPoints;
        graphObject.transform.rotation = Quaternion.Euler(47.11f, 0, 0);
        lineRenderer.transform.rotation = Quaternion.Euler(47.11f, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // FPS�f�[�^�̍X�V
        fpsData.Add(fps);
        if (fpsData.Count > maxDataPoints)
        {
            fpsData.RemoveAt(0); // �Â��f�[�^���폜
        }

        // �O���t�̕`��
        for (int i = 0; i < fpsData.Count; i++)
        {
            float x = i * (graphObject.transform.localScale.x / maxDataPoints) + ax;
            float y = fpsData[i] * (graphObject.transform.localScale.y / 100f) + by; // 100FPS���ő�l�Ƃ��ăX�P�[�����O
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }

        fps = 1f / Time.deltaTime;
        if (name == "Main Camera")
        {
            if (adata.ready_to_start)
            {
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    Debug.Log("push f3");
                    if (adata.showFPS)
                    {
                        adata.showFPS = false;
                    }
                    else
                    {
                        adata.showFPS = true;
                    }
                }
            }
        }
        else
        {
            if (!adata.ready_to_start)
            {
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    Debug.Log("push f3");
                    if (adata.showFPS)
                    {
                        adata.showFPS = false;
                    }
                    else
                    {
                        adata.showFPS = true;
                    }
                }
            }
        }

        // FPS�\���̏���
        if (adata.showFPS)
        {
            FPSText.GetComponent<TextMeshProUGUI>().text = "FPS (Press F3 to invisible): " + fps;
            graphObject.SetActive(true); // �O���t��\��
        }
        else
        {
            FPSText.GetComponent<TextMeshProUGUI>().text = "";
            graphObject.SetActive(false); // �O���t���\��
        }
    }
}
