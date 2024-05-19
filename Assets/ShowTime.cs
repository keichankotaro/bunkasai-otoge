using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTime : MonoBehaviour
{
    [SerializeField]
    public Slider ProgressBar;
    public static float prog;

    // Start is called before the first frame update
    void Start()
    {
        ProgressBar.maxValue = 100.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!adata.ready_to_start)
        {
            ProgressBar.value = 0;
            adata.progress = 0.0f;
            adata.fcap_ed = false;
        }

        if (adata.now / adata.length * 100.0f + "" == "NaN")
        {
            ProgressBar.value = 0;
        }
        else
        {
            if (PlayMusic.time >= 0.0f && PlayMusic.played)
            {
                adata.now += Time.deltaTime;
                ProgressBar.value = adata.now / adata.length * 100.0f;
                adata.progress = adata.now / adata.length * 100.0f;
            }
        }
    }
}