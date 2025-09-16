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
using static UnityEditor.LightingExplorerTableColumn;

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
    public TMP_Dropdown DataTypeDropdown;
    //����ѡ����
    public DatePicker_DateRange datePicker;

    //XChartͼ��
    public LineChart lineChart;

    //API����URL
    private string apiBaseUrl = "http://localhost:8080/api/sensors";
    
    //�������б�
    private List<string> sensorTypes = new List<string> { "λ�ƴ�����", "Ӧ��������" };
    private Dictionary<string, List<string>> availableSensorIds = new Dictionary<string, List<string>>();

    // ��������ѡ�����������
    private List<string> displacementDataTypes = new List<string> { "���Ӷ�", "ˮƽλ��", "����" };
    private List<string> stressDataTypes = new List<string> { "Ӧ��" };

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void InitializeDropdowns()
    {
        // ��ʼ������������������
        sensorTypeDropdown.ClearOptions();
        sensorTypeDropdown.AddOptions(sensorTypes);

        // ��ʼ����������������
        DataTypeDropdown.ClearOptions();
        DataTypeDropdown.AddOptions(displacementDataTypes);

        // �Ӻ�˻�ȡ���п��õĴ�����ID������䵽�ֵ���
        StartCoroutine(FetchAllSensorIds());
    }

    public void SetDropdownSelections(string sensorType, string dataType)
    {
        // 1. ���ô���������������
        int sensorTypeIndex = sensorTypes.IndexOf(sensorType);
        if (sensorTypeIndex >= 0)
        {
            sensorTypeDropdown.value = sensorTypeIndex;

            // ȷ�����������������Ѹ���
            OnSensorTypeChanged(sensorTypeIndex);
        }

        // 2. ������������������
        List<string> currentDataTypes = new List<string>();
        if (sensorType == "λ�ƴ�����")
        {
            currentDataTypes = displacementDataTypes;
        }
        else if (sensorType == "Ӧ��������")
        {
            currentDataTypes = stressDataTypes;
        }

        int dataTypeIndex = currentDataTypes.IndexOf(dataType);
        if (dataTypeIndex >= 0)
        {
            DataTypeDropdown.value = dataTypeIndex;
        }

        // ��󣬴���һ�β�ѯ������ͼ��
        OnQueryButtonClicked();
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

        // ���ݴ��������ͣ��������������������ѡ��
        if (selectedType == "λ�ƴ�����")
        {
            DataTypeDropdown.AddOptions(displacementDataTypes);
        }
        else if (selectedType == "Ӧ��������")
        {
            DataTypeDropdown.AddOptions(stressDataTypes);
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
        string selectedDataType = DataTypeDropdown.options[DataTypeDropdown.value].text;
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
            StartCoroutine(FetchDisplacementData(selectedId, startTime, endTime, selectedDataType));
        }
        else if (selectedType == "Ӧ��������")
        {
            StartCoroutine(FetchStressData(selectedId, startTime, endTime, selectedDataType));
        }
    }

    IEnumerator FetchDisplacementData(string sensorId, string startTime, string endTime, string dataType)
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

                        double value = 0;
                        // ����ѡ����������ͣ������ݵ�����ȡ��Ӧ��ֵ
                        switch (dataType)
                        {
                            case "���Ӷ�":
                                value = dataPoint.deflectionValue; // ��ĺ���Ѿ����������ֵ
                                break;
                            case "ˮƽλ��":
                                value = dataPoint.totalHorizontalDisplacement;
                                break;
                            case "����":
                                value = dataPoint.settlement;
                                break;
                            default:
                                value = dataPoint.deflectionValue;
                                break;
                        }


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
                        series.AddXYData(unixTimestamp, value);
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

    IEnumerator FetchStressData(string sensorId, string startTime, string endTime, string dataType)
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

    void OnDataTypeChanged(int index)
    {
        // ���������͸ı�ʱ�����½��в�ѯ
        OnQueryButtonClicked();
    }

    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        qureyButton.onClick.AddListener(OnQueryButtonClicked);
        refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        DataTypeDropdown.onValueChanged.AddListener(OnDataTypeChanged);
        datePicker.FromDate = System.DateTime.Now.AddDays(-30);
        datePicker.ToDate = System.DateTime.Now;

        InitializeDropdowns();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
