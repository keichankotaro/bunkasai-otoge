using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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
        string chart = adata.chart;
        string loadjson = Resources.Load<TextAsset>("Charts/" + chart).ToString();

        jsonObj = JObject.Parse(loadjson);
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
                    if (adata.game_time >= float.Parse(Changes[changed][0] + ""))
                    {
                        adata.speed = adata.default_speed * float.Parse(Changes[changed][1] + "");
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
        // DestroyŽž‚É“o˜^‚µ‚½Invoke‚ð‚·‚×‚ÄƒLƒƒƒ“ƒZƒ‹
        CancelInvoke();
    }
}
