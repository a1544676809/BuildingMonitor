using Assets.Codes.Entities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorListPanelController : MonoBehaviour
{
    public Transform sensorListContentPanel; // ���� Scroll View �� Content
    public Button closeButton;
    public GameObject sensorItemPrefab;
    public GameObject noticePanel;

    // �ֵ����洢ÿ���������� UI �ı����ã������������
    private Dictionary<int, TextMeshProUGUI> sensorValueTexts = new Dictionary<int, TextMeshProUGUI>();
    void OnCloseButtonClicked()
    {
               gameObject.SetActive(false);
    }


    /// <summary>
    /// ʹ�ø����Ĵ���������������б�
    /// </summary>
    public void PopulateSensorList(List<SensorsInfo> sensors)
    {
        // 1. ����ɵ� UI �б�������ڣ�
        foreach (Transform child in sensorListContentPanel)
        {
            Destroy(child.gameObject);
        }
        sensorValueTexts.Clear();

        // 2. ������������Ϣ��Ϊÿ��������ʵ����һ�� UI ��Ŀ
        foreach (var sensor in sensors)
        {
            GameObject sensorItem = Instantiate(sensorItemPrefab, sensorListContentPanel);

            // ��ȡ�������ı����
            // ע�⣺����Ҫ������� Prefab �ڲ��ṹ������ Find ��·��
            TextMeshProUGUI idText = sensorItem.transform.Find("SensorIdText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI typeText = sensorItem.transform.Find("SensorTypeText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI statusText = sensorItem.transform.Find("SensorStatusText").GetComponent<TextMeshProUGUI>();

            idText.text = sensor.sensorId.ToString();
            typeText.text = sensor.sensorType;
            statusText.text = "״̬: δ֪"; // ��ʼ״̬

            // ���ı����ô����ֵ䣬�Ա��������״̬
            sensorValueTexts.Add(sensor.sensorId, statusText);
        }
    }

    /// <summary>
    /// ���ݴ�����ID������״̬�ı���
    /// </summary>
    public void UpdateSensorStatus(int sensorId, string status)
    {
        if (sensorValueTexts.ContainsKey(sensorId))
        {
            sensorValueTexts[sensorId].text = "״̬: " + status;
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
