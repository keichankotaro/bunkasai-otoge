using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ResultUI : MonoBehaviour
{
    public GameObject Result;
    public static int Perfect;
    public static int Good;
    public static int Bad;
    public static int Miss;
    public static int Score;
    private bool showed = false;
    private bool reseted = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!adata.ready_to_start && !showed && reseted)
        {
            Debug.Log("P:" + Perfect + ",G:" + Good + ",B:" + Bad + ",M:" + Miss);
            showed = true;
            reseted = false;
        }
        if (adata.ready_to_start && showed && !reseted)
        {
            Debug.Log("Reset");
            reseted = true;
            showed = false;
            Perfect = 0;
            Good = 0;
            Bad = 0;
            Miss = 0;
        }
    }
}
