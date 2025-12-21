using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class SpeedManager : MonoBehaviour
{
    private JObject jsonObj;
    private JArray Changes;
    private JArray nullarray;
    private int i;
    private int changed = 0;
    private bool setupped = false;
    private bool change = false;

    // Start is called before the first frame update
    private void Setup()
    {
        changed = 0;
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
    }

    private IEnumerator DelayCoroutine(float seconds, UnityAction callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (adata.ready_to_start)
        {
            if (!setupped)
            {
                Setup();
                setupped = true;
            }
            if (change)
            {
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
                            float factor = CubicBezier.Evaluate(t, x1, y1, x2, y2);
                            //float currentMultiplier = Mathf.Lerp(adata.speed, targetMultiplier, factor);
                            //Debug.Log("t=" + t + ", startTime=" + startTime + ", duration=" + duration + ", targetMultiplier=" + targetMultiplier + ", factor=" + factor + ", nowSpeed=" + adata.speed);
                            adata.speed = adata.default_speed * factor;
                            //Debug.Log("speed=" + adata.speed + ", t=" + t);

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
                    }
                }
            }
        }
        else
        {
            setupped = false;
        }
    }

    private void OnDestroy()
    {
        // Destroy���ɓo�^����Invoke�����ׂăL�����Z��
        CancelInvoke();
    }
}
