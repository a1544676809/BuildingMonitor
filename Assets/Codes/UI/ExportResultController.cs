using Assets.Codes.Entities;
using OfficeOpenXml; // ���� EPPlus �����ռ�
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

    // �������ݿ��ӻ���壬�Ա��ȡ���ѯ����
    public DataVisualizationPanel dataVisualizationPanel;

    private string apiBaseUrl = "http://localhost:8080/api/sensors";

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void OnExportButtonClicked()
    {
        // ȷ�������� DataVisualizationPanel
        if (dataVisualizationPanel == null)
        {
            Debug.LogError("DataVisualizationPanel ����δ���ã�");
            return;
        }

        // ��ȡ��ǰ���ݿ��ӻ����Ĳ�ѯ����
        string sensorId = dataVisualizationPanel.sensorDropdown.options[dataVisualizationPanel.sensorDropdown.value].text;
        string startDate = dataVisualizationPanel.datePicker.FromDate.Date.ToString("yyyy-MM-dd") + "T00:00:00";
        string endDate = dataVisualizationPanel.datePicker.ToDate.Date.ToString("yyyy-MM-dd") + "T23:59:59";
        string sensorType = dataVisualizationPanel.sensorTypeDropdown.options[dataVisualizationPanel.sensorTypeDropdown.value].text;

        // ���ѡ��Ĳ���λ�ƴ���������ִ�е���
        if (sensorType != "λ�ƴ�����")
        {
            Debug.LogWarning("�������ܽ�֧��λ�ƴ�������");
            return;
        }

        // ����Э������ȡ���ݲ�����
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
                    // ������ִ�е����߼�
                    ExportToExcel(historyData);
                }
                else
                {
                    Debug.Log("û����ʷ���ݿɵ�����");
                }
            }
            else
            {
                Debug.LogError("��ȡ��ʷ����ʧ��: " + webRequest.error);
            }
        }
    }

    void ExportToExcel(List<DisplacementSensorData> data)
    {
        // ����һ���µ� Excel �ļ���
        using (var package = new ExcelPackage())
        {
            // ���ÿ�� Toggle ��״̬�����ѡ���򴴽�һ���µĹ�����
            if (totalDeflectionToggle.isOn)
            {
                CreateWorksheet(package, "���Ӷ�", data, "���Ӷ�", d => d.deflectionValue);
            }
            if (horizontalDisplacementToggle.isOn)
            {
                CreateWorksheet(package, "ˮƽλ��", data, "ˮƽλ��", d => d.totalHorizontalDisplacement);
            }
            if (settlementToggle.isOn)
            {
                CreateWorksheet(package, "������", data, "������", d => d.settlement);
            }

            // ��ȡ�����ļ���·��
            string fileName = $"������_{data[0].sensorId}_λ������_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            Debug.Log($"���ڽ��ļ�������: {filePath}");

            // �����ļ�
            File.WriteAllBytes(filePath, package.GetAsByteArray());
            Debug.Log("������ɣ�");
        }
    }

    // ͨ�÷���������������д������
    void CreateWorksheet(ExcelPackage package, string sheetName, List<DisplacementSensorData> data, string valueHeader, System.Func<DisplacementSensorData, double> valueSelector)
    {
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        // д���ͷ
        worksheet.Cells[1, 1].Value = "ʱ���";
        worksheet.Cells[1, 2].Value = valueHeader;

        // д������
        for (int i = 0; i < data.Count; i++)
        {
            var row = worksheet.Cells[i + 2, 1];
            row.Value = data[i].timestamp.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
            row.Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
            worksheet.Cells[i + 2, 2].Value = valueSelector(data[i]);
        }

        // �Զ������п�
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        exportButton.onClick.AddListener(OnExportButtonClicked);
        // ���� EPPlus ���֤
        ExcelPackage.License.SetNonCommercialPersonal("sdust");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
