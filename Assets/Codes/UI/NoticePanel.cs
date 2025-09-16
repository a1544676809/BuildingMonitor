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
        // ��ȡ RectTransform ���
        rectTransform = GetComponent<RectTransform>();

        // ��һ������ʱ���������ʼλ��
        defaultAnchoredPosition = rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        ResetToDefaultPosition();
    }

    /// <summary>
    /// �����λ������Ϊ��ʼλ��
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
