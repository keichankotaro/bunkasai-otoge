using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class SpeedManager : MonoBehaviour
{
    private JObject jsonObj;
    // 速度変化のデータ
    private JArray Changes;
    private JArray nullarray;
    private int i;
    // 変化回数カウント
    private int changed = 0;
    // セットアップ完了フラグ
    private bool setupped = false;
    // 変化の有無
    private bool change = false;
    
    private bool inFormula = false;
    private float formulaStartMultiplier = 1.0f;
    
    public GameObject speedChangesUI;
    private TMPro.TextMeshProUGUI fromText;
    private TMPro.TextMeshProUGUI toText;
    private float lastChangeEndTime = -999f;
    private bool enableSoflanPreview = false;

    // Start is called before the first frame update
    private void Setup()
    {
        // ここからセットアップ
        changed = 0;
        lastChangeEndTime = -999f;
        inFormula = false;
        
        string chart = adata.chart;
        //string loadjson = Resources.Load<TextAsset>("Charts/" + chart).ToString();

        //jsonObj = JObject.Parse(adata.loadjson);
        jsonObj = adata.jsonObj;
        try
        {
            Changes = (JArray)jsonObj["speeddata"]["changes"];
            change = true;
        }
        catch (NullReferenceException)
        {
            change = false;
        }

        string settingsFilePath = System.IO.Path.Combine(Application.persistentDataPath, "settings.json");
        enableSoflanPreview = false;
        if (System.IO.File.Exists(settingsFilePath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(settingsFilePath);
                JObject settings = JObject.Parse(json);
                enableSoflanPreview = settings["enableSoflanPreview"]?.Value<bool>() ?? false;
            }
            catch (Exception) {}
        }

        if (speedChangesUI != null)
        {
            fromText = speedChangesUI.transform.Find("From")?.GetComponent<TMPro.TextMeshProUGUI>();
            toText = speedChangesUI.transform.Find("To")?.GetComponent<TMPro.TextMeshProUGUI>();
            speedChangesUI.SetActive(false);
        }
        // ここまでセットアップ
    }

    private IEnumerator DelayCoroutine(float seconds, UnityAction callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        // ここからスピード更新処理
        if (adata.ready_to_start)
        {
            if (!setupped)
            {
                Setup();
                setupped = true;
            }
            if (change)
            {
                // Calculate UI visibility and texts
                bool shouldShowUI = false;
                float curMult = adata.speed / adata.default_speed;
                string targetText = "";
                Color targetColor = Color.white;

                if (enableSoflanPreview)
                {
                    if (inFormula)
                    {
                        shouldShowUI = true;
                        if (changed < Changes.Count)
                        {
                            float targetMultiplier = float.Parse(Changes[changed][3] + "");
                            targetText = "x" + targetMultiplier.ToString("F2");
                            if (targetMultiplier > curMult) targetColor = new Color(1f, 0.4f, 0.4f);
                            else if (targetMultiplier < curMult) targetColor = new Color(0.4f, 0.8f, 1f);
                        }
                    }
                    else if (changed < Changes.Count)
                    {
                        float nextTime = (Changes[changed][0]+"" == "formula") ? float.Parse(Changes[changed][1]+"") : float.Parse(Changes[changed][0]+"");
                        float nextMult = (Changes[changed][0]+"" == "formula") ? float.Parse(Changes[changed][3]+"") : float.Parse(Changes[changed][1]+"");

                        if (adata.game_time >= nextTime - 2.0f && adata.game_time <= nextTime + 0.1f) // Ensure it's active exactly until it starts, or slightly after
                        {
                            shouldShowUI = true;
                            targetText = "x" + nextMult.ToString("F2");
                            if (nextMult > curMult) targetColor = new Color(1f, 0.4f, 0.4f);
                            else if (nextMult < curMult) targetColor = new Color(0.4f, 0.8f, 1f);
                        }
                    }
                    
                    if (!shouldShowUI && adata.game_time <= lastChangeEndTime + 1.0f)
                    {
                        shouldShowUI = true;
                    }
                }

                if (speedChangesUI != null)
                {
                    if (shouldShowUI)
                    {
                        if (!speedChangesUI.activeSelf) speedChangesUI.SetActive(true);
                        if (fromText != null) fromText.text = "x" + curMult.ToString("F4");
                        if (toText != null && targetText != "")
                        {
                            toText.text = targetText;
                            toText.color = targetColor;
                        }
                    }
                    else
                    {
                        if (speedChangesUI.activeSelf) speedChangesUI.SetActive(false);
                    }
                }

                ////Debug.LogWarning("speed=" + adata.speed);
                if (changed < Changes.Count)
                {
                    if (Changes[changed][0]+"" == "formula")
                    {
                        float startTime = float.Parse(Changes[changed][1] + "");
                        float duration = float.Parse(Changes[changed][2] + "");
                        float targetMultiplier = float.Parse(Changes[changed][3] + "");
                        float x1 = float.Parse(Changes[changed][4] + "");
                        float y1 = float.Parse(Changes[changed][5] + "");
                        float x2 = float.Parse(Changes[changed][6] + "");
                        float y2 = float.Parse(Changes[changed][7] + "");

                        float t = (adata.game_time - startTime) / duration;
                        
                        if (t >= 0f && t <= 1f)
                        {
                            if (!inFormula)
                            {
                                formulaStartMultiplier = adata.speed / adata.default_speed;
                                inFormula = true;
                            }

                            float factor = CubicBezier.Evaluate(t, x1, y1, x2, y2);
                            float currentMultiplier = Mathf.Lerp(formulaStartMultiplier, targetMultiplier, factor);
                            adata.speed = adata.default_speed * currentMultiplier;

                            JObject payload = new JObject();
                            payload["event"] = "formula_update";
                            payload["new_speed"] = adata.speed;
                            payload["t"] = t;
                            payload["factor"] = factor;
                            payload["start_time"] = startTime;
                            payload["duration"] = duration;
                            payload["target_multiplier"] = targetMultiplier;
                            payload["x1"] = x1;
                            payload["y1"] = y1;
                            payload["x2"] = x2;
                            payload["y2"] = y2;
                            JObject root = new JObject();
                            root["source"] = "SpeedManager";
                            root["payload"] = payload;
                            DebugSocketClient.Instance.SendData(root.ToString());
                        }
                        if (t > 1f)
                        {
                            adata.speed = adata.default_speed * targetMultiplier;
                            changed++;
                            inFormula = false;
                            lastChangeEndTime = adata.game_time;

                            JObject payload = new JObject();
                            payload["event"] = "formula_end";
                            payload["new_speed"] = adata.speed;
                            payload["target_multiplier"] = targetMultiplier;
                            JObject root = new JObject();
                            root["source"] = "SpeedManager";
                            root["payload"] = payload;
                            DebugSocketClient.Instance.SendData(root.ToString());
                        }
                    }
                    else if (adata.game_time >= float.Parse(Changes[changed][0] + ""))
                    {
                        float multiplier = float.Parse(Changes[changed][1] + "");
                        adata.speed = adata.default_speed * multiplier;
                        
                        JObject payload = new JObject();
                        payload["event"] = "time_trigger";
                        payload["new_speed"] = adata.speed;
                        payload["trigger_time"] = float.Parse(Changes[changed][0] + "");
                        payload["multiplier"] = multiplier;
                        JObject root = new JObject();
                        root["source"] = "SpeedManager";
                        root["payload"] = payload;
                        DebugSocketClient.Instance.SendData(root.ToString());

                        changed++;
                        lastChangeEndTime = adata.game_time;
                    }
                }
            }
        }
        else
        {
            setupped = false;
            if (speedChangesUI != null && speedChangesUI.activeSelf)
            {
                speedChangesUI.SetActive(false);
            }
        }
        // ここまでスピード更新処理
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
