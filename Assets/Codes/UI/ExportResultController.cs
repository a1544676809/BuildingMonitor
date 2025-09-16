using Assets.Codes.Entities;
using OfficeOpenXml; // 引入 EPPlus 命名空间
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ExportResultController : MonoBehaviour
{
    public Button closeButton;
    public Button exportButton;
    public Toggle totalDeflectionToggle;
    public Toggle horizontalDisplacementToggle;
    public Toggle settlementToggle;

    // 引用数据可视化面板，以便获取其查询参数
    public DataVisualizationPanel dataVisualizationPanel;

    private string apiBaseUrl = "http://localhost:8080/api/sensors";

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void OnExportButtonClicked()
    {
        // 确保关联了 DataVisualizationPanel
        if (dataVisualizationPanel == null)
        {
            Debug.LogError("DataVisualizationPanel 引用未设置！");
            return;
        }

        // 获取当前数据可视化面板的查询参数
        string sensorId = dataVisualizationPanel.sensorDropdown.options[dataVisualizationPanel.sensorDropdown.value].text;
        string startDate = dataVisualizationPanel.datePicker.FromDate.Date.ToString("yyyy-MM-dd") + "T00:00:00";
        string endDate = dataVisualizationPanel.datePicker.ToDate.Date.ToString("yyyy-MM-dd") + "T23:59:59";
        string sensorType = dataVisualizationPanel.sensorTypeDropdown.options[dataVisualizationPanel.sensorTypeDropdown.value].text;

        // 如果选择的不是位移传感器，则不执行导出
        if (sensorType != "位移传感器")
        {
            Debug.LogWarning("导出功能仅支持位移传感器。");
            return;
        }

        // 启动协程来获取数据并导出
        StartCoroutine(FetchDataAndExport(sensorId, startDate, endDate));
    }

    IEnumerator FetchDataAndExport(string sensorId, string startTime, string endTime)
    {
        string url = $"{apiBaseUrl}/displacement/history/{sensorId}?startTime={startTime}&endTime={endTime}";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                List<DisplacementSensorData> historyData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DisplacementSensorData>>(jsonResponse);

                if (historyData != null && historyData.Count > 0)
                {
                    // 在这里执行导出逻辑
                    ExportToExcel(historyData);
                }
                else
                {
                    Debug.Log("没有历史数据可导出。");
                }
            }
            else
            {
                Debug.LogError("获取历史数据失败: " + webRequest.error);
            }
        }
    }

    void ExportToExcel(List<DisplacementSensorData> data)
    {
        // 创建一个新的 Excel 文件包
        using (var package = new ExcelPackage())
        {
            // 检查每个 Toggle 的状态，如果选中则创建一个新的工作表
            if (totalDeflectionToggle.isOn)
            {
                CreateWorksheet(package, "总挠度", data, "总挠度", d => d.deflectionValue);
            }
            if (horizontalDisplacementToggle.isOn)
            {
                CreateWorksheet(package, "水平位移", data, "水平位移", d => d.totalHorizontalDisplacement);
            }
            if (settlementToggle.isOn)
            {
                CreateWorksheet(package, "沉降量", data, "沉降量", d => d.settlement);
            }

            // 获取导出文件的路径
            string fileName = $"传感器_{data[0].sensorId}_位移数据_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            Debug.Log($"正在将文件导出到: {filePath}");

            // 保存文件
            File.WriteAllBytes(filePath, package.GetAsByteArray());
            Debug.Log("导出完成！");
        }
    }

    // 通用方法：创建工作表并写入数据
    void CreateWorksheet(ExcelPackage package, string sheetName, List<DisplacementSensorData> data, string valueHeader, System.Func<DisplacementSensorData, double> valueSelector)
    {
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        // 写入表头
        worksheet.Cells[1, 1].Value = "时间戳";
        worksheet.Cells[1, 2].Value = valueHeader;

        // 写入数据
        for (int i = 0; i < data.Count; i++)
        {
            var row = worksheet.Cells[i + 2, 1];
            row.Value = data[i].timestamp.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
            row.Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
            worksheet.Cells[i + 2, 2].Value = valueSelector(data[i]);
        }

        // 自动调整列宽
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        exportButton.onClick.AddListener(OnExportButtonClicked);
        // 设置 EPPlus 许可证
        ExcelPackage.License.SetNonCommercialPersonal("sdust");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
