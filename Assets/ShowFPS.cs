using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class ShowFPS : MonoBehaviour
{
    private float fps;
    public GameObject FPSText;
    public GameObject graphObject; // LineRendererがアタッチされるオブジェクト
    private LineRenderer lineRenderer;
    private int maxDataPoints = 100; // グラフに表示する最大データ点
    private List<float> fpsData = new List<float>(); // FPSデータを格納するリスト
    private float ax = 3f; // グラフのX軸のオフセット
    private float by = -2.4f; // グラフのY軸のオフセット
    private TMPro.TextMeshProUGUI fpsText;
    private float refreshTimer = 0f;
    [SerializeField] private float refreshInterval = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        fpsText = FPSText.GetComponent<TMPro.TextMeshProUGUI>();
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
        fps = 1f / Time.unscaledDeltaTime;
        refreshTimer += Time.unscaledDeltaTime;

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

        if (refreshTimer < refreshInterval)
        {
            return;
        }

        refreshTimer = 0f;

        if (!adata.showFPS)
        {
            if (graphObject.activeSelf)
            {
                graphObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(fpsText.text))
            {
                fpsText.text = string.Empty;
            }

            fpsData.Clear();
            return;
        }

        graphObject.SetActive(true);

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

        fpsText.text = $"FPS (Press F3 to invisible): {fps:0.0}";
    }
}
