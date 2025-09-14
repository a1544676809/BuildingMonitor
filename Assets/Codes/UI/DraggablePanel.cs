using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class DraggablePanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform dragAreaRectTransform;
    private RectTransform parentWindowRectTransform;
    private Vector2 pointerOffset;

    void Awake()
    {
        // ��ȡ�϶������ RectTransform
        dragAreaRectTransform = GetComponent<RectTransform>();

        // ��ȡ�������ڵ� RectTransform���⽫������Ҫ�ƶ��Ķ���
        parentWindowRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    /// <summary>
    /// ����갴���϶�����ʱ����
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // ���������λ���봰�����½�λ�õ�ƫ����
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentWindowRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pointerOffset
        );
    }

    /// <summary>
    /// ������϶�ʱ����
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // ��ȡ����� Canvas �ϵĵ�ǰλ��
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentWindowRectTransform.parent.GetComponent<RectTransform>(), // ͨ���� Canvas
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        ))
        {
            // ���´��ڵ�λ�ã��µ�λ�� = ���λ�� - ��ʼƫ����
            parentWindowRectTransform.localPosition = localPointerPosition - pointerOffset;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
