using Assets.Codes.Entities;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DeformableModelController : MonoBehaviour
{
    // 传感器 Gizmo 预制件
    public GameObject displacementSensorGizmoPrefab;
    public GameObject stressSensorGizmoPrefab;

    private int currentlySelectedSensorId = -1; // 当前选中的传感器ID

    // 新增：用于判断是否处于编辑模式（供GizmoHandle使用）
    public bool IsEditMode => isEditMode;

    private Dictionary<int, GameObject> sensorGizmos = new Dictionary<int, GameObject>();


    // 存储每个子网格的网格数据
    private Mesh[] meshes;
    private Vector3[][] originalVertices;
    private Vector3[][] deformedVertices;
    private Color[][] originalColors;
    private MeshRenderer[] meshRenderers;

    // 存储所有传感器的数据
    private List<SensorData> sensorDataList = new List<SensorData>();

    // 传感器位置映射：将传感器ID映射到其在模型局部坐标系下的位置
    private Dictionary<int, Vector3> sensorPositions = new Dictionary<int, Vector3>();

    // 存储所有传感器的最新位移数据
    private Dictionary<int, Vector3> currentDisplacements = new Dictionary<int, Vector3>();

    // 存储所有传感器的最新应力数据
    private Dictionary<int, float> currentStressValues = new Dictionary<int, float>();

    // 影响半径：用于确定一个传感器影响周围多大范围的顶点
    public float influenceRadius = 5.0f;
    // 编辑模式状态
    private bool isEditMode = false;
    // 后端 API 地址
    private string apiBaseUrl = "http://localhost:8080/api";
    // 应力热图颜色渐变（可选）
    public Gradient stressColorGradient;

    public GameObject editModePanel; // 主编辑面板
    public GameObject sensorEditPanel; // 传感器编辑面板


    // 存储所有传感器的 SensorsInfo 完整信息
    private Dictionary<int, SensorsInfo> allSensorsInfo = new Dictionary<int, SensorsInfo>();




    /// <summary>
    /// 切换编辑模式
    /// </summary>
    public void ToggleEditMode(bool isEditing)
    {
        isEditMode = isEditing;
        editModePanel.SetActive(isEditing);

        // 绑定新建传感器按钮事件（仅在进入编辑模式时绑定一次）
        if (isEditing)
        {
            editModePanel.GetComponentInChildren<Button>().onClick.AddListener(OnNewSensorButtonClicked);
        }

        foreach (var gizmo in sensorGizmos.Values)
        {
            gizmo.SetActive(isEditing);
        }
    }

    // 新增方法：处理新建传感器按钮点击事件
    private void OnNewSensorButtonClicked()
    {
        OpenEditPanel(null);
    }

        // 新增方法：打开传感器编辑面板
    public void OpenEditPanel(int? sensorId)
    {

        if (sensorId.HasValue)
        {
            currentlySelectedSensorId = sensorId.Value;
            GameObject selectedSensorGizmo = sensorGizmos[currentlySelectedSensorId];

            // ... (显示 SensorEditPanel 的代码不变)
            sensorEditPanel.SetActive(true);
            SensorEditController editPanelScript = sensorEditPanel.GetComponent<SensorEditController>();
            editPanelScript.PopulatePanel(allSensorsInfo[sensorId.Value], this, allSensorsInfo);

        }
        else
        {
            currentlySelectedSensorId = -1; // 没有选中传感器
            // ... (新建传感器逻辑，可能不需要Gizmo轴)
            // 如果新建传感器时也需要Gizmo轴，则需要在这里实例化一个
            sensorEditPanel.SetActive(true);
            SensorEditController editPanelScript = sensorEditPanel.GetComponent<SensorEditController>();
            editPanelScript.PopulatePanel(null, this, allSensorsInfo);
        }
    }

    // 新增方法：获取指定ID的传感器Gizmo
    public GameObject GetSensorGizmo(int sensorId)
    {
        if (sensorGizmos.ContainsKey(sensorId))
        {
            return sensorGizmos[sensorId];
        }
        return null;
    }

    // 新增方法：更新内存中的传感器Unity基准位置
    public void UpdateSensorUnityBaseline(int sensorId, Vector3 newPosition)
    {
        if (allSensorsInfo.ContainsKey(sensorId))
        {
            SensorsInfo sensorInfo = allSensorsInfo[sensorId];
            sensorInfo.unityBaselineX = newPosition.x;
            sensorInfo.unityBaselineY = newPosition.y;
            sensorInfo.unityBaselineZ = newPosition.z;

            // 实时更新 SensorEditPanel 中的坐标输入框
            if (sensorEditPanel.activeSelf && currentlySelectedSensorId == sensorId)
            {
                SensorEditController editPanelScript = sensorEditPanel.GetComponent<SensorEditController>();
                editPanelScript.UpdateCoordinateInputs(newPosition);
            }
        }
    }


    /// <summary>
    /// 从后端加载传感器信息并创建 Gizmo
    /// </summary>
    public IEnumerator LoadAndCreateSensors()
    {
        string url = apiBaseUrl + "/sensors"; // 假设这个API返回所有传感器信息
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log(webRequest.downloadHandler.text);

                // 清空旧数据和Gizmo，以便重新加载
                allSensorsInfo.Clear();
                foreach (var gizmo in sensorGizmos.Values) Destroy(gizmo);
                sensorGizmos.Clear();

                List<SensorsInfo> sensors = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);

                foreach (var sensor in sensors)
                {
                    // 将传感器完整信息存入字典
                    allSensorsInfo[sensor.sensorId] = sensor;

                    Vector3 position = new Vector3(
                        (float)sensor.unityBaselineX,
                        (float)sensor.unityBaselineY,
                        (float)sensor.unityBaselineZ
                    );

                    GameObject gizmo = null;
                    if (sensor.sensorType == "位移传感器")
                    {
                        gizmo = Instantiate(displacementSensorGizmoPrefab, position, Quaternion.identity);
                    }
                    else if (sensor.sensorType == "应力传感器")
                    {
                        gizmo = Instantiate(stressSensorGizmoPrefab, position, Quaternion.identity);
                    }

                    if (gizmo != null)
                    {
                        gizmo.GetComponent<SensorGizmo>().SetSensorId(sensor.sensorId);
                        sensorGizmos[sensor.sensorId] = gizmo;
                        AddSensorPosition(sensor.sensorId, position);

                        SensorGizmoClick gizmoClickScript = gizmo.GetComponent<SensorGizmoClick>();
                        if (gizmoClickScript != null)
                        {
                            gizmoClickScript.controller = this; // 将当前的 DeformableModelController 实例赋值
                            gizmoClickScript.sensorId = sensor.sensorId;
                        }
                    }
                }
                // 数据加载和Gizmo创建完成后，再激活主面板
                editModePanel.SetActive(true);
                // 绑定新建传感器按钮事件
                editModePanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners(); // 防止重复绑定
                editModePanel.GetComponentInChildren<Button>().onClick.AddListener(OnNewSensorButtonClicked);
            }
        }
    }

    /// <summary>
    /// 当用户点击保存按钮时调用
    /// </summary>
    public void SaveSensorPositions()
    {
        List<SensorsInfoToSave> updates = new List<SensorsInfoToSave>();

        foreach (var gizmo in sensorGizmos)
        {
            SensorsInfoToSave update = new SensorsInfoToSave
            {
                sensorId = gizmo.Key,
                // 这里你需要获取当前的 real_world_baseline 字段
                // 假设后端返回了这些字段，并在内存中保存着
                // 或者你只需要更新 Unity 基准位置
                unityBaselineX = gizmo.Value.transform.position.x,
                unityBaselineY = gizmo.Value.transform.position.y,
                unityBaselineZ = gizmo.Value.transform.position.z
            };
            updates.Add(update);
        }

        StartCoroutine(SendSaveRequest(updates));
    }

    /// <summary>
    /// 发送保存请求到后端
    /// </summary>
    IEnumerator SendSaveRequest(List<SensorsInfoToSave> updates)
    {
        string url = apiBaseUrl + "/sensors/update_settings";
        string json = JsonConvert.SerializeObject(updates);

        using (UnityWebRequest webRequest = UnityWebRequest.Put(url, json))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("传感器位置保存成功！");
            }
            else
            {
                Debug.LogError("传感器位置保存失败: " + webRequest.error);
            }
        }
    }


    // 新增方法：由编辑面板调用，用于更新数据和保存
    public IEnumerator UpdateAndSaveSensor(SensorsInfo sensorData)
    {
        List<SensorsInfo> updates = new List<SensorsInfo> { sensorData };
        string url = apiBaseUrl + "/sensors/update_settings";
        string json = JsonConvert.SerializeObject(updates);

        using (UnityWebRequest webRequest = UnityWebRequest.Put(url, json))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("传感器位置保存成功！");
                // 成功后，更新内存中的 SensorsInfo 字典
                if (allSensorsInfo.ContainsKey(sensorData.sensorId))
                {
                    allSensorsInfo[sensorData.sensorId] = sensorData;
                }

                // 找到对应的 Gizmo 并更新它的基准位置
                GameObject gizmo = GetSensorGizmo(sensorData.sensorId);
                if (gizmo != null)
                {
                    // 获取 Gizmo 的绑定脚本
                    DisplacementSensorsBinding binding = gizmo.GetComponent<DisplacementSensorsBinding>();
                    if (binding != null)
                    {
                        // 更新 Gizmo 绑定的 unityBaseline 变量
                        binding.unityBaseline = new Vector3(
                            (float)sensorData.unityBaselineX,
                            (float)sensorData.unityBaselineY,
                            (float)sensorData.unityBaselineZ
                        );

                        // 强制更新 Gizmo 的位置，使其立即跳到新的基准点
                        gizmo.transform.position = binding.unityBaseline;
                    }
                }
            }
            else
            {
                Debug.LogError("传感器位置保存失败: " + webRequest.error);
            }
        }
    }

    // 新增方法：由 SensorEditController 调用来更新 Gizmo 位置
    // 这里只处理 Gizmo 的实时移动，不修改数据模型
    public void UpdateSensorGizmoPosition(int sensorId, Vector3 newPosition)
    {
        if (sensorGizmos.ContainsKey(sensorId))
        {
            GameObject gizmo = sensorGizmos[sensorId];

            // 直接更新 Gizmo 的世界位置
            gizmo.transform.position = newPosition;
        }
    }

    void Awake()
    {
        // 自动查找所有子网格
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        meshes = new Mesh[meshFilters.Length];
        originalVertices = new Vector3[meshFilters.Length][];
        deformedVertices = new Vector3[meshFilters.Length][];

        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        originalColors = new Color[meshFilters.Length][];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            //创建一个可写入的网格实例副本
            meshes[i] = Instantiate(meshFilters[i].mesh);
            meshFilters[i].mesh = meshes[i]; // 将新的可写入网格赋值给MeshFilter

            originalVertices[i] = meshes[i].vertices;
            deformedVertices[i] = new Vector3[originalVertices[i].Length];
        }
    }


    /// <summary>
    /// 在 Unity 编辑器中手动设置传感器在模型上的位置
    /// </summary>
    public void AddSensorPosition(int sensorId, Vector3 localPosition)
    {
        sensorPositions[sensorId] = localPosition;
    }

    /// <summary>
    /// 接收并更新位移传感器的最新位移数据
    /// </summary>
    public void UpdateDisplacementData(int sensorId, Vector3 newDisplacement)
    {
        if (sensorPositions.ContainsKey(sensorId))
        {
            currentDisplacements[sensorId] = newDisplacement;
            UpdateModelDeformation();
        }
    }
    /// <summary>
    /// 接收并更新应力传感器的最新应力数据
    /// </summary>
    public void UpdateStressData(int sensorId, float newStress)
    {
        if (sensorPositions.ContainsKey(sensorId))
        {
            currentStressValues[sensorId] = newStress;
            UpdateStressVisualization();
        }
    }

    /// <summary>
    /// 更新模型变形的逻辑
    /// </summary>
    void UpdateModelDeformation()
    {
        for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
        {
            // 将原始顶点复制到变形顶点数组，每次更新都从原始状态开始
            originalVertices[meshIndex].CopyTo(deformedVertices[meshIndex], 0);

            foreach (var displacementEntry in currentDisplacements)
            {
                int sensorId = displacementEntry.Key;
                Vector3 displacement = displacementEntry.Value;

                if (sensorPositions.ContainsKey(sensorId))
                {
                    Vector3 sensorLocalPosition = sensorPositions[sensorId];
                    Vector3 displacementLocal = transform.InverseTransformDirection(displacement);

                    for (int i = 0; i < deformedVertices[meshIndex].Length; i++)
                    {
                        Vector3 vertexLocalPosition = originalVertices[meshIndex][i];
                        float distance = Vector3.Distance(vertexLocalPosition, sensorLocalPosition);

                        if (distance < influenceRadius)
                        {
                            // 计算衰减影响，例如线性衰减
                            float influence = 1.0f - (distance / influenceRadius);
                            deformedVertices[meshIndex][i] += displacementLocal * influence;
                        }
                    }
                }
            }
            // 将变形后的顶点赋值给网格
            meshes[meshIndex].vertices = deformedVertices[meshIndex];
            meshes[meshIndex].RecalculateNormals();
            meshes[meshIndex].RecalculateBounds();
        }
    }

    /// <summary>
    /// 更新模型应力可视化的逻辑
    /// </summary>
    void UpdateStressVisualization()
    {
        for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
        {
            Color[] colors = meshes[meshIndex].colors;

            if (colors.Length == 0) continue; // 跳过没有颜色的网格

            bool hasColorChange = false;

            foreach (var stressEntry in currentStressValues)
            {
                int sensorId = stressEntry.Key;
                float stressValue = stressEntry.Value;

                if (sensorPositions.ContainsKey(sensorId))
                {
                    Vector3 sensorLocalPosition = sensorPositions[sensorId];

                    for (int i = 0; i < colors.Length; i++)
                    {
                        float distance = Vector3.Distance(originalVertices[meshIndex][i], sensorLocalPosition);
                        if (distance < influenceRadius)
                        {
                            hasColorChange = true;
                            float influence = 1.0f - (distance / influenceRadius);
                            float normalizedStress = Mathf.Clamp01(stressValue / 100.0f);
                            Color stressColor = stressColorGradient.Evaluate(normalizedStress);

                            colors[i] = Color.Lerp(colors[i], stressColor, influence);
                        }
                    }
                }
            }

            if (hasColorChange)
            {
                meshes[meshIndex].colors = colors;
            }
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 在游戏开始时立即加载传感器数据
        StartCoroutine(LoadAndCreateSensors());
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
