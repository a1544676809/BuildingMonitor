using Assets.Codes.Entities;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class DeformableModelController : MonoBehaviour
{
    // 传感器 Gizmo 预制件
    public GameObject displacementSensorGizmoPrefab;
    public GameObject stressSensorGizmoPrefab;

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


    /// <summary>
    /// 切换编辑模式
    /// </summary>
    public void ToggleEditMode(bool isEditing)
    {
        isEditMode = isEditing;
        foreach (var gizmo in sensorGizmos.Values)
        {
            gizmo.SetActive(isEditing); // 在编辑模式下显示 Gizmo
        }
        // 如果退出编辑模式，隐藏保存按钮
        // 如果进入编辑模式，显示保存按钮
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
                List<SensorsInfo> sensors = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);

                foreach (var sensor in sensors)
                {
                    Vector3 position = new Vector3(
                        (float)sensor.unity_baseline_X,
                        (float)sensor.unity_baseline_Y,
                        (float)sensor.unity_baseline_Z
                    );

                    GameObject gizmo = null;
                    if (sensor.sensorType == "位移传感器")
                    {
                        gizmo = Instantiate(displacementSensorGizmoPrefab, position, Quaternion.identity, transform);
                    }
                    else if (sensor.sensorType == "应力传感器")
                    {
                        gizmo = Instantiate(stressSensorGizmoPrefab, position, Quaternion.identity, transform);
                    }

                    if (gizmo != null)
                    {
                        gizmo.GetComponent<SensorGizmo>().SetSensorId(sensor.sensorId);
                        sensorGizmos[sensor.sensorId] = gizmo;
                        AddSensorPosition(sensor.sensorId, position);
                    }
                }
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
                unityBaselineX = gizmo.Value.transform.localPosition.x,
                unityBaselineY = gizmo.Value.transform.localPosition.y,
                unityBaselineZ = gizmo.Value.transform.localPosition.z
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
            meshes[i] = meshFilters[i].mesh;
            originalVertices[i] = meshes[i].vertices;
            deformedVertices[i] = new Vector3[originalVertices[i].Length];

            // 获取并保存原始顶点颜色
            originalColors[i] = meshes[i].colors;
            if (originalColors[i].Length == 0)
            {
                originalColors[i] = Enumerable.Repeat(Color.white, originalVertices[i].Length).ToArray();
            }
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
            originalVertices[meshIndex].CopyTo(deformedVertices[meshIndex], 0);

            foreach (var displacementEntry in currentDisplacements)
            {
                int sensorId = displacementEntry.Key;
                Vector3 displacement = displacementEntry.Value;

                if (sensorPositions.ContainsKey(sensorId))
                {
                    Vector3 sensorLocalPosition = sensorPositions[sensorId];

                    for (int i = 0; i < deformedVertices[meshIndex].Length; i++)
                    {
                        Vector3 vertexLocalPosition = originalVertices[meshIndex][i];
                        float distance = Vector3.Distance(vertexLocalPosition, sensorLocalPosition);

                        if (distance < influenceRadius)
                        {
                            float influence = 1.0f - (distance / influenceRadius);
                            deformedVertices[meshIndex][i] += displacement * influence;
                        }
                    }
                }
            }

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
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
