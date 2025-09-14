using Assets.Codes.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI.Dates;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using XCharts;
using XCharts.Runtime;

public class DataVisualizationPanel : MonoBehaviour
{
    //UI����ϵĸ������
    //�ı�
    public TextMeshProUGUI dateTimeRangeText;
    //��ť
    public Button closeButton;
    public Button qureyButton;
    public Button refreshButton;
    //�����˵�
    public TMP_Dropdown sensorTypeDropdown;
    public TMP_Dropdown sensorDropdown;
    //����ѡ����
    public DatePicker_DateRange datePicker;

    //XChartͼ��
    public LineChart lineChart;

    //API����URL
    private string apiBaseUrl = "http://localhost:8080/api/sensors";
    
    //�������б�
    private List<string> sensorTypes = new List<string> { "λ�ƴ�����", "Ӧ��������" };
    private Dictionary<string, List<string>> availableSensorIds = new Dictionary<string, List<string>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void InitializeDropdowns()
    {
        // ��䴫��������������
        sensorTypeDropdown.ClearOptions();
        sensorTypeDropdown.AddOptions(sensorTypes);

        // �Ӻ�˻�ȡ���п��õĴ�����ID������䵽�ֵ���
        StartCoroutine(FetchAllSensorIds());
    }

    IEnumerator FetchAllSensorIds()
    {
        string allSensorsUrl = apiBaseUrl;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(allSensorsUrl))
        {
            yield return webRequest.SendWebRequest();
            if(webRequest.result==UnityWebRequest.Result.Success)
            {
                // �����˷���һ���������д�������Ϣ��JSON
                string jsonResponse = webRequest.downloadHandler.text;
                // ����Ҫһ����Ӧ��˷��ص�SensorInfo��
                var sensorInfoList = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);
                // ���ݴ��������ͷ���ID
                availableSensorIds.Clear();

                foreach (var info in sensorInfoList)
                {
                    if (!availableSensorIds.ContainsKey(info.sensorType)) // info.type ��Ӧ "λ�ƴ�����" �� "Ӧ��������"
                    {
                        availableSensorIds[info.sensorType] = new List<string>();
                    }
                    availableSensorIds[info.sensorType].Add(info.sensorId.ToString());
                }
                // ��ʼ��������ID������
                OnSensorTypeChanged(0);
            }
            else
            {
                Debug.LogError("��ȡ���д�����IDʧ��: " + webRequest.error);
            }
        }
    }

    // ����������������ֵ�ı�ʱ����
    void OnSensorTypeChanged(int index)
    {
        string selectedType = sensorTypes[index];
        sensorDropdown.ClearOptions();
        if (availableSensorIds.ContainsKey(selectedType))
        {
            sensorDropdown.AddOptions(availableSensorIds[selectedType]);
        }

        // �����͸ı�ʱ���Զ�����һ�β�ѯ����ʾĬ������
        OnQueryButtonClicked();
    }

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void OnRefreshButtonClicked()
    {
        InitializeDropdowns();
    }

    void OnQueryButtonClicked()
    {
        string selectedType = sensorTypes[sensorTypeDropdown.value];
        string selectedId = sensorDropdown.options[sensorDropdown.value].text;

        // ��ȡ����ѡ������ֵ
        string startDate = datePicker.FromDate.Date.ToString("yyyy-MM-dd");
        string endDate = datePicker.ToDate.Date.ToString("yyyy-MM-dd");

        // �Զ�Ϊ��ʼ�������00:00:00
        string startTime = startDate + "T00:00:00";
        // �Զ�Ϊ�����������23:59:59���԰����������ڵ���������
        string endTime = endDate + "T23:59:59";

        Debug.Log($"��ʼ����: {startTime}, ��������: {endTime}");

        // ���ݴ��������͵��ò�ͬ��Э��
        if (selectedType == "λ�ƴ�����")
        {
            StartCoroutine(FetchDisplacementData(selectedId, startTime, endTime));
        }
        else if (selectedType == "Ӧ��������")
        {
            StartCoroutine(FetchStressData(selectedId, startTime, endTime));
        }
    }

    IEnumerator FetchDisplacementData(string sensorId, string startTime, string endTime)
    {
        string url = $"{apiBaseUrl}/displacement/history/{sensorId}?startTime={startTime}&endTime={endTime}";
        // ... ����UnityWebRequest��������Ӧ������lineChart ...
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                List<DisplacementSensorData> historyData = JsonConvert.DeserializeObject<List<DisplacementSensorData>>(jsonResponse);
                if (historyData != null && historyData.Count > 1)
                {
                    lineChart.RemoveAllSerie();
                    var series = lineChart.AddSerie<Line>();
                    series.lineStyle.width = 2f;
                    series.symbol.show = true;

                    // �ҳ���������������
                    DateTime minDate = DateTime.MaxValue;
                    DateTime maxDate = DateTime.MinValue;

                    foreach (var dataPoint in historyData)
                    {
                        DateTime dataPointTime = dataPoint.timestamp.DateTime;
                        long unixTimestamp = dataPoint.timestamp.ToUnixTimeSeconds();

                        // �����������������
                        if (dataPointTime < minDate)
                        {
                            minDate = dataPointTime;
                        }
                        if (dataPointTime > maxDate)
                        {
                            maxDate = dataPointTime;
                        }
                        // ��Ӱ���ʱ���������ֵ�����ݵ�
                        series.AddXYData(unixTimestamp, dataPoint.totalDisplacement);
                    }
                    UpdateDateTimeRangeText(minDate, maxDate);
                    Debug.Log("λ�����ݵ�����: " + historyData.Count);
                }
                else
                {
                    lineChart.RemoveAllSerie();
                    UpdateDateTimeRangeText(DateTime.Now, DateTime.Now);
                    Debug.Log("û���㹻��λ�ƴ�������ʷ����������ͼ��");
                }
            }
            else
            {
                Debug.LogError("��ȡλ������ʧ��: " + webRequest.error);
            }
        }
    }

    IEnumerator FetchStressData(string sensorId, string startTime, string endTime)
    {
        // �������в�ͬ��API����ȡӦ������
        string url = $"{apiBaseUrl}/stress/history/{sensorId}?startTime={startTime}&endTime={endTime}";
        // ... ��д���Ƶ�UnityWebRequest�߼�����ȡ������Ӧ������ ...
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            // ... (�����߼���FetchDisplacementData����)
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                List<DisplacementSensorData> historyData = JsonConvert.DeserializeObject<List<DisplacementSensorData>>(jsonResponse);
                if (historyData != null && historyData.Count > 1)
                {

                    lineChart.RemoveAllSerie();
                    var series = lineChart.AddSerie<Line>();
                    series.lineStyle.width = 2f;
                    series.symbol.show = true;

                    // �ҳ���������������
                    DateTime minDate = DateTime.MaxValue;
                    DateTime maxDate = DateTime.MinValue;

                    foreach (var dataPoint in historyData)
                    {
                        DateTime dataPointTime = dataPoint.timestamp.DateTime;
                        long unixTimestamp = dataPoint.timestamp.ToUnixTimeSeconds();

                        // �����������������
                        if (dataPointTime < minDate)
                        {
                            minDate = dataPointTime;
                        }
                        if (dataPointTime > maxDate)
                        {
                            maxDate = dataPointTime;
                        }
                        // ��Ӱ���ʱ���������ֵ�����ݵ�
                        series.AddXYData(unixTimestamp, dataPoint.totalDisplacement);
                    }
                    UpdateDateTimeRangeText(minDate, maxDate);
                }
                else
                {
                    lineChart.RemoveAllSerie();
                    UpdateDateTimeRangeText(DateTime.Now, DateTime.Now);
                    Debug.Log("û���㹻��Ӧ����������ʷ����������ͼ��");
                }
            }
            else
            {
                Debug.LogError("��ȡӦ������ʧ��: " + webRequest.error);
            }

        }
    }

    void UpdateDateTimeRangeText(DateTime minDate, DateTime maxDate)
    {
        if (dateTimeRangeText != null)
        {
            string formattedMinDate = minDate.ToString("yyyy-MM-dd HH:mm:ss");
            string formattedMaxDate = maxDate.ToString("yyyy-MM-dd HH:mm:ss");
            dateTimeRangeText.text = $"ʵ�����ݷ�Χ��{formattedMinDate} - {formattedMaxDate}";
        }
    }

    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        qureyButton.onClick.AddListener(OnQueryButtonClicked);
        refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        datePicker.FromDate = System.DateTime.Now.AddDays(-30);
        datePicker.ToDate = System.DateTime.Now;
        FetchAllSensorIds();
        InitializeDropdowns();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
