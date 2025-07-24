using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class ShowFPS : MonoBehaviour
{
    private float fps;
    public GameObject FPSText;
    public GameObject graphObject; // LineRendererをアタッチするオブジェクト
    private LineRenderer lineRenderer;
    private int maxDataPoints = 100; // グラフに表示する最大データ点数
    private List<float> fpsData = new List<float>(); // FPSデータを格納するリスト
    private float ax = 3f; // グラフのX軸のオフセット
    private float by = -2.4f; // グラフのY軸のオフセット

    // Start is called before the first frame update
    void Start()
    {
        // LineRendererの初期設定
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
        // FPSデータの更新
        fpsData.Add(fps);
        if (fpsData.Count > maxDataPoints)
        {
            fpsData.RemoveAt(0); // 古いデータを削除
        }

        // グラフの描画
        for (int i = 0; i < fpsData.Count; i++)
        {
            float x = i * (graphObject.transform.localScale.x / maxDataPoints) + ax;
            float y = fpsData[i] * (graphObject.transform.localScale.y / 100f) + by; // 100FPSを最大値としてスケーリング
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

        // FPS表示の処理
        if (adata.showFPS)
        {
            FPSText.GetComponent<TextMeshProUGUI>().text = "FPS (Press F3 to invisible): " + fps;
            graphObject.SetActive(true); // グラフを表示
        }
        else
        {
            FPSText.GetComponent<TextMeshProUGUI>().text = "";
            graphObject.SetActive(false); // グラフを非表示
        }
    }
}
