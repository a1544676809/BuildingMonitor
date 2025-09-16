using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NoticePanel : MonoBehaviour
{
    public Button confirmButton;
    private RectTransform rectTransform;
    private Vector2 defaultAnchoredPosition;
    public TextMeshProUGUI noticeText;

    void OnConfirmButtonClicked()
    {
        gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void Awake()
    {
        // 获取 RectTransform 组件
        rectTransform = GetComponent<RectTransform>();

        // 第一次启动时，保存其初始位置
        defaultAnchoredPosition = rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        ResetToDefaultPosition();
    }

    /// <summary>
    /// 将面板位置重置为初始位置
    /// </summary>
    public void ResetToDefaultPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = defaultAnchoredPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
