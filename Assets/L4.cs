using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor;

public class L4 : MonoBehaviour
{
    public string[] data;
    public float arrsec;
    public float arrframe;
    public float speed;
    public int aid;
    public int id;
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

    // Om[c̎n_ƏI_̍W
    private Vector3 startPoint;
    private float sPoint;
    private Vector3 endPoint;
    private float ePoint;
    private bool isLongNoteActive = false;

    private bool isPreviousObjectDestroyed = false;

    private bool s_change;
    private int change_count = 0;
    private JArray change;

    private String type;

    float whereiam()
    {
        // »݂¢Ȃ¯ê΂ȂçȂ¢ꏊð擾·éB
        ////Debug.Log(arrsec + "");
        time_reming = (game_time - arrsec) * -1.0f;
        ////Debug.Log("Time reming: "+ time_reming);
        now = (time_reming * (speed / 2)) - 9.51f;
        return now;
    }

    private async Task destroy_gameobject()
    {
        Destroy(this.gameObject);
    }

    // Start is called before the first frame update
    private IEnumerator Initialize()
    {
        string chart = adata.chart;
        game_time = 0.0f;
        string[] data = name.Split('_');
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
        cd = JObject.Parse(jsonObj["chartdata"]["L4"][id.ToString()] + "");
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
        //game_time = game_time - offset;
        if (cd["type"] + "" == "long")
        {
            endsec = float.Parse(cd["endtime"] + "");
            length = (speed / 2) * (endsec - arrsec) / 2;
            this.transform.localScale = new Vector3(0.7f, 0.01f, length);
            startPoint = transform.position;
            startPoint.z = -length / 2;
            endPoint = startPoint + new Vector3(0, 0, length);
            isLongNoteActive = true;
            Transform initial_Transform = this.transform;
            Vector3 initial_pos = initial_Transform.position;
            initial_pos.z = whereiam() + (length / 2.0f);
            initial_Transform.position = initial_pos;
        }
        else if (cd["type"] + "" == "tap")
        {
            Transform initial_Transform = this.transform;
            Vector3 initial_pos = initial_Transform.position;
            initial_pos.z = whereiam();
            initial_Transform.position = initial_pos;
        }
        ////Debug.Log("Arrive frame is " + arrframe);
        ////Debug.Log("Speed :" + speed);

        Transform myTransform = this.transform;
        Vector3 pos = myTransform.position;
        pos.z = -40000;
        myTransform.position = pos;
        yield return null;
    }

    void Start()
    {
        StartCoroutine(Initialize());
    }

