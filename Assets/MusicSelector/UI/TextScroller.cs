/*
using System.Collections;
using TMPro;
using UnityEngine;

public class TextScroller : MonoBehaviour
{
    public TMP_Text textMeshPro;
    public float scrollSpeed = 75f;
    public float padding = 50f;

    private RectTransform textRectTransform;
    private RectTransform parentRectTransform;
    private float textWidth;
    private float parentWidth;

    private Coroutine scrollCoroutine;

    void Start()
    {
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TMP_Text>();
        }

        textRectTransform = textMeshPro.GetComponent<RectTransform>();
        parentRectTransform = textMeshPro.transform.parent.GetComponent<RectTransform>();

        if (parentRectTransform == null)
        {
            Debug.LogError("Parent RectTransform is missing.");
            return;
        }

        // �I�u�W�F�N�g���ɉ����ď����z�u�ݒ�
        string objName = gameObject.name;
        if (objName == "MusicTitle")
        {
            parentRectTransform.anchoredPosition3D = new Vector3(0f, -70f, 0f);
            parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 334f);
            parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f);
        }
        else if (objName == "Composer")
        {
            parentRectTransform.anchoredPosition3D = new Vector3(18f, -115.9f, 0f);
            //parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 331.05f);
            //parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f);
            textRectTransform.anchoredPosition3D = new Vector3(18f, 0f, 0f);
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 320f);
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f);
        }

        Debug.Log("Start schrolling.");
        StartCoroutine(InitializeAndMaybeScrollText());
    }

    IEnumerator InitializeAndMaybeScrollText()
    {
        yield return new WaitForEndOfFrame();

        textMeshPro.ForceMeshUpdate();
        textWidth = textMeshPro.preferredWidth + padding * 2;
        parentWidth = parentRectTransform.rect.width;

        // RectTransform�̃T�C�Y���X�V
        textRectTransform.sizeDelta = new Vector2(textWidth, textRectTransform.sizeDelta.y);

        // �e�L�X�g���e���������Ƃ������X�N���[��
        if (textWidth > parentWidth)
        {
            Vector2 startPosition = new Vector2(parentWidth + padding, textRectTransform.anchoredPosition.y);
            textRectTransform.anchoredPosition = startPosition;
            scrollCoroutine = StartCoroutine(ScrollText(startPosition));
        }
        else
        {
            // �X�N���[�����Ȃ� �� �����Œ肵�������낦�itext-align: center �����j
            parentWidth = 300f;
            parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth);
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300f);

            textMeshPro.alignment = TextAlignmentOptions.Center;
            textRectTransform.pivot = new Vector2(0.5f, textRectTransform.pivot.y);
            textRectTransform.anchorMin = new Vector2(0.5f, textRectTransform.anchorMin.y);
            textRectTransform.anchorMax = new Vector2(0.5f, textRectTransform.anchorMax.y);
            textRectTransform.anchoredPosition = new Vector2(0f, textRectTransform.anchoredPosition.y);
        }
    }

    IEnumerator ScrollText(Vector2 startPosition)
    {
        while (true)
        {
            float newX = textRectTransform.anchoredPosition.x - scrollSpeed * Time.deltaTime;

            if (newX <= -textWidth)
            {
                newX = parentWidth + padding;
            }

            textRectTransform.anchoredPosition = new Vector2(newX, startPosition.y);
            yield return null;
        }
    }
}
*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �e�L�X�g�̃X�N���[��
/// TMP_Text�ɃA�^�b�`���Ă�������
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class TextScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 100f;
    [SerializeField] private ContentSizeFitter ContentSizeFitter;
    [SerializeField] private bool forceScroll = true; // �����X�N���[���ifalse�̏ꍇ�͒������낦�\���j
    [SerializeField] private Vector3 musicTitlePosition = Vector3.zero; // MusicTitle �p�̈ʒu
    [SerializeField] private Vector3 composerPosition = Vector3.zero; // Composer �p�̈ʒu

    private RectTransform textRectTransform;
    private RectTransform parentRectTransform;
    private Vector3 startPosition;
    private float textWidth;
    private float parentWidth;
    private float scrollValue;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!forceScroll) return;

        scrollValue += scrollSpeed * Time.deltaTime;
        textRectTransform.anchoredPosition = new Vector3(startPosition.x - scrollValue, startPosition.y, startPosition.z);

        if (scrollValue > textWidth + parentWidth)
        {
            scrollValue = -parentWidth;
        }
    }

    private void Initialize()
    {
        if (!TryGetComponent<TMP_Text>(out var textComponent))
        {
            Debug.LogWarning("TMP_Text���A�^�b�`����Ă��Ȃ��̂ŃX�N���[���͋@�\���܂���");
            return;
        }

        if (ContentSizeFitter == null)
        {
            ContentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        ContentSizeFitter.SetLayoutHorizontal();
        ContentSizeFitter.SetLayoutVertical();
        LayoutRebuilder.ForceRebuildLayoutImmediate(ContentSizeFitter.GetComponent<RectTransform>());

        textRectTransform = textComponent.GetComponent<RectTransform>();
        parentRectTransform = textComponent.transform.parent.GetComponent<RectTransform>();

        textWidth = textRectTransform.rect.width;
        parentWidth = parentRectTransform.rect.width;
        startPosition = textRectTransform.anchoredPosition;

        /*
        if (textWidth <= parentWidth || !forceScroll)
        {
            // �X�N���[���s�v�A�������낦�\��
            Vector3 targetPos = Vector3.zero;
            switch (gameObject.name)
            {
                case "MusicTitle":
                    targetPos = musicTitlePosition;
                    break;
                case "Composer":
                    targetPos = composerPosition;
                    break;
                default:
                    targetPos = textRectTransform.anchoredPosition;
                    break;
            }
            textRectTransform.anchoredPosition = targetPos;
            textComponent.alignment = TextAlignmentOptions.Center;
            forceScroll = false;
        }
        else
        {
            scrollValue = -parentWidth;
        }
        */
        scrollValue = -parentWidth;
    }
}
