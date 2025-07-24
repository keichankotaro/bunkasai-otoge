using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextScrollerForDetails : MonoBehaviour
{
    public TMP_Text textMeshPro;
    public float scrollSpeed = 100f;
    public float padding = 25f; // テキストの両端に追加する余白

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private float textWidth;
    private float containerWidth;
    private string parentName;
    private bool setup;

    void Start()
    {
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TMP_Text>();
        }

        rectTransform = GetComponentInParent<RectTransform>();
        parentName = transform.parent.name;
        startPosition = rectTransform.anchoredPosition;

        IEnumerator routine;
        routine = null;
        routine = ScrollText();
        StartCoroutine(routine);
    }

    private void Update()
    {
        if (!setup)
        {
            setup = true;
            Start();
        }
        /*
        if (adata.ready_to_start)
        {
            setup = false;
        }
        else
        {
            if (!setup)
            {
                setup = true;
                Start();
            }
        }
        */
    }

    IEnumerator ScrollText()
    {
        yield return new WaitForEndOfFrame();

        while (true)
        {
            scrollSpeed = 100f;
            textWidth = textMeshPro.preferredWidth + padding * 2;
            containerWidth = rectTransform.rect.width;

            // TextMeshProのRectTransformを更新して、はみ出した部分も表示されるようにする
            rectTransform.sizeDelta = new Vector2(textWidth, rectTransform.sizeDelta.y);
            var corners = new Vector3[4];
            transform.parent.GetComponent<RectTransform>().GetLocalCorners(corners);
            containerWidth = corners[3].x - corners[2].x;

            float newX = rectTransform.anchoredPosition.x - scrollSpeed * Time.deltaTime;

            if (newX <= -textWidth)
            {
                //newX = textWidth; // テキストが見えなくなったら位置をリセット
                newX = corners[3].x + textWidth / 2;
            }

            rectTransform.anchoredPosition = new Vector2(newX, startPosition.y);
            yield return null;
        }
    }
}