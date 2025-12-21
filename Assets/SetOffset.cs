using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetOffset : MonoBehaviour
{
    private InputField inputField;
    private InputField SpeedInputField;
    private string tmp_offset;
    private string tmp_speed;
    
    // Start is called before the first frame update
    void Start()
    {
        inputField = GameObject.Find("AudioOffset").GetComponent<InputField>();
        inputField.text = "0";
        SpeedInputField = GameObject.Find("Speed").GetComponent<InputField>();
        SpeedInputField.text = "20";
    }

    // Update is called once per frame
    void Update()
    {
        if (tmp_offset != inputField.text)
        {
            ChangeOffset();
        }
        if (tmp_speed != SpeedInputField.text)
        {
            ChangeSpeed();
        }
        tmp_offset = inputField.text;
        tmp_speed = SpeedInputField.text;
    }

    public void ChangeOffset()
    {
        Debug.Log("Change Offset.");
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

    public void ChangeSpeed()
    {
        Debug.Log("Change Speed.");
        if (float.Parse(SpeedInputField.text) <= 0.0f)
        {
            SpeedInputField.text = "0.1";
        }
        else if (float.Parse(SpeedInputField.text) >= 100.0f)
        {
            SpeedInputField.text = "99.9";
        }
        adata.speed = float.Parse(SpeedInputField.text);
        adata.default_speed = float.Parse(SpeedInputField.text);
    }
}
