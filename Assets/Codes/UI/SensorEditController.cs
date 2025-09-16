using Assets.Codes.Entities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class SensorEditController : MonoBehaviour
{
    public TMP_Dropdown sensorTypeDropdown;
    public TMP_Dropdown sensorIdDropdown;
    public TMP_InputField xAxisInputField;
    public TMP_InputField yAxisInputField;
    public TMP_InputField zAxisInputField;
    public Button saveButton;
    public Button cancelButton;
    public Button closeButton;

    private SensorsInfo currentSensor;
    private DeformableModelController controller;
    private Dictionary<string, List<string>> allAvailableSensors;
    private Dictionary<int, SensorsInfo> allSensorsInfo;

    public void PopulatePanel(SensorsInfo sensor, DeformableModelController parentController, Dictionary<int, SensorsInfo> allSensorsData)
    {
        controller = parentController;
        currentSensor = sensor;
        allSensorsInfo = allSensorsData;

        UnregisterInputListeners();

        // ���� allSensorsData �������������˵����ֵ�
        Dictionary<string, List<string>> tempAvailableSensors = new Dictionary<string, List<string>>();
        foreach (var info in allSensorsData.Values)
        {
            if (!tempAvailableSensors.ContainsKey(info.sensorType))
            {
                tempAvailableSensors[info.sensorType] = new List<string>();
            }
            tempAvailableSensors[info.sensorType].Add(info.sensorId.ToString());
        }
        allAvailableSensors = tempAvailableSensors;

        // ��䴫�������������˵�
        sensorTypeDropdown.ClearOptions();
        sensorTypeDropdown.AddOptions(new List<string>(allAvailableSensors.Keys));

        if (sensor != null)
        {
            sensorTypeDropdown.value = sensorTypeDropdown.options.FindIndex(option => option.text == sensor.sensorType);
            
            // �ڵ��� OnSensorTypeChanged ֮ǰ��ȷ�������˵���ѡ��
            if (sensorTypeDropdown.options.Count > 0)
            {
                OnSensorTypeChanged(sensorTypeDropdown.value);
            }
            else
            {
                Debug.LogWarning("���������������˵���û�п��õ�ѡ�");
            }
            sensorIdDropdown.value = sensorIdDropdown.options.FindIndex(option => option.text == sensor.sensorId.ToString());
            sensorIdDropdown.interactable = false;

            xAxisInputField.text = sensor.unityBaselineX.ToString();
            yAxisInputField.text = sensor.unityBaselineY.ToString();
            zAxisInputField.text = sensor.unityBaselineZ.ToString();
        }
        else
        {
            sensorIdDropdown.interactable = true;
            // ͬ�����ڵ��� OnSensorTypeChanged ֮ǰ���м��
            if (sensorTypeDropdown.options.Count > 0)
            {
                OnSensorTypeChanged(0);
            }

            xAxisInputField.text = "0";
            yAxisInputField.text = "0";
            zAxisInputField.text = "0";
        }
        RegisterInputListeners();
    }

    // ������������ GizmoHandle ������ʵʱ���������
    public void UpdateCoordinateInputs(Vector3 newLocalPosition)
    {
        // ��ͨ���������UIʱҲ��Ҫ��ʱ�Ƴ�������
        UnregisterInputListeners();

        xAxisInputField.text = newLocalPosition.x.ToString("F3"); // F3��ʾ3λС��
        yAxisInputField.text = newLocalPosition.y.ToString("F3");
        zAxisInputField.text = newLocalPosition.z.ToString("F3");

        RegisterInputListeners();
    }


    void OnSaveButtonClicked()
    {
        SensorsInfo sensorToSave = new SensorsInfo();

        if (currentSensor != null)
        {
            // �༭���д�����
            sensorToSave = currentSensor;
        }
        else
        {
            // �½�������
            if (string.IsNullOrEmpty(sensorIdDropdown.captionText.text))
            {
                Debug.LogError("��ѡ��һ��������ID��");
                return;
            }
            sensorToSave.sensorId = int.Parse(sensorIdDropdown.captionText.text);
            sensorToSave.sensorType = sensorTypeDropdown.captionText.text;
        }

        // ���� Unity ��׼����
        sensorToSave.unityBaselineX = double.Parse(xAxisInputField.text);
        sensorToSave.unityBaselineY = double.Parse(yAxisInputField.text);
        sensorToSave.unityBaselineZ = double.Parse(zAxisInputField.text);

        // �������������ķ�������������
        StartCoroutine(controller.UpdateAndSaveSensor(sensorToSave));

        // �ر����
        //gameObject.SetActive(false);
    }

    void OnCancelButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnSensorTypeChanged(int index)
    {
        string selectedType = sensorTypeDropdown.options[index].text;
        sensorIdDropdown.ClearOptions();
        if (allAvailableSensors.ContainsKey(selectedType))
        {
            sensorIdDropdown.AddOptions(allAvailableSensors[selectedType]);
        }
        // �����͸ı�ʱ��������Ǳ༭ģʽ��������������
        if (currentSensor == null)
        {
            xAxisInputField.text = "0";
            yAxisInputField.text = "0";
            zAxisInputField.text = "0";
        }
    }

    private void OnSensorIdChanged(int index)
    {
        // ����Ǳ༭ģʽ����ID�ı�ʱ����ȡ��Ӧ����������Ϣ����������
        if (currentSensor != null && index >= 0)
        {
            // ����Ҫ����һ�������� DeformableModelController ��ͨ��ID��ȡ������Ϣ
            // ����Ϊ�˼򻯣������Ѿ�����
            int selectedId = int.Parse(sensorIdDropdown.options[index].text);
            if (allSensorsInfo.ContainsKey(selectedId))
            {
                SensorsInfo info = allSensorsInfo[selectedId];
                UnregisterInputListeners();
                xAxisInputField.text = info.unityBaselineX.ToString();
                yAxisInputField.text = info.unityBaselineY.ToString();
                zAxisInputField.text = info.unityBaselineZ.ToString();
                RegisterInputListeners();
            }
        }
    }

    private void OnCoordinateChanged(string value)
    {
        // ȷ��������ڱ༭һ���Ѵ��ڵĴ�����
        if (currentSensor != null)
        {
            // ��������ȡ��ǰ����ֵ
            if (float.TryParse(xAxisInputField.text, out float x) &&
                float.TryParse(yAxisInputField.text, out float y) &&
                float.TryParse(zAxisInputField.text, out float z))
            {
                // ��ֵ��װ�� Vector3
                Vector3 newPosition = new Vector3(x, y, z);

                // ���� DeformableModelController �ķ��������� Gizmo λ��
                controller.UpdateSensorGizmoPosition(currentSensor.sensorId, newPosition);
            }
        }
    }

    // ����������ע����Ƴ�������
    private void RegisterInputListeners()
    {
        xAxisInputField.onValueChanged.AddListener(OnCoordinateChanged);
        yAxisInputField.onValueChanged.AddListener(OnCoordinateChanged);
        zAxisInputField.onValueChanged.AddListener(OnCoordinateChanged);
    }

    private void UnregisterInputListeners()
    {
        xAxisInputField.onValueChanged.RemoveListener(OnCoordinateChanged);
        yAxisInputField.onValueChanged.RemoveListener(OnCoordinateChanged);
        zAxisInputField.onValueChanged.RemoveListener(OnCoordinateChanged);
    }


    // �������˵��¼�
    private void OnEnable()
    {
        sensorTypeDropdown.onValueChanged.AddListener(OnSensorTypeChanged);
        sensorIdDropdown.onValueChanged.AddListener(OnSensorIdChanged);
    }

    private void OnDisable()
    {
        sensorTypeDropdown.onValueChanged.RemoveListener(OnSensorTypeChanged);
        sensorIdDropdown.onValueChanged.RemoveListener(OnSensorIdChanged);
        UnregisterInputListeners();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
