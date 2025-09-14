using Assets.Codes.Entities;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class DeformableModelController : MonoBehaviour
{
    // ������ Gizmo Ԥ�Ƽ�
    public GameObject displacementSensorGizmoPrefab;
    public GameObject stressSensorGizmoPrefab;

    private Dictionary<int, GameObject> sensorGizmos = new Dictionary<int, GameObject>();
    // �洢ÿ�����������������
    private Mesh[] meshes;
    private Vector3[][] originalVertices;
    private Vector3[][] deformedVertices;
    private Color[][] originalColors;
    private MeshRenderer[] meshRenderers;

    // �洢���д�����������
    private List<SensorData> sensorDataList = new List<SensorData>();

    // ������λ��ӳ�䣺��������IDӳ�䵽����ģ�;ֲ�����ϵ�µ�λ��
    private Dictionary<int, Vector3> sensorPositions = new Dictionary<int, Vector3>();

    // �洢���д�����������λ������
    private Dictionary<int, Vector3> currentDisplacements = new Dictionary<int, Vector3>();

    // �洢���д�����������Ӧ������
    private Dictionary<int, float> currentStressValues = new Dictionary<int, float>();

    // Ӱ��뾶������ȷ��һ��������Ӱ����Χ���Χ�Ķ���
    public float influenceRadius = 5.0f;
    // �༭ģʽ״̬
    private bool isEditMode = false;
    // ��� API ��ַ
    private string apiBaseUrl = "http://localhost:8080/api";
    // Ӧ����ͼ��ɫ���䣨��ѡ��
    public Gradient stressColorGradient;


    /// <summary>
    /// �л��༭ģʽ
    /// </summary>
    public void ToggleEditMode(bool isEditing)
    {
        isEditMode = isEditing;
        foreach (var gizmo in sensorGizmos.Values)
        {
            gizmo.SetActive(isEditing); // �ڱ༭ģʽ����ʾ Gizmo
        }
        // ����˳��༭ģʽ�����ر��水ť
        // �������༭ģʽ����ʾ���水ť
    }

    /// <summary>
    /// �Ӻ�˼��ش�������Ϣ������ Gizmo
    /// </summary>
    public IEnumerator LoadAndCreateSensors()
    {
        string url = apiBaseUrl + "/sensors"; // �������API�������д�������Ϣ
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
                    if (sensor.sensorType == "λ�ƴ�����")
                    {
                        gizmo = Instantiate(displacementSensorGizmoPrefab, position, Quaternion.identity, transform);
                    }
                    else if (sensor.sensorType == "Ӧ��������")
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
    /// ���û�������水ťʱ����
    /// </summary>
    public void SaveSensorPositions()
    {
        List<SensorsInfoToSave> updates = new List<SensorsInfoToSave>();

        foreach (var gizmo in sensorGizmos)
        {
            SensorsInfoToSave update = new SensorsInfoToSave
            {
                sensorId = gizmo.Key,
                // ��������Ҫ��ȡ��ǰ�� real_world_baseline �ֶ�
                // �����˷�������Щ�ֶΣ������ڴ��б�����
                // ������ֻ��Ҫ���� Unity ��׼λ��
                unityBaselineX = gizmo.Value.transform.localPosition.x,
                unityBaselineY = gizmo.Value.transform.localPosition.y,
                unityBaselineZ = gizmo.Value.transform.localPosition.z
            };
            updates.Add(update);
        }

        StartCoroutine(SendSaveRequest(updates));
    }

    /// <summary>
    /// ���ͱ������󵽺��
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
                Debug.Log("������λ�ñ���ɹ���");
            }
            else
            {
                Debug.LogError("������λ�ñ���ʧ��: " + webRequest.error);
            }
        }
    }


    void Awake()
    {
        // �Զ���������������
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

            // ��ȡ������ԭʼ������ɫ
            originalColors[i] = meshes[i].colors;
            if (originalColors[i].Length == 0)
            {
                originalColors[i] = Enumerable.Repeat(Color.white, originalVertices[i].Length).ToArray();
            }
        }
    }


    /// <summary>
    /// �� Unity �༭�����ֶ����ô�������ģ���ϵ�λ��
    /// </summary>
    public void AddSensorPosition(int sensorId, Vector3 localPosition)
    {
        sensorPositions[sensorId] = localPosition;
    }

    /// <summary>
    /// ���ղ�����λ�ƴ�����������λ������
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
    /// ���ղ�����Ӧ��������������Ӧ������
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
    /// ����ģ�ͱ��ε��߼�
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
    /// ����ģ��Ӧ�����ӻ����߼�
    /// </summary>
    void UpdateStressVisualization()
    {
        for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
        {
            Color[] colors = meshes[meshIndex].colors;

            if (colors.Length == 0) continue; // ����û����ɫ������

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
