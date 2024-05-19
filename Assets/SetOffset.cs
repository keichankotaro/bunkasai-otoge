using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetOffset : MonoBehaviour
{
    private InputField inputField;
    
    // Start is called before the first frame update
    void Start()
    {
        inputField = GameObject.Find("AudioOffset").GetComponent<InputField>();
        inputField.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        if (float.Parse(inputField.text) < -8.0f)
        {
            inputField.text = "-8.0";
        }
        else if (float.Parse(inputField.text) > 8.0f)
        {
            inputField.text = "8.0";
        }
        adata.tlag = float.Parse(inputField.text);
    }
}
