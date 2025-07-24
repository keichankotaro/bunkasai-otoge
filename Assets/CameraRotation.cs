using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraRotation : MonoBehaviour
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
        gameObject.transform.rotation = Quaternion.Euler(47.11f, 0f, 0f);
        //string loadjson = Resources.Load<TextAsset>("Charts/" + chart).ToString();  

        //jsonObj = JObject.Parse(adata.loadjson);  
        jsonObj = adata.jsonObj;
        try
        {
            Changes = (JArray)jsonObj["cameradata"]["changes"];
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
                    if (adata.game_time >= float.Parse(Changes[changed][0] + ""))
                    {
                        if (Changes[changed][1]+"" == "set")
                        {
                            //var currentRotation = gameObject.transform.rotation;
                            //currentRotation.x = (float)(47.11f * Math.Cos(Math.PI / 360 * float.Parse(Changes[changed][2]+"")));
                            //currentRotation.y = (float)(47.11f * Math.Sin(Math.PI / 360 * float.Parse(Changes[changed][2] + "")));
                            //gameObject.transform.rotation = new Quaternion(47.11f, 0f, float.Parse(Changes[changed][2] + ""), currentRotation.w);
                            gameObject.transform.rotation = Quaternion.Euler(47.11f, 0f, float.Parse(Changes[changed][2] + ""));
                            changed++;
                        }
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
