using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrackSkip : MonoBehaviour
{
    private float pressTime = 0.0f;
    public GameObject TrackSkipRemain;

    // Start is called before the first frame update  
    void Start()
    {
    }

    // Update is called once per frame  
    void Update()
    {
        if (adata.MusicStarted)
        {
            if (Input.GetKey(KeyCode.Escape) && pressTime >= 3.0f && !adata.Process)
            {
                Debug.Log("Skipping track...");
                adata.Process = true;

                string[] patterns = new string[]
                {
                    "L1_", "L2_", "L3_", "L4_",
                    "L1-long_", "L2-long_", "L3-long_", "L4-long_"
                };

                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    foreach (var pat in patterns)
                    {
                        if (obj.name.StartsWith(pat))
                        {
                            Destroy(obj);
                            break;
                        }
                    }
                }
                TrackSkipRemain.GetComponent<TextMeshProUGUI>().text = "";
                adata.ForceFinish = true;
            }
            else if (Input.GetKey(KeyCode.Escape) && pressTime < 3.0f && !adata.Process)
            {
                TrackSkipRemain.GetComponent<TextMeshProUGUI>().text = $"�g���b�N�X�L�b�v�܂ł��� {3.0f-pressTime:F1} �b";
                pressTime += Time.deltaTime;
            }
            else
            {
                TrackSkipRemain.GetComponent<TextMeshProUGUI>().text = "";
                pressTime = 0.0f;
            }
        }
    }
}
