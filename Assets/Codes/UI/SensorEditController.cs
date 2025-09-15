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

        // 根据 allSensorsData 构建用于下拉菜单的字典
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

        // 填充传感器类型下拉菜单
        sensorTypeDropdown.ClearOptions();
        sensorTypeDropdown.AddOptions(new List<string>(allAvailableSensors.Keys));

        if (sensor != null)
        {
            sensorTypeDropdown.value = sensorTypeDropdown.options.FindIndex(option => option.text == sensor.sensorType);
            
            // 在调用 OnSensorTypeChanged 之前，确保下拉菜单有选项
            if (sensorTypeDropdown.options.Count > 0)
            {
                OnSensorTypeChanged(sensorTypeDropdown.value);
            }
            else
            {
                Debug.LogWarning("传感器类型下拉菜单中没有可用的选项。");
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
            // 同样，在调用 OnSensorTypeChanged 之前进行检查
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

    // 新增方法：由 GizmoHandle 调用来实时更新输入框
    public void UpdateCoordinateInputs(Vector3 newLocalPosition)
    {
        // 在通过代码更新UI时也需要暂时移除监听器
        UnregisterInputListeners();

        xAxisInputField.text = newLocalPosition.x.ToString("F3"); // F3表示3位小数
        yAxisInputField.text = newLocalPosition.y.ToString("F3");
        zAxisInputField.text = newLocalPosition.z.ToString("F3");

        RegisterInputListeners();
    }


    void OnSaveButtonClicked()
    {
        SensorsInfo sensorToSave = new SensorsInfo();

        if (currentSensor != null)
        {
            // 编辑现有传感器
            sensorToSave = currentSensor;
        }
        else
        {
            // 新建传感器
            if (string.IsNullOrEmpty(sensorIdDropdown.captionText.text))
            {
                Debug.LogError("请选择一个传感器ID。");
                return;
            }
            sensorToSave.sensorId = int.Parse(sensorIdDropdown.captionText.text);
            sensorToSave.sensorType = sensorTypeDropdown.captionText.text;
        }

        // 更新 Unity 基准坐标
        sensorToSave.unityBaselineX = double.Parse(xAxisInputField.text);
        sensorToSave.unityBaselineY = double.Parse(yAxisInputField.text);
        sensorToSave.unityBaselineZ = double.Parse(zAxisInputField.text);

        // 调用主控制器的方法来保存数据
        StartCoroutine(controller.UpdateAndSaveSensor(sensorToSave));

        // 关闭面板
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
        // 当类型改变时，如果不是编辑模式，清空坐标输入框
        if (currentSensor == null)
        {
            xAxisInputField.text = "0";
            yAxisInputField.text = "0";
            zAxisInputField.text = "0";
        }
    }

    private void OnSensorIdChanged(int index)
    {
        // 如果是编辑模式，当ID改变时，获取对应传感器的信息并更新坐标
        if (currentSensor != null && index >= 0)
        {
            // 这需要你有一个方法在 DeformableModelController 中通过ID获取完整信息
            // 这里为了简化，假设已经有了
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
        // 确保面板正在编辑一个已存在的传感器
        if (currentSensor != null)
        {
            // 从输入框获取当前坐标值
            if (float.TryParse(xAxisInputField.text, out float x) &&
                float.TryParse(yAxisInputField.text, out float y) &&
                float.TryParse(zAxisInputField.text, out float z))
            {
                // 将值封装成 Vector3
                Vector3 newPosition = new Vector3(x, y, z);

                // 调用 DeformableModelController 的方法来更新 Gizmo 位置
                controller.UpdateSensorGizmoPosition(currentSensor.sensorId, newPosition);
            }
        }
    }

    // 辅助方法：注册和移除监听器
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


    // 绑定下拉菜单事件
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
