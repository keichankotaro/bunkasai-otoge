using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetNoteDebug : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject dbg;
    void Start()
    {
        dbg = transform.Find("Debug").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (DebugText.isDebugMode)
        {
            dbg.GetComponent<TextMeshPro>().text = "Note:" + name.Split("_")[1];
        }
        else
        {
            dbg.GetComponent<TextMeshPro>().text = "";
        }
    }
}
