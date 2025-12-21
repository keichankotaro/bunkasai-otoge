using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class gettime : MonoBehaviour
{
    [SerializeField] GameObject ayr;
    public float time;
    //public GameObject text;
    private float start_remaing;
    // Start is called before the first frame update
    private bool did;
    public GameObject timeText;

    void Start()
    {
        time = 0.0f;
        //text = GameObject.Find("time");
    }

    // Update is called once per frame
    void Update()
    {
        if (adata.ready_to_start)
        {
            start_remaing += Time.deltaTime;
            time += Time.deltaTime;
            //Debug.Log(adata.game_time);
            if (start_remaing >= 0)
            {
                if (!did)
                {
                    adata.game_time = -2.0f;
                    did = true;
                }
                ayr.GetComponent<TextMeshProUGUI>().text = "Are You Ready?";
            }
            if (start_remaing >= 2)
            {
                ayr.GetComponent<TextMeshProUGUI>().text = "3";
            }
            if (start_remaing >= 3)
            {
                ayr.GetComponent<TextMeshProUGUI>().text = "2";
            }
            if (start_remaing >= 4)
            {
                ayr.GetComponent<TextMeshProUGUI>().text = "1";
            }
            if (start_remaing >= 5)
            {
                ayr.GetComponent<TextMeshProUGUI>().text = "";
            }
            //timeText.GetComponent<TextMeshProUGUI>().text = adata.game_time + "";
            if (start_remaing >= 6 && adata.game_time <= 0)
            {
                //Debug.Log(adata.game_time);
                adata.game_time += Time.deltaTime;
            }
        } 
        else
        {
            did = false;
            adata.game_time = 0.0f;
            start_remaing = 0.0f;
            time = 0.0f;
            adata.l1notes = 0;
            adata.l2notes = 0;
            adata.l3notes = 0;
            adata.l4notes = 0;
            adata.clicked_L1 = 0;
            adata.clicked_L2 = 0;
            adata.clicked_L3 = 0;
            adata.clicked_L4 = 0;
        }
    }
}
