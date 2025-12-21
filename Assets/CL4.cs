using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CL4 : MonoBehaviour
{
    [SerializeField] GameObject sphere;
    private string pk;
    private List<bool> hits = new List<bool>();

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("k") || Input.GetKey(adata.ControlerLine4))
        {
            GetComponent<Renderer>().material.color = new Color32(101, 101, 101, 61);
        }
        else
        {
            if (Input.touchCount > 0)
            {
                hits = new List<bool>();
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hit;

                    hits.Add((Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject && hit.collider.gameObject.name == "CL4"));

                }
                if (hits.Any(value => value))
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
