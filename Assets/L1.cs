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

public class L1 : MonoBehaviour
{
    public string[] data;
    public float arrsec;
    public float arrframe;
    public float speed;
    public int aid;
    private int id;
    public string json;
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

    private String type;

    private Vector<Vector<float>> moves = new Vector<Vector<float>>();
    private bool s_change;
    private int change_count = 0;
    private JArray change;

    private GameObject debug;
    float whereiam()
    {
        time_reming = (game_time - arrsec) * -1.0f;
        now = (time_reming * (speed / 2)) - 9.51f;
        return now;
    }

    private async Task destroy_gameobject()
    {
        Destroy(this.gameObject);
    }

    private IEnumerator Initialize()
    {
        string chart = adata.chart;
        game_time = 0.0f;
        data = name.Split('_');
        try
        {
            id = int.Parse(data[1]);
        }
        catch (IndexOutOfRangeException e)
        {
            if (name.Contains("Clone"))
            {
                Destroy(this.gameObject);
            }
        }
        string loadjson = Resources.Load<TextAsset>("Charts/" + chart).ToString();
        JObject jsonObj = JObject.Parse(loadjson);
        cd = JObject.Parse(jsonObj["chartdata"]["L1"][id.ToString()] + "");
        arrsec = float.Parse(cd["time"] + "");
        offset = float.Parse(jsonObj["maindata"]["offset"] + "") / 1000;
        type = cd["type"] + "";
        speed = float.Parse(cd["speed"] + "") * (adata.speed * 10.0f);
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
            initial_Transform.position = initial_pos;
        }
        else if (cd["type"] + "" == "tap")
        {
            Transform initial_Transform = this.transform;
            UnityEngine.Vector3 initial_pos = initial_Transform.position;
            initial_pos.z = whereiam();
            initial_Transform.position = initial_pos;
        }

