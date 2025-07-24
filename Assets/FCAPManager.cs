using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FCAPManager : MonoBehaviour
{
    [SerializeField]
    public GameObject canvas;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!adata.fcap_ed && !adata.showResult) {
            if (ScoreManeger.ratioscore >= 1000000)
            {
                // All Perfect
                Debug.Log("AP");
                Instantiate((GameObject)Resources.Load("Animations/AllPerfect")).transform.SetParent(canvas.transform);
                adata.fcap_ed = true;
            }
            else if (adata.progress >= 100.0f)
            {
                if (ScoreManeger.combo == ScoreManeger.notes)
                {
                    // Full Combo
                    Debug.Log("FC");
                    Instantiate((GameObject)Resources.Load("Animations/FullCombo")).transform.SetParent(canvas.transform);
                    adata.fcap_ed = true;
                }
                else if (ScoreManeger.ratioscore >= 750000)
                {
                    // Clear
                    Debug.Log("C");
                    Instantiate((GameObject)Resources.Load("Animations/Clear")).transform.SetParent(canvas.transform);
                    adata.fcap_ed = true;
                }
                else
                {
                    // Failed
                    //Debug.Log("F");
                    //Instantiate((GameObject)Resources.Load("Animations/Failed")).transform.SetParent(canvas.transform);
                    adata.fcap_ed = true;
                }
                adata.fcap_ed = true;
            }
        }
    }
}
