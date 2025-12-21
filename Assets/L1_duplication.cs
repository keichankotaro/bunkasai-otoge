using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class L1_duplication : MonoBehaviour
{
    public GameObject prefab;
    public GameObject prefab_long;
    private int notecnt;
    private List<GameObject> objs;
    private JObject jsonObj;
    private int tmpnotes;
    private int now_count = 0;
    public static bool started = false;
    private AsyncInstantiateOperation<GameObject> newObjOp;
    private GameObject newObj;

    // Start is called before the first frame update
    /*
    void Start()
    {
        string chart = adata.chart;
        string loadjson = Resources.Load<TextAsset>("Charts/" + chart).ToString();

        jsonObj = JObject.Parse(loadjson);
        notecnt = int.Parse(jsonObj["chartdata"]["L1"]["L1data"]["notes"] + "");
        adata.l1notes += notecnt;
        objs = new List<GameObject>();
        if (notecnt > 50)
        {
            for (int i = 1; i <= 50; i++)
            {
                if (jsonObj["chartdata"]["L1"][i + ""]["type"].ToString() == "tap")
                {
                    objs.Add((GameObject)Instantiate(prefab));
                    objs[i - 1].name = "L1_" + i;
                }
                else if (jsonObj["chartdata"]["L1"][i + ""]["type"].ToString() == "long")
                {
                    objs.Add((GameObject)Instantiate(prefab_long));
                    objs[i - 1].name = "L1-long_" + i;
                }
            }
            now_count += 50;
        }
        else if (notecnt <= 50)
        {
            for (int i = 1; i <= notecnt; i++)
            {
                if (jsonObj["chartdata"]["L1"][i + ""]["type"].ToString() == "tap")
                {
                    objs.Add((GameObject)Instantiate(prefab));
                    objs[i - 1].name = "L1_" + i;
                }
                else if (jsonObj["chartdata"]["L1"][i + ""]["type"].ToString() == "long")
                {
                    objs.Add((GameObject)Instantiate(prefab_long));
                    objs[i - 1].name = "L1-long_" + i;
                }
            }
            now_count += notecnt;
        }
    }
    */

    private async Task Start_clone()
    {
        jsonObj = adata.jsonObj;
        notecnt = int.Parse(jsonObj["chartdata"]["L1"]["L1data"]["notes"] + "");
        adata.l1notes += notecnt;
        objs = new List<GameObject>();
        if (notecnt > adata.maximum_notes)
        {
            for (int i = 1; i <= adata.maximum_notes; i++)
            {
                if (jsonObj["chartdata"]["L1"][i + ""]["type"].ToString() == "tap")
                {
                    objs.Add((GameObject)Instantiate(prefab));
                    objs[i - 1].name = "L1_" + i;
                    adata.LastID_L1 = i;
                }
                else if (jsonObj["chartdata"]["L1"][i + ""]["type"].ToString() == "long")
                {
                    objs.Add((GameObject)Instantiate(prefab_long));
                    objs[i - 1].name = "L1-long_" + i;
                    adata.LastID_L1 = i;
                }
            }
            now_count += adata.maximum_notes;
        }
        else if (notecnt <= adata.maximum_notes)
        {
            for (int i = 1; i <= notecnt; i++)
            {
                if (jsonObj["chartdata"]["L1"][i + ""]["type"].ToString() == "tap")
                {
                    objs.Add((GameObject)Instantiate(prefab));
                    objs[i - 1].name = "L1_" + i;
                    adata.LastID_L1 = i;
                }
                else if (jsonObj["chartdata"]["L1"][i + ""]["type"].ToString() == "long")
                {
                    objs.Add((GameObject)Instantiate(prefab_long));
                    objs[i - 1].name = "L1-long_" + i;
                    adata.LastID_L1 = i;
                }
            }
            now_count += notecnt;
        }
        L1_duplication.started = true;
    }

    private async Task add_notes(int notecnt, List<GameObject> objs, JObject jsonObj, GameObject prefab, GameObject prefab_long)
    {
        // None
        if (notecnt > adata.maximum_notes && notecnt <= adata.l1notes)
        {
            if (objs.Count != (adata.maximum_notes + adata.clicked_L1))
            {
                if (jsonObj["chartdata"]["L1"][(adata.maximum_notes + adata.clicked_L1) + ""]["type"].ToString() == "tap")
                {
                    objs.Add((GameObject)Instantiate(prefab));
                    objs[(adata.maximum_notes + adata.clicked_L1) - 1].name = "L1_" + (adata.maximum_notes + adata.clicked_L1);
                    adata.LastID_L1 = adata.maximum_notes + adata.clicked_L1;
                }
                else if (jsonObj["chartdata"]["L1"][(adata.maximum_notes + adata.clicked_L1) + ""]["type"].ToString() == "long")
                {
                    objs.Add((GameObject)Instantiate(prefab_long));
                    objs[(adata.maximum_notes + adata.clicked_L1) - 1].name = "L1-long_" + (adata.maximum_notes + adata.clicked_L1);
                    adata.LastID_L1 = adata.maximum_notes + adata.clicked_L1;
                }
                now_count++;
                Transform myTransform = objs[now_count].transform;
                Vector3 pos = myTransform.position;
                pos.z = 1000;
                myTransform.position = pos;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (adata.ready_to_start)
        {
            if (!started)
            {
                Start_clone();
            }

            if (started)
            {
                //Task.Run(() => add_notes(notecnt, objs, jsonObj, prefab, prefab_long));
                //add_notes(notecnt, objs, jsonObj, prefab, prefab_long);
                // None
                /*
                if (notecnt > adata.maximum_notes)
                {
                    if (objs.Count != (adata.maximum_notes + adata.clicked_L1))
                    {
                        tmpnotes++;
                        //////Debug.LogWarning(tmpnotes);
                        if (tmpnotes == 5)
                        {
                            tmpnotes = 0;
                            for (int i = 0; i <= 5; i++)
                            {
                                //////Debug.LogWarning("i=" + i);
                                if (jsonObj["chartdata"]["L1"][(adata.maximum_notes + adata.clicked_L1 + i) + ""]["type"].ToString() == "tap")
                                {
                                    objs.Add((GameObject)Instantiate(prefab));
                                    objs[(adata.maximum_notes + adata.clicked_L1 + i) - 1].name = "L1_" + (adata.maximum_notes + adata.clicked_L1 + i);
                                }
                                else if (jsonObj["chartdata"]["L1"][(adata.maximum_notes + adata.clicked_L1 + i) + ""]["type"].ToString() == "long")
                                {
                                    objs.Add((GameObject)Instantiate(prefab_long));
                                    objs[(adata.maximum_notes + adata.clicked_L1 + i) - 1].name = "L1-long_" + (adata.maximum_notes + adata.clicked_L1 + i);
                                }
                                Transform myTransform = objs[adata.maximum_notes + adata.clicked_L1 + i].transform;
                                Vector3 pos = myTransform.position;
                                pos.z = 1000000;
                                myTransform.position = pos;
                            }
                        }
                    }
                }
                */
            }
        }
        else
        {
            started = false;
            objs = null;
        }
    }
}