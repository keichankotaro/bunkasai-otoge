using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public string mode = "forward";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(this.name);
        if (mode == "forward")
        {
            Transform test = this.transform;

            Vector3 pos = test.position;
            pos.x += 0.00f;
            pos.y += 0.00f;
            pos.z -= 0.1f;

            test.position = pos;
            if (pos.z <= -9.61)
            {
                mode = "backward";
            }
        } else if (mode == "backward")
        {
            Transform test = this.transform;

            Vector3 pos = test.position;
            pos.x += 0.00f;
            pos.y += 0.00f;
            pos.z += 0.1f;

            test.position = pos;
            if (pos.z >= -7.61)
            {
                mode = "forward";
            }
        }
    }
}