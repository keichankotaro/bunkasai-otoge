using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CL3 : MonoBehaviour
{
    [SerializeField] GameObject sphere;
    private string pk;

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("j") || Input.GetKey(KeyCode.Joystick1Button1))
        {
            GetComponent<Renderer>().material.color = new Color32(101, 101, 101, 61);
        }
        else
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject && hit.collider.gameObject.name == "CL3")
                {
                    GetComponent<Renderer>().material.color = new Color32(101, 101, 101, 61);
                }
                else
                {
                    GetComponent<Renderer>().material.color = new Color32(101, 101, 101, 0);
                }
            }
            else
            {
                GetComponent<Renderer>().material.color = new Color32(101, 101, 101, 0);
            }
        }
    }
}
