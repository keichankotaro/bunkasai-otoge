using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Numerics;
using System.Linq;
using UnityEngine.Events;
using System.Runtime.CompilerServices;

public class L1 : MonoBehaviour
{
    public string[] data;
    public float arrsec;
    public float arrframe;
    public float speed;
    public int aid;
    private int id;
    public string json;
    public string loadjson;
    public JObject jsonObj;
    public float game_time = 0;
    public JObject cd;
    public float time_reming;
    public float now;
    private bool long_click = false;
    private bool long_clicked = false;
    private float endsec;
    private float length;
    private float tmptime;
    private float offset;

    private UnityEngine.Vector3 startPoint;
    private float sPoint;
    private UnityEngine.Vector3 endPoint;
    private float ePoint;
    private bool isLongNoteActive = false;

    private bool isPreviousObjectDestroyed = false;

    private bool EndTask = false;

    private String type;

    private Vector<Vector<float>> moves = new Vector<Vector<float>>();
    private bool s_change;
    private int change_count = 0;
    private JArray change;

    private GameObject debug;

    private bool LongNoteDetermined = false;
    private bool debugDataSent;

    float whereiam()
    {
        time_reming = (game_time - arrsec) * -1.0f;
        now = (time_reming * (speed / 2)) - 9.51f;
        return now;
    }

