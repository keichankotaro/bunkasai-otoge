using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CL1 : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] GameObject sphere;
    private string pk;
    public Material material;

    public void OnPointerDown(PointerEventData eventData)
    {
        //this.GetComponent<Renderer>().material.color = new Color32(101, 101, 101, 61);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //this.GetComponent<Renderer>().material.color = new Color32(101, 101, 101, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.Input.GetKey("d") || UnityEngine.Input.GetKey(KeyCode.Joystick1Button0))
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

                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject && hit.collider.gameObject.name == "CL1")
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

        GameObject go = GameObject.Find("CL1");
        RectTransform rc = go.GetComponent<RectTransform>();
        //Debug.Log(go);
        //Debug.Log(rc);
        /*
        Touch[] touches = Input.touches;//全タップ情報の取得
        for (int i = 0; i < touches.Length; i++)
        {
            Touch t = touches[i];
            //Panelが属するCanvasのRenderModeがOverlayなら第3引数はnull
            //タップ位置をPanel内の座標で取得
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rc, t.position, null, out Vector2 localPoint);
            if (rc.rect.xMin < localPoint.x && localPoint.x < rc.rect.xMax
                    && rc.rect.yMin < localPoint.y && localPoint.y < rc.rect.yMax)
            {
                //タップがpanelの内部だったら
                Debug.Log(localPoint);
            }
        }
        */
    }
}
