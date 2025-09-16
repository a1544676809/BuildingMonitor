using Assets.Codes.Entities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorListPanelController : MonoBehaviour
{
    public Transform sensorListContentPanel; // 关联 Scroll View 的 Content
    public Button closeButton;
    public GameObject sensorItemPrefab;
    public GameObject noticePanel;

    // 字典来存储每个传感器的 UI 文本引用，方便后续更新
    private Dictionary<int, TextMeshProUGUI> sensorValueTexts = new Dictionary<int, TextMeshProUGUI>();
    void OnCloseButtonClicked()
    {
               gameObject.SetActive(false);
    }


    /// <summary>
    /// 使用给定的传感器数据来填充列表。
    /// </summary>
    public void PopulateSensorList(List<SensorsInfo> sensors)
    {
        // 1. 清除旧的 UI 列表（如果存在）
        foreach (Transform child in sensorListContentPanel)
        {
            Destroy(child.gameObject);
        }
        sensorValueTexts.Clear();

        // 2. 遍历传感器信息，为每个传感器实例化一个 UI 条目
        foreach (var sensor in sensors)
        {
            GameObject sensorItem = Instantiate(sensorItemPrefab, sensorListContentPanel);

            // 获取并设置文本组件
            // 注意：你需要根据你的 Prefab 内部结构来调整 Find 的路径
            TextMeshProUGUI idText = sensorItem.transform.Find("SensorIdText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI typeText = sensorItem.transform.Find("SensorTypeText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI statusText = sensorItem.transform.Find("SensorStatusText").GetComponent<TextMeshProUGUI>();

            idText.text = sensor.sensorId.ToString();
            typeText.text = sensor.sensorType;
            statusText.text = "状态: 未知"; // 初始状态

            // 将文本引用存入字典，以便后续更新状态
            sensorValueTexts.Add(sensor.sensorId, statusText);
        }
    }

    /// <summary>
    /// 根据传感器ID更新其状态文本。
    /// </summary>
    public void UpdateSensorStatus(int sensorId, string status)
    {
        if (sensorValueTexts.ContainsKey(sensorId))
        {
            sensorValueTexts[sensorId].text = "状态: " + status;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