    private IEnumerator DelayCoroutine(float seconds, UnityAction callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    private async Task destroy_gameobject()
    {
        Transform myTransform = this.transform;
        UnityEngine.Vector3 pos = myTransform.position;
        pos.z = -40000f;
        myTransform.position = pos;

        int newId = id + adata.maximum_notes;

        if (newId > adata.l1notes)
        {
            Destroy(gameObject);
        }
        else
        {
            // ���̃m�[�g�̎�ނ�JSON����擾
            JObject nextNoteData = JObject.Parse(adata.jsonObj["chartdata"]["L1"][newId + ""] + "");
            string nextNoteType = nextNoteData["type"].ToString();

            // ��ނɉ��������������O��t����
            if (nextNoteType == "long")
            {
                name = "L1-long_" + newId;
            }
            else
            {
                name = "L1_" + newId;
            }

            await Initialize();
        }
    }

    private void destroy_gameobject_2()
    {
        Destroy(this.gameObject);
    }

    private int getName(int AttemptCount)
    {
        data = name.Split('_');
        try
        {
            id = int.Parse(data[1]);
        }
        catch
        {
            return getName(AttemptCount++);
        }

        if (AttemptCount < 50)
        {
            if (id == 0)
            {
                return getName(AttemptCount++);
            }
            else
            {
                return id;
            }
        }
        else
        {
            Destroy(this.gameObject);
            return 0;
        }
    }

    private void Init_0()
    {
        jsonObj = adata.jsonObj;
    }

    private async Task Initialize()
    {
        game_time = 0.0f;
        id = getName(0);
        //Debug.LogError("Hi! I am " + name);
        //Debug.Log("L1 id: " + id + " Name: " + name);
        cd = JObject.Parse(jsonObj["chartdata"]["L1"][id+""] + "");
        arrsec = float.Parse(cd["time"] + "");
        offset = float.Parse(jsonObj["maindata"]["offset"] + "");
        type = cd["type"] + "";
        speed = float.Parse(cd["speed"] + "") * (adata.speed * 10.0f);
        long_click = false;
        long_clicked = false;
        bool s;
        try
        {
            s = (bool)cd.SelectToken("s_change");
        }
        catch (ArgumentNullException e)
        {
            s = false;
        }

        if (s)
        {
            s_change = true;
            change = (JArray)cd["changes"];
        }
        else
        {
            s_change = false;
        }

        if (cd["type"] + "" == "long")
        {
            endsec = float.Parse(cd["endtime"] + "");
            length = (speed / 2) * (endsec - arrsec) / 2;
            GetComponent<Renderer>().material.color = new Color32(90, 255, 96, 255);
            this.transform.localScale = new UnityEngine.Vector3(0.7f, 0.01f, length);
            startPoint = transform.position;
            startPoint.z = -length / 2;
            endPoint = startPoint + new UnityEngine.Vector3(0, 0, length);
            isLongNoteActive = true;
            Transform initial_Transform = this.transform;
            UnityEngine.Vector3 initial_pos = initial_Transform.position;
            float init_z = (-3 - arrsec) * -1.0f;
            init_z = ((arrsec + 3) * speed) - 9.51f;
            initial_pos.z = init_z + (length / 2.0f);
            initial_pos.y = 0.0001f;
            initial_Transform.position = initial_pos;
        }
        else if (cd["type"] + "" == "tap")
        {
            isLongNoteActive = false;
            GetComponent<Renderer>().material.color = new Color32(0, 255, 232, 255);
            transform.localScale = new UnityEngine.Vector3(0.7f, 0.01f, 0.1f);
            Transform initial_Transform = this.transform;
            UnityEngine.Vector3 initial_pos = initial_Transform.position;
            initial_pos.z = whereiam();
            initial_pos.y = 0.0004f;
            initial_Transform.position = initial_pos;
        }

        Transform myTransform = this.transform;
        UnityEngine.Vector3 pos = myTransform.position;
        pos.z = -40000;
        myTransform.position = pos;
        LongNoteDetermined = false;
        EndTask = false;
        debugDataSent = false;
    }

    void Start()
    {
        Init_0();
        Initialize();
    }

    private async Task Update()
    {
        try
        {
            // if (tmptime >= 8 - offset)
            if (adata.ready_to_start && !EndTask)
            {
                speed = float.Parse(cd["speed"] + "") * (adata.speed * 10.0f);
                game_time = adata.game_time;
                if (s_change)
                {
                    if (change_count < change.Count)
                    {
                        if (change[change_count][0] + "" == "formula")
                        {
                            float startTime = float.Parse(change[change_count][1] + "");
                            float duration = float.Parse(change[change_count][2] + "");
                            float targetMultiplier = float.Parse(change[change_count][3] + "");
                            float x1 = float.Parse(change[change_count][4] + "");
                            float y1 = float.Parse(change[change_count][5] + "");
                            float x2 = float.Parse(change[change_count][6] + "");
                            float y2 = float.Parse(change[change_count][7] + "");
                            float t = (adata.game_time - startTime) / duration;
                            if (game_time >= startTime && game_time <= startTime + duration)
                            {
                                float factor = CubicBezier.Evaluate(t, x1, y1, x2, y2);
                                speed = Mathf.Lerp(1f, targetMultiplier, factor);
                            }
                            if (game_time > startTime + duration)
                            {
                                speed = targetMultiplier;
                                change_count++;
                            }
                        }
                        else
                        {
                            if (game_time >= float.Parse(change[change_count][0] + ""))
                            {
                                speed = float.Parse(change[change_count][1] + "");
                                change_count++;
                            }
                        }
                    }
                }
                if (type == "long")
                {
                    endsec = float.Parse(cd["endtime"] + "");
                    length = (adata.speed * 10.0f * float.Parse(cd["speed"] + "") * (endsec - arrsec)) / 2;
                    this.transform.localScale = new UnityEngine.Vector3(0.7f, 0.01f, length);
                }
                if (type == "tap")
                {
                    Transform myTransform = this.transform;
                    UnityEngine.Vector3 pos = myTransform.position;
                    float timelag = game_time - arrsec;
                    if (timelag < 0)
                    {
                        timelag = timelag * -1.0f;
                    }
                    pos.z = whereiam();
                    myTransform.position = pos;

                    if (isPreviousObjectDestroyed)
                    {
                        if (!adata.auto_play)
                        {
                            if (Input.GetKeyDown("d") || Input.GetKeyDown(adata.ControlerLine1))
                            {
                                if (timelag <= adata.perfect)
                                {
                                    GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                    await Task.Run(() =>
                                    {
                                        ScoreManeger.combo++;
                                        ScoreManeger.score = ScoreManeger.score + 5;
                                        ResultUI.Perfect++;
                                        adata.clicked_L1++;
                                        DebugText.judged_l1++;
                                        DebugText.score_l1 += 5;
                                        DebugText.perfect_l1++;
                                    });
                                    EndTask = true;
                                    await destroy_gameobject();
                                }
                                else if (timelag <= adata.good)
                                {
                                    GameObject h = Instantiate(adata.g, new UnityEngine.Vector3(8.97f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                    await Task.Run(() =>
                                    {
                                        ScoreManeger.combo++;
                                        ScoreManeger.score = ScoreManeger.score + 3;
                                        ResultUI.Good++;
                                        adata.clicked_L1++;
                                        DebugText.judged_l1++;
                                        DebugText.score_l1 += 3;
                                        DebugText.good_l1++;
                                    });
                                    EndTask = true;
                                    await destroy_gameobject();
                                }
                                else if (timelag <= adata.bad)
                                {
                                    GameObject h = Instantiate(adata.b, new UnityEngine.Vector3(9.02f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                    await Task.Run(() =>
                                    {
                                        ScoreManeger.combo = 0;
                                        ResultUI.Bad++;
                                        adata.clicked_L1++;
                                        DebugText.judged_l1++;
                                        DebugText.bad_l1++;
                                    });
                                    EndTask = true;
                                    await destroy_gameobject();
                                }
                            }
                            if (game_time > arrsec + adata.miss)
                            {
                                GameObject h = Instantiate(adata.m, new UnityEngine.Vector3(8.98f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                await Task.Run(() =>
                                {
                                    ScoreManeger.combo = 0;
                                    ResultUI.Miss++;
                                    adata.clicked_L1++;
                                    DebugText.judged_l1++;
                                    DebugText.miss_l1++;
                                });
                                EndTask = true;
                                await destroy_gameobject();
                            }
                        }
                        else
                        {
                            if (arrsec - game_time <= adata.auto)
                            {
                                GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                await Task.Run(() =>
                                {
                                    ScoreManeger.combo++;
                                    ScoreManeger.score += 5;
                                    ResultUI.Perfect++;
                                    adata.clicked_L1++;
                                    DebugText.judged_l1++;
                                    DebugText.score_l1 += 5;
                                    DebugText.perfect_l1++;
                                });
                                //adata.SECriPlayer.SetCue(adata.SEAcb, "SE-clap");
                                //adata.SECriPlayer.Start();
                                EndTask = true;
                                await destroy_gameobject();
                            }
                        }
                    }
                }
                if (isLongNoteActive)
                {
                    Transform myTransform = this.transform;
                    UnityEngine.Vector3 pos = myTransform.position;
                    if (LongNoteDetermined)
                    {
                        pos.z = whereiam() + (length / 2.0f);
                        myTransform.position = pos;
                        return;
                    }

                    float timelag = game_time - arrsec;
                    float end_timelag = game_time - endsec;
                    if (timelag < 0)
                    {
                        timelag *= -1.0f;
                    }
                    if (end_timelag < 0)
                    {
                        end_timelag *= -1.0f;
                    }

                    pos.z = whereiam() + (length / 2.0f);
                    myTransform.position = pos;
                    sPoint = pos.z - (length / 2.0f);
                    ePoint = pos.z + (length / 2.0f);

                    if (isPreviousObjectDestroyed)
                    {
                        if (long_click && (game_time >= arrsec))
                        {
                            if (!adata.auto_play)
                            {
                                if (Input.GetKey("d") || Input.GetKey(adata.ControlerLine1))
                                {
                                    long_click = true;
                                    if (game_time >= endsec - adata.longEndTimeLag && !LongNoteDetermined)
                                    {
                                        GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                        await Task.Run(() =>
                                        {
                                            ScoreManeger.combo++;
                                            ScoreManeger.score = ScoreManeger.score + 5;
                                            ResultUI.Perfect++;
                                            adata.clicked_L1++;
                                            DebugText.judged_l1++;
                                            DebugText.score_l1 += 5;
                                            DebugText.perfect_l1++;
                                        });
                                        EndTask = true;
                                        LongNoteDetermined = true;
                                        await destroy_gameobject();
                                        return;
                                    }
                                }
                                else if (Input.GetMouseButton(0))
                                {
                                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                    RaycastHit hit;

                                    if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == "CL1")
                                    {
                                        long_click = true;
                                        if (game_time >= endsec && !LongNoteDetermined)
                                        {
                                            GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                            h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                            await Task.Run(() =>
                                            {
                                                ScoreManeger.combo++;
                                                ScoreManeger.score = ScoreManeger.score + 5;
                                                ResultUI.Perfect++;
                                                adata.clicked_L1++;
                                                DebugText.judged_l1++;
                                                DebugText.score_l1 += 5;
                                                DebugText.perfect_l1++;
                                            });
                                            EndTask = true;
                                            LongNoteDetermined = true;
                                            await destroy_gameobject();
                                            return;
                                        }
                                    }
                                }
                                
                                else if (long_click && !LongNoteDetermined && !(Input.GetKey("d") || Input.GetKey(adata.ControlerLine1)) && !(Input.GetMouseButton(0)))
                                {
                                    if (game_time >= endsec - adata.perfect && game_time <= endsec + adata.perfect)
                                    {
                                        GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                        await Task.Run(() =>
                                        {
                                            ScoreManeger.combo++;
                                            ScoreManeger.score = ScoreManeger.score + 5;
                                            ResultUI.Perfect++;
                                            adata.clicked_L1++;
                                            DebugText.judged_l1++;
                                            DebugText.score_l1 += 5;
                                            DebugText.perfect_l1++;
                                        });
                                    }
                                    else if (game_time >= endsec - adata.good && game_time <= endsec + adata.good)
                                    {
                                        GameObject h = Instantiate(adata.g, new UnityEngine.Vector3(8.97f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                        await Task.Run(() =>
                                        {
                                            ScoreManeger.combo++;
                                            ScoreManeger.score = ScoreManeger.score + 3;
                                            ResultUI.Good++;
                                            adata.clicked_L1++;
                                            DebugText.judged_l1++;
                                            DebugText.score_l1 += 3;
                                            DebugText.good_l1++;
                                        });
                                    }
                                    else if (game_time >= endsec - adata.bad && game_time <= endsec + adata.bad)
                                    {
                                        GameObject h = Instantiate(adata.b, new UnityEngine.Vector3(9.02f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                        await Task.Run(() =>
                                        {
                                            ScoreManeger.combo = 0;
                                            ResultUI.Bad++;
                                            adata.clicked_L1++;
                                            DebugText.judged_l1++;
                                            DebugText.bad_l1++;
                                        });
                                    }
                                    else
                                    {
                                        GameObject h = Instantiate(adata.m, new UnityEngine.Vector3(8.98f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                        await Task.Run(() =>
                                        {
                                            ScoreManeger.combo = 0;
                                            ResultUI.Miss++;
                                            adata.clicked_L1++;
                                            DebugText.judged_l1++;
                                            DebugText.miss_l1++;
                                        });
                                    }
                                    LongNoteDetermined = true;
                                    EndTask = true;
                                    await destroy_gameobject();
                                    return;
                                }
                                else if (long_click && !LongNoteDetermined && game_time > endsec + adata.miss)
                                {
                                    GameObject h = Instantiate(adata.m, new UnityEngine.Vector3(8.98f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                    await Task.Run(() =>
                                    {
                                        ScoreManeger.combo = 0;
                                        ResultUI.Miss++;
                                        adata.clicked_L1++;
                                        DebugText.judged_l1++;
                                        DebugText.miss_l1++;
                                    });
                                    LongNoteDetermined = true;
                                    EndTask = true;
                                    await destroy_gameobject();
                                    return;
                                }
                            }
                            else
                            {
                                long_click = true;
                                if (game_time >= endsec - adata.longEndTimeLag && !LongNoteDetermined)
                                {
                                    GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                    await Task.Run(() =>
                                    {
                                        ScoreManeger.combo++;
                                        ScoreManeger.score += 5;
                                        ResultUI.Perfect++;
                                        adata.clicked_L1++;
                                        DebugText.judged_l1++;
                                        DebugText.score_l1 += 5;
                                        DebugText.perfect_l1++;
                                    });
                                    
                                    LongNoteDetermined = true;
                                    EndTask = true;
                                    await destroy_gameobject();
                                }
                            }
                        }

                        if (!adata.auto_play && !long_click)
                        {
                            if ((Input.GetKeyDown("d") || Input.GetKeyDown(adata.ControlerLine1)) && !long_click)
                            {
                                if (timelag <= adata.perfect)
                                {
                                    GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                    long_click = true;
                                    long_clicked = true;
                                    await Task.Run(() =>
                                    {
                                        ScoreManeger.combo++;
                                        ScoreManeger.score = ScoreManeger.score + 5;
                                        ResultUI.Perfect++;
                                        adata.clicked_L1++;
                                        DebugText.judged_l1++;
                                        DebugText.score_l1 += 5;
                                        DebugText.perfect_l1++;
                                    });
                                }
                                else if (timelag <= adata.good)
                                {
                                    GameObject h = Instantiate(adata.g, new UnityEngine.Vector3(8.97f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                    long_click = true;
                                    long_clicked = true;
                                    await Task.Run(() =>
                                    {
                                        ScoreManeger.combo++;
                                        ScoreManeger.score = ScoreManeger.score + 3;
                                        ResultUI.Good++;
                                        adata.clicked_L1++;
                                        DebugText.judged_l1++;
                                        DebugText.score_l1 += 3;
                                        DebugText.good_l1++;
                                    });
                                }
                                else if (timelag <= adata.bad)
                                {
                                    GameObject h = Instantiate(adata.b, new UnityEngine.Vector3(9.02f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                    isLongNoteActive = false;
                                    await Task.Run(() =>
                                    {
                                        ScoreManeger.combo = 0;
                                        ResultUI.Bad++;
                                        ResultUI.Miss++;
                                        adata.clicked_L1 += 2;
                                        DebugText.judged_l1 += 2;
                                        DebugText.bad_l1++;
                                        DebugText.miss_l1++;
                                    });
                                    EndTask = true;
                                    await destroy_gameobject();
                                }
                            }
                            else if (Input.GetMouseButtonDown(0) && !long_click)
                            {
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hit;

                                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == "CL1")
                                {
                                    if (timelag <= adata.perfect)
                                    {
                                        GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                        long_click = true;
                                        long_clicked = true;
                                        await Task.Run(() =>
                                        {
                                            ScoreManeger.combo++;
                                            ScoreManeger.score = ScoreManeger.score + 5;
                                            ResultUI.Perfect++;
                                            adata.clicked_L1++;
                                            DebugText.judged_l1++;
                                            DebugText.score_l1 += 5;
                                            DebugText.perfect_l1++;
                                        });
                                    }
                                    else if (timelag <= adata.good)
                                    {
                                        GameObject h = Instantiate(adata.g, new UnityEngine.Vector3(8.97f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                        long_click = true;
                                        long_clicked = true;
                                        await Task.Run(() =>
                                        {
                                            ScoreManeger.combo++;
                                            ScoreManeger.score = ScoreManeger.score + 3;
                                            ResultUI.Good++;
                                            adata.clicked_L1++;
                                            DebugText.judged_l1++;
                                            DebugText.score_l1 += 3;
                                            DebugText.good_l1++;
                                        });
                                    }
                                    else if (timelag <= adata.bad)
                                    {
                                        GameObject h = Instantiate(adata.b, new UnityEngine.Vector3(9.02f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                        isLongNoteActive = false;
                                        await Task.Run(() =>
                                        {
                                            ScoreManeger.combo = 0;
                                            ResultUI.Bad++;
                                            ResultUI.Miss++;
                                            adata.clicked_L1 += 2;
                                            DebugText.judged_l1 += 2;
                                            DebugText.bad_l1++;
                                            DebugText.miss_l1++;
                                        });
                                        EndTask = true;
                                        await destroy_gameobject();
                                    }
                                }
                            }
                            if (game_time > arrsec + adata.miss && !long_click)
                            {
                                GameObject h = Instantiate(adata.m, new UnityEngine.Vector3(8.98f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                isLongNoteActive = false;
                                await Task.Run(() =>
                                {
                                    ScoreManeger.combo = 0;
                                    ResultUI.Miss += 2;
                                    adata.clicked_L1 += 2;
                                    DebugText.judged_l1 += 2;
                                    DebugText.miss_l1 += 2;
                                });
                                EndTask = true;
                                await destroy_gameobject();
                            }
                        }
                        else if (adata.auto_play && !long_click)
                        {
                            if (arrsec - game_time <= adata.auto)
                            {
                                GameObject h = Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                h.GetComponent<goodbye>().SetInstantiatorName(name.Split("_")[1] + "", 1);
                                long_click = true;
                                long_clicked = true;
                                await Task.Run(() =>
                                {
                                    ScoreManeger.combo++;
                                    ScoreManeger.score += 5;
                                    ResultUI.Perfect++;
                                    adata.clicked_L1++;
                                    DebugText.judged_l1++;
                                    DebugText.score_l1 += 5;
                                    DebugText.perfect_l1++;
                                });
                                //adata.SECriPlayer.SetCue(adata.SEAcb, "SE-clap");
                                //adata.SECriPlayer.Start();
                            }
                        }
                    }
                }
            }
            else
            {
                tmptime += Time.deltaTime;
                Transform myTransform = this.transform;
                UnityEngine.Vector3 pos = myTransform.position;
                float timelag = game_time - arrsec;
                pos.z = whereiam();
                myTransform.position = pos;
            }
            int nf = Time.frameCount;
            float nt = Time.time;

            if (id != 0)
            {
                GameObject obj = GameObject.Find("L1_" + (id - 1));
                if (obj != null)
                {

                }
                else
                {
                    obj = null;
                }
                GameObject long_obj = GameObject.Find("L1-long_" + (id - 1));
                if (long_obj != null)
                {

                }
                else
                {
                    long_obj = null;
                }

                if (obj == null && long_obj == null)
                {
                    isPreviousObjectDestroyed = true;
                    if (!debugDataSent)
                    {
                        JObject root = new JObject();
                        root["source"] = "L1";
                        root["payload"] = cd;
                        DebugSocketClient.Instance.SendData(root.ToString());
                        debugDataSent = true;
                    }
                }
                else
                {
                    isPreviousObjectDestroyed = false;
                }
            }
            else
            {
                isPreviousObjectDestroyed = true;
            }
        }
        catch (Exception JsonReaderException)
        {
            Destroy(this.gameObject);
        }
    }
}
