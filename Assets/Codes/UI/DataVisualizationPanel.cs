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
    //UI面板上的各个组件
    //文本
    public TextMeshProUGUI dateTimeRangeText;
    //按钮
    public Button closeButton;
    public Button qureyButton;
    public Button refreshButton;
    //下拉菜单
    public TMP_Dropdown sensorTypeDropdown;
    public TMP_Dropdown sensorDropdown;
    //日期选择器
    public DatePicker_DateRange datePicker;

    //XChart图标
    public LineChart lineChart;

    //API基础URL
    private string apiBaseUrl = "http://localhost:8080/api/sensors";
    
    //传感器列表
    private List<string> sensorTypes = new List<string> { "位移传感器", "应力传感器" };
    private Dictionary<string, List<string>> availableSensorIds = new Dictionary<string, List<string>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void InitializeDropdowns()
    {
        // 填充传感器类型下拉框
        sensorTypeDropdown.ClearOptions();
        sensorTypeDropdown.AddOptions(sensorTypes);

        // 从后端获取所有可用的传感器ID，并填充到字典中
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
                // 假设后端返回一个包含所有传感器信息的JSON
                string jsonResponse = webRequest.downloadHandler.text;
                // 你需要一个对应后端返回的SensorInfo类
                var sensorInfoList = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);
                // 根据传感器类型分组ID
                availableSensorIds.Clear();

                foreach (var info in sensorInfoList)
                {
                    if (!availableSensorIds.ContainsKey(info.sensorType)) // info.type 对应 "位移传感器" 或 "应力传感器"
                    {
                        availableSensorIds[info.sensorType] = new List<string>();
                    }
                    availableSensorIds[info.sensorType].Add(info.sensorId.ToString());
                }
                // 初始化传感器ID下拉框
                OnSensorTypeChanged(0);
            }
            else
            {
                Debug.LogError("获取所有传感器ID失败: " + webRequest.error);
            }
        }
    }

    // 传感器类型下拉框值改变时调用
    void OnSensorTypeChanged(int index)
    {
        string selectedType = sensorTypes[index];
        sensorDropdown.ClearOptions();
        if (availableSensorIds.ContainsKey(selectedType))
        {
            sensorDropdown.AddOptions(availableSensorIds[selectedType]);
        }

        // 当类型改变时，自动触发一次查询以显示默认数据
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

        // 获取日期选择器的值
        string startDate = datePicker.FromDate.Date.ToString("yyyy-MM-dd");
        string endDate = datePicker.ToDate.Date.ToString("yyyy-MM-dd");

        // 自动为起始日期添加00:00:00
        string startTime = startDate + "T00:00:00";
        // 自动为结束日期添加23:59:59，以包含结束日期的所有数据
        string endTime = endDate + "T23:59:59";

        Debug.Log($"开始日期: {startTime}, 结束日期: {endTime}");

        // 根据传感器类型调用不同的协程
        if (selectedType == "位移传感器")
        {
            StartCoroutine(FetchDisplacementData(selectedId, startTime, endTime));
        }
        else if (selectedType == "应力传感器")
        {
            StartCoroutine(FetchStressData(selectedId, startTime, endTime));
        }
    }

    IEnumerator FetchDisplacementData(string sensorId, string startTime, string endTime)
    {
        string url = $"{apiBaseUrl}/displacement/history/{sensorId}?startTime={startTime}&endTime={endTime}";
        // ... 发送UnityWebRequest并处理响应，更新lineChart ...
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

                    // 找出最早和最晚的日期
                    DateTime minDate = DateTime.MaxValue;
                    DateTime maxDate = DateTime.MinValue;

                    foreach (var dataPoint in historyData)
                    {
                        DateTime dataPointTime = dataPoint.timestamp.DateTime;
                        long unixTimestamp = dataPoint.timestamp.ToUnixTimeSeconds();

                        // 更新最早和最晚日期
                        if (dataPointTime < minDate)
                        {
                            minDate = dataPointTime;
                        }
                        if (dataPointTime > maxDate)
                        {
                            maxDate = dataPointTime;
                        }
                        // 添加包含时间戳和数据值的数据点
                        series.AddXYData(unixTimestamp, dataPoint.totalDisplacement);
                    }
                    UpdateDateTimeRangeText(minDate, maxDate);
                    Debug.Log("位移数据点数量: " + historyData.Count);
                }
                else
                {
                    lineChart.RemoveAllSerie();
                    UpdateDateTimeRangeText(DateTime.Now, DateTime.Now);
                    Debug.Log("没有足够的位移传感器历史数据来绘制图表。");
                }
            }
            else
            {
                Debug.LogError("获取位移数据失败: " + webRequest.error);
            }
        }
    }

    IEnumerator FetchStressData(string sensorId, string startTime, string endTime)
    {
        // 假设后端有不同的API来获取应力数据
        string url = $"{apiBaseUrl}/stress/history/{sensorId}?startTime={startTime}&endTime={endTime}";
        // ... 编写类似的UnityWebRequest逻辑来获取并绘制应力数据 ...
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            // ... (后续逻辑与FetchDisplacementData类似)
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

                    // 找出最早和最晚的日期
                    DateTime minDate = DateTime.MaxValue;
                    DateTime maxDate = DateTime.MinValue;

                    foreach (var dataPoint in historyData)
                    {
                        DateTime dataPointTime = dataPoint.timestamp.DateTime;
                        long unixTimestamp = dataPoint.timestamp.ToUnixTimeSeconds();

                        // 更新最早和最晚日期
                        if (dataPointTime < minDate)
                        {
                            minDate = dataPointTime;
                        }
                        if (dataPointTime > maxDate)
                        {
                            maxDate = dataPointTime;
                        }
                        // 添加包含时间戳和数据值的数据点
                        series.AddXYData(unixTimestamp, dataPoint.totalDisplacement);
                    }
                    UpdateDateTimeRangeText(minDate, maxDate);
                }
                else
                {
                    lineChart.RemoveAllSerie();
                    UpdateDateTimeRangeText(DateTime.Now, DateTime.Now);
                    Debug.Log("没有足够的应力传感器历史数据来绘制图表。");
                }
            }
            else
            {
                Debug.LogError("获取应力数据失败: " + webRequest.error);
            }

        }
    }

    void UpdateDateTimeRangeText(DateTime minDate, DateTime maxDate)
    {
        if (dateTimeRangeText != null)
        {
            string formattedMinDate = minDate.ToString("yyyy-MM-dd HH:mm:ss");
            string formattedMaxDate = maxDate.ToString("yyyy-MM-dd HH:mm:ss");
            dateTimeRangeText.text = $"实际数据范围：{formattedMinDate} - {formattedMaxDate}";
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
