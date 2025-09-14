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
        // 获取拖动区域的 RectTransform
        dragAreaRectTransform = GetComponent<RectTransform>();

        // 获取父级窗口的 RectTransform，这将是我们要移动的对象
        parentWindowRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 当鼠标按下拖动区域时调用
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // 计算鼠标点击位置与窗口左下角位置的偏移量
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentWindowRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pointerOffset
        );
    }

    /// <summary>
    /// 当鼠标拖动时调用
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // 获取鼠标在 Canvas 上的当前位置
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentWindowRectTransform.parent.GetComponent<RectTransform>(), // 通常是 Canvas
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        ))
        {
            // 更新窗口的位置：新的位置 = 鼠标位置 - 初始偏移量
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