        Transform myTransform = this.transform;
        UnityEngine.Vector3 pos = myTransform.position;
        pos.z = -40000;
        myTransform.position = pos;
        yield return null;
    }

    void Start()
    {
        StartCoroutine(Initialize());
    }

    private async Task Update()
    {
        // if (tmptime >= 8 - offset)
        if (adata.ready_to_start)
        {
            speed = float.Parse(cd["speed"] + "") * (adata.speed * 10.0f);
            game_time = adata.game_time;
            if (s_change)
            {
                if (change_count < change.Count)
                {
                    if (game_time >= float.Parse(change[change_count][0] + ""))
                    {
                        speed = float.Parse(change[change_count][1] + "") * (adata.speed * 10.0f);
                        change_count++;
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
                        if (Input.GetKeyDown("d") || Input.GetKeyDown(KeyCode.Joystick1Button0))
                        {
                            if (timelag <= adata.perfect)
                            {
                                Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo++;
                                ScoreManeger.score = ScoreManeger.score + 5;
                                ResultUI.Perfect++;
                                adata.clicked_L1++;
                                destroy_gameobject();
                            }
                            else if (timelag <= adata.good)
                            {
                                Instantiate(adata.g, new UnityEngine.Vector3(8.97f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo++;
                                ScoreManeger.score = ScoreManeger.score + 3;
                                ResultUI.Good++;
                                adata.clicked_L1++;
                                destroy_gameobject();
                            }
                            else if (timelag <= adata.bad)
                            {
                                Instantiate(adata.b, new UnityEngine.Vector3(9.02f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo = 0;
                                ResultUI.Bad++;
                                adata.clicked_L1++;
                                destroy_gameobject();
                            }
                        }
                        else if (Input.GetMouseButtonDown(0))
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;

                            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == "CL1")
                            {
                                if (timelag <= adata.perfect)
                                {
                                    Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 5;
                                    ResultUI.Perfect++;
                                    adata.clicked_L1++;
                                    destroy_gameobject();
                                }
                                else if (timelag <= adata.good)
                                {
                                    Instantiate(adata.g, new UnityEngine.Vector3(8.97f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 3;
                                    ResultUI.Good++;
                                    adata.clicked_L1++;
                                    destroy_gameobject();
                                }
                                else if (timelag <= adata.bad)
                                {
                                    Instantiate(adata.b, new UnityEngine.Vector3(9.02f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    ScoreManeger.combo = 0;
                                    ResultUI.Bad++;
                                    adata.clicked_L1++;
                                    destroy_gameobject();
                                }
                            }
                        }
                        if (game_time > arrsec + adata.miss)
                        {
                            Instantiate(adata.m, new UnityEngine.Vector3(8.98f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                            ScoreManeger.combo = 0;
                            ResultUI.Miss++;
                            adata.clicked_L1++;
                            destroy_gameobject();
                        }
                    }
                    else
                    {
                        if (arrsec - game_time <= adata.auto)
                        {
                            Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                            ScoreManeger.combo++;
                            ScoreManeger.score += 5;
                            ResultUI.Perfect++;
                            adata.clicked_L1++;
                            destroy_gameobject();
                        }
                    }
                }
            }
            if (isLongNoteActive)
            {
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

                Transform myTransform = this.transform;
                UnityEngine.Vector3 pos = myTransform.position;
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
                            if (Input.GetKey("d") || Input.GetKey(KeyCode.Joystick1Button0))
                            {
                                long_click = true;
                                if (game_time >= endsec)
                                {
                                    Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 5;
                                    ResultUI.Perfect++;
                                    adata.clicked_L1++;
                                    destroy_gameobject();
                                }
                            }
                            else if (Input.GetMouseButton(0))
                            {
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hit;

                                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == "CL1")
                                {
                                    long_click = true;
                                    if (game_time >= endsec)
                                    {
                                        Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        ScoreManeger.combo++;
                                        ScoreManeger.score = ScoreManeger.score + 5;
                                        ResultUI.Perfect++;
                                        adata.clicked_L1++;
                                        destroy_gameobject();
                                    }
                                }
                            }
                            else
                            {
                                long_click = false;
                                Instantiate(adata.m, new UnityEngine.Vector3(8.98f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo = 0;
                                ResultUI.Miss++;
                                adata.clicked_L1++;
                                destroy_gameobject();
                            }
                        }
                        else
                        {
                            long_click = true;
                            if (game_time >= endsec)
                            {
                                Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo++;
                                ScoreManeger.score += 5;
                                ResultUI.Perfect++;
                                adata.clicked_L1++;
                                destroy_gameobject();
                            }
                        }
                    }

                    if (!adata.auto_play)
                    {
                        if ((Input.GetKeyDown("d") || Input.GetKeyDown(KeyCode.Joystick1Button0)) && !long_click)
                        {
                            if (timelag <= adata.perfect)
                            {
                                Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                long_click = true;
                                long_clicked = true;
                                ScoreManeger.combo++;
                                ScoreManeger.score = ScoreManeger.score + 5;
                                ResultUI.Perfect++;
                                adata.clicked_L1++;
                            }
                            else if (timelag <= adata.good)
                            {
                                Instantiate(adata.g, new UnityEngine.Vector3(8.97f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                long_click = true;
                                long_clicked = true;
                                ScoreManeger.combo++;
                                ScoreManeger.score = ScoreManeger.score + 3;
                                ResultUI.Good++;
                                adata.clicked_L1++;
                            }
                            else if (timelag <= adata.bad)
                            {
                                Instantiate(adata.b, new UnityEngine.Vector3(9.02f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                isLongNoteActive = false;
                                ScoreManeger.combo = 0;
                                ResultUI.Bad++;
                                ResultUI.Miss++;
                                adata.clicked_L1 += 2;
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
                                    Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    long_click = true;
                                    long_clicked = true;
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 5;
                                    ResultUI.Perfect++;
                                    adata.clicked_L1++;
                                }
                                else if (timelag <= adata.good)
                                {
                                    Instantiate(adata.g, new UnityEngine.Vector3(8.97f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    long_click = true;
                                    long_clicked = true;
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 3;
                                    ResultUI.Good++;
                                    adata.clicked_L1++;
                                }
                                else if (timelag <= adata.bad)
                                {
                                    Instantiate(adata.b, new UnityEngine.Vector3(9.02f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    isLongNoteActive = false;
                                    ScoreManeger.combo = 0;
                                    ResultUI.Bad++;
                                    ResultUI.Miss++;
                                    adata.clicked_L1 += 2;
                                    destroy_gameobject();
                                }
                            }
                        }
                        if (game_time > arrsec + adata.miss && !long_click)
                        {
                            Instantiate(adata.m, new UnityEngine.Vector3(8.98f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                            isLongNoteActive = false;
                            ScoreManeger.combo = 0;
                            ResultUI.Miss += 2;
                            adata.clicked_L1 += 2;
                            destroy_gameobject();
                        }
                    }
                    else
                    {
                        if (arrsec - game_time <= adata.auto && !long_click)
                        {
                            Instantiate(adata.p, new UnityEngine.Vector3(8.875f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                            long_click = true;
                            long_clicked = true;
                            ScoreManeger.combo++;
                            ScoreManeger.score += 5;
                            ResultUI.Perfect++;
                            adata.clicked_L1++;
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

            if (obj != null || long_obj != null)
            {
                isPreviousObjectDestroyed = false;
            }
            else
            {
                isPreviousObjectDestroyed = true;
            }
        }
        else
        {
            isPreviousObjectDestroyed = true;
        }
    }
}