    // Update is called once per frame
    private async Task Update()
    {
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
                this.transform.localScale = new Vector3(0.7f, 0.01f, length);
            }
            if (type == "tap")
            {
                //タップノーツ
                Transform myTransform = this.transform;
                Vector3 pos = myTransform.position;
                float timelag = game_time - arrsec;
                if (timelag < 0)
                {
                    timelag = timelag * -1.0f;
                }
                pos.z = whereiam();
                myTransform.position = pos;

                // TODO: Ƃ肠¦¸^bvm[cp̔»èðìéB
                if (isPreviousObjectDestroyed)
                {
                    if (!adata.auto_play)
                    {
                        if (Input.GetKeyDown("k") || Input.GetKeyDown(KeyCode.Joystick1Button2))
                        {
                            if (timelag <= adata.perfect)
                            {
                                // p[tFNg
                                ////Debug.Log("Prefect");
                                Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo++;
                                ScoreManeger.score = ScoreManeger.score + 5;
                                ResultUI.Perfect++;
                                adata.clicked_L4++;
                                destroy_gameobject();
                            }
                            else if (timelag <= adata.good)
                            {
                                // Obh
                                ////Debug.Log("Good");
                                Instantiate(adata.g, new UnityEngine.Vector3(10.74f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo++;
                                ScoreManeger.score = ScoreManeger.score + 3;
                                ResultUI.Good++;
                                adata.clicked_L4++;
                                destroy_gameobject();
                            }
                            else if (timelag <= adata.bad)
                            {
                                // obh
                                ////Debug.Log("Bad");
                                Instantiate(adata.b, new UnityEngine.Vector3(10.77f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo = 0;
                                ResultUI.Bad++;
                                adata.clicked_L4++;
                                destroy_gameobject();
                            }
                        }
                        else if (Input.GetMouseButtonDown(0))
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;

                            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == "CL4")
                            {
                                if (timelag <= adata.perfect)
                                {
                                    Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 5;
                                    ResultUI.Perfect++;
                                    adata.clicked_L4++;
                                    destroy_gameobject();
                                }
                                else if (timelag <= adata.good)
                                {
                                    Instantiate(adata.g, new UnityEngine.Vector3(10.74f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 3;
                                    ResultUI.Good++;
                                    adata.clicked_L4++;
                                    destroy_gameobject();
                                }
                                else if (timelag <= adata.bad)
                                {
                                    Instantiate(adata.b, new UnityEngine.Vector3(10.77f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    ScoreManeger.combo = 0;
                                    ResultUI.Bad++;
                                    adata.clicked_L4++;
                                    destroy_gameobject();
                                }
                            }
                        }
                        if (game_time > arrsec + adata.miss)
                        {
                            // ~X
                            ////Debug.Log("Miss");
                            Instantiate(adata.m, new UnityEngine.Vector3(10.78f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                            ScoreManeger.combo = 0;
                            ResultUI.Miss++;
                            adata.clicked_L4++;
                            destroy_gameobject();
                        }
                    }
                    else
                    {
                        if (arrsec - game_time <= adata.auto)
                        {
                            Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                            ScoreManeger.combo++;
                            ScoreManeger.score += 5;
                            ResultUI.Perfect++;
                            adata.clicked_L4++;
                            destroy_gameobject();
                        }
                    }
                }
            }
            if (isLongNoteActive)
            {
                // Om[c
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
                Vector3 pos = myTransform.position;
                pos.z = whereiam() + (length / 2.0f);
                myTransform.position = pos;
                sPoint = pos.z - (length / 2.0f);
                ePoint = pos.z + (length / 2.0f);

                // Om[c̔»è
                if (isPreviousObjectDestroyed)
                {
                    if (long_click && (game_time >= arrsec))
                    {
                        if (!adata.auto_play)
                        {
                            if (Input.GetKey("k") || Input.GetKey(KeyCode.Joystick1Button2))
                            {
                                long_click = true;
                                if (game_time >= endsec)
                                {
                                    Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 5;
                                    ResultUI.Perfect++;
                                    adata.clicked_L4++;
                                    destroy_gameobject();
                                }
                            }
                            else if (Input.GetMouseButton(0))
                            {
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hit;

                                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == "CL4")
                                {
                                    long_click = true;
                                    if (game_time >= endsec)
                                    {
                                        Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                        ScoreManeger.combo++;
                                        ScoreManeger.score = ScoreManeger.score + 5;
                                        ResultUI.Perfect++;
                                        adata.clicked_L4++;
                                        destroy_gameobject();
                                    }
                                }
                            }
                            else
                            {
                                long_click = false;
                                Instantiate(adata.m, new UnityEngine.Vector3(10.78f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo = 0;
                                ResultUI.Miss++;
                                adata.clicked_L4++;
                                destroy_gameobject();
                            }
                        }
                        else
                        {
                            long_click = true;
                            if (game_time >= endsec)
                            {
                                Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                ScoreManeger.combo++;
                                ScoreManeger.score += 5;
                                ResultUI.Perfect++;
                                adata.clicked_L4++;
                                destroy_gameobject();
                            }
                        }
                    }

                    if (!adata.auto_play)
                    {
                        if ((Input.GetKeyDown("k") || Input.GetKeyDown(KeyCode.Joystick1Button2)) && !long_click)
                        {
                            if (timelag <= adata.perfect)
                            {
                                // p[tFNg
                                ////Debug.Log("Prefect");
                                Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                long_click = true;
                                long_clicked = true;
                                ScoreManeger.combo++;
                                ScoreManeger.score = ScoreManeger.score + 5;
                                ResultUI.Perfect++;
                                adata.clicked_L4++;
                            }
                            else if (timelag <= adata.good)
                            {
                                // Obh
                                ////Debug.Log("Good");
                                Instantiate(adata.g, new UnityEngine.Vector3(10.74f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                long_click = true;
                                long_clicked = true;
                                ScoreManeger.combo++;
                                ScoreManeger.score = ScoreManeger.score + 3;
                                ResultUI.Good++;
                                adata.clicked_L4++;
                            }
                            else if (timelag <= adata.bad)
                            {
                                // obh
                                ////Debug.Log("Bad");
                                Instantiate(adata.b, new UnityEngine.Vector3(10.77f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                isLongNoteActive = false;
                                ScoreManeger.combo = 0;
                                ResultUI.Bad++;
                                ResultUI.Miss++;
                                adata.clicked_L4 += 2;
                                destroy_gameobject();
                            }
                        }
                        else if (Input.GetMouseButtonDown(0) && !long_click)
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;

                            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == "CL4")
                            {
                                if (timelag <= adata.perfect)
                                {
                                    Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    long_click = true;
                                    long_clicked = true;
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 5;
                                    ResultUI.Perfect++;
                                    adata.clicked_L4++;
                                }
                                else if (timelag <= adata.good)
                                {
                                    Instantiate(adata.g, new UnityEngine.Vector3(10.74f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    long_click = true;
                                    long_clicked = true;
                                    ScoreManeger.combo++;
                                    ScoreManeger.score = ScoreManeger.score + 3;
                                    ResultUI.Good++;
                                    adata.clicked_L4++;
                                }
                                else if (timelag <= adata.bad)
                                {
                                    Instantiate(adata.b, new UnityEngine.Vector3(10.77f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                                    isLongNoteActive = false;
                                    ScoreManeger.combo = 0;
                                    ResultUI.Bad++;
                                    ResultUI.Miss++;
                                    adata.clicked_L4 += 2;
                                    destroy_gameobject();
                                }
                            }
                        }
                        // Om[c̏I¹»è
                        if (game_time > arrsec + adata.miss && !long_click)
                        {
                            // Om[c̏I_܂ł̎Ԃð߂¬Ăµ܂B½ꍇ̓~XƂ·é
                            ////Debug.Log("Miss");
                            Instantiate(adata.m, new UnityEngine.Vector3(10.78f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                            isLongNoteActive = false;
                            ScoreManeger.combo = 0;
                            ResultUI.Miss += 2;
                            adata.clicked_L4 += 2;
                            destroy_gameobject();
                        }
                    }
                    else
                    {
                        if (arrsec - game_time <= adata.auto && !long_click)
                        {
                            // p[tFNg
                            ////Debug.Log("Prefect");
                            Instantiate(adata.p, new UnityEngine.Vector3(10.64f, 0.2f, -11.9f), UnityEngine.Quaternion.Euler(68.85f, 0.0f, 0.0f));
                            long_click = true;
                            long_clicked = true;
                            ScoreManeger.combo++;
                            ScoreManeger.score += 5;
                            ResultUI.Perfect++;
                            adata.clicked_L4++;
                        }
                    }
                }
            }
        }
        else
        {
            tmptime += Time.deltaTime;
            Transform myTransform = this.transform;
            Vector3 pos = myTransform.position;
            float timelag = game_time - arrsec;
            pos.z = whereiam();
            myTransform.position = pos;
        }

        int nf = Time.frameCount;
        float nt = Time.time;

        if (id != 0)
        {
            GameObject obj = GameObject.Find("L4_" + (id - 1));
            if (obj != null)
            {

            }
            else
            {
                obj = null;
            }
            GameObject long_obj = GameObject.Find("L4-long_" + (id - 1));
            if (long_obj != null)
            {

            }
            else
            {
                long_obj = null;
            }

            if (obj != null || long_obj != null)
            {
                // 前のidのオブジェクトがまだ存在している場合
                isPreviousObjectDestroyed = false;
            }
            else
            {
                // 前のidのオブジェクトが消えた場合
                isPreviousObjectDestroyed = true;
            }
        }
        else
        {
            // idが0の場合は常に前のオブジェクトが消えたことにする
            isPreviousObjectDestroyed = true;
        }
    }
}