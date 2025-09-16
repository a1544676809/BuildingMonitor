using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button sensorStatusButton;
    public Button sensorListButton;
    public Button closeButton;

    public GameObject sensorListPanel;
    public GameObject noticePanel;
    public TextMeshProUGUI noticeText;

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void OnSensorStatusButtonClicked()
    {
        noticePanel.SetActive(true);
        noticeText.text = "请点击传感器查看传感器信息";
    }

    void OnSensorListButtonClicked()
    {
        sensorListPanel.SetActive(true);
    }

    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        sensorListButton.onClick.AddListener(OnSensorListButtonClicked);
        sensorStatusButton.onClick.AddListener(OnSensorStatusButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
