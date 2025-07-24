using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class goodbye : MonoBehaviour
{
    private float time;
    private string instantiatorName; // Instantiate����GameObject����ێ�����ϐ�
    private int lineNo;

    // Instantiate����GameObject����ݒ肷�郁�\�b�h
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

                // �A�j���[�V�������~�܂��͍폜
                var animator = this.gameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    Destroy(animator); // �A�j���[�V�����R���|�[�l���g���폜
                }

                // �ړ�����
                Vector3 newPosition = this.gameObject.transform.position;
                newPosition.z = -11.5f;
                this.gameObject.transform.position = newPosition;

                // �����lineNo��instantiatorName�����̂��̂��폜
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
