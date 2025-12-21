using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class goodbye : MonoBehaviour
{
    private float time;
    private string instantiatorName; // Instantiate元のGameObject名を保持する変数
    private int lineNo;

    // Instantiate元のGameObject名を設定するメソッド
    public void SetInstantiatorName(string name, int line)
    {
        instantiatorName = name;
        lineNo = line;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (DebugText.isDebugMode)
        {
            name = "Hantei_" + lineNo + "_" + instantiatorName;
        }

        if (time >= adata.del_hantei)
        {
            if (DebugText.isDebugMode)
            {
                this.gameObject.GetComponent<TextMeshPro>().text = "Note: " + instantiatorName;

                // アニメーションを停止または削除
                var animator = this.gameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    Destroy(animator); // アニメーションコンポーネントを削除
                }

                // 移動処理
                Vector3 newPosition = this.gameObject.transform.position;
                newPosition.z = -11.5f;
                this.gameObject.transform.position = newPosition;

                // 同一のlineNoでinstantiatorName未満のものを削除
                foreach (var obj in FindObjectsOfType<goodbye>())
                {
                    if (obj.lineNo == this.lineNo && string.Compare(obj.instantiatorName, this.instantiatorName) < 0)
                    {
                        Destroy(obj.gameObject);
                    }
                }
                if (time >= adata.del_hantei + 2)
                    Destroy(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
}
