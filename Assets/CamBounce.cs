using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class CamBounce : MonoBehaviour
{   
    private List<float> bounceTimes = new List<float>();
    private int currentBounceIndex = 0;
    private float currentBounceTime = 0f; // Ç±Ç±Ç0Ç…èâä˙âª
    private float lastGameTime = 0.0f;
    private bool isBouncing = false;
    private Vector3 initialPosition;
    private int count = 0;

    private void Start()
    {
        string chart = adata.chart;
        string textAsset = Resources.Load<TextAsset>("Charts/" + chart).ToString();
        if (textAsset != null)
        {
            JObject jsonObj = JObject.Parse(textAsset);
            JToken bounceData = jsonObj["chartdata"]["camdata"]["bounce"];
            int times = (int)bounceData["times"];
            //bounceTimes = new float[times];
            for (int i = 0; i < times; i++)
            {
                float time = (float)bounceData[(i + 1).ToString()]["time"];
                bounceTimes.Add(time);
            }
            //Debug.Log(bounceTimes + "");
            //Debug.Log(bounceData + "");
        }
    }

    private void Update()
    {
        float game_time = adata.game_time;
        if (count < bounceTimes.Count)
        {
            if (game_time >= bounceTimes[count])
            {
                ////Debug.LogError("Bounce.");
                count++;
            }
        }
    }

    private void CheckBounceTime(float game_time)
    {
        if (currentBounceIndex < bounceTimes.Count && (bounceTimes[currentBounceIndex] == game_time))
        {
            isBouncing = true;
            // Perform the camera bounce here
            currentBounceTime = 0f; // currentBounceTimeÇ0Ç…èâä˙âª
            this.transform.position = new Vector3(initialPosition.x, initialPosition.y + Mathf.Sin(currentBounceTime * 10f) * 0.2f, initialPosition.z);
            //Debug.Log("Camera bouncing at time: " + game_time);
            //currentBounceIndex++;
            if (currentBounceTime >= 1.0f) // Adjust this time as per your requirement
            {
                isBouncing = false;
                currentBounceTime = 0f;
                currentBounceIndex++;
            }
        }
        else
        {
            isBouncing = false;
        }
    }
}
