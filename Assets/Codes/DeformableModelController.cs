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
    // ������ Gizmo Ԥ�Ƽ�
    public GameObject displacementSensorGizmoPrefab;
    public GameObject stressSensorGizmoPrefab;

    private int currentlySelectedSensorId = -1; // ��ǰѡ�еĴ�����ID

    // �����������ж��Ƿ��ڱ༭ģʽ����GizmoHandleʹ�ã�
    public bool IsEditMode => isEditMode;

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

    public GameObject editModePanel; // ���༭���
    public GameObject sensorEditPanel; // �������༭���


    // �洢���д������� SensorsInfo ������Ϣ
    private Dictionary<int, SensorsInfo> allSensorsInfo = new Dictionary<int, SensorsInfo>();




    /// <summary>
    /// �л��༭ģʽ
    /// </summary>
    public void ToggleEditMode(bool isEditing)
    {
        isEditMode = isEditing;
        editModePanel.SetActive(isEditing);

        // ���½���������ť�¼������ڽ���༭ģʽʱ��һ�Σ�
        if (isEditing)
        {
            editModePanel.GetComponentInChildren<Button>().onClick.AddListener(OnNewSensorButtonClicked);
        }

        foreach (var gizmo in sensorGizmos.Values)
        {
            gizmo.SetActive(isEditing);
        }
    }

    // ���������������½���������ť����¼�
    private void OnNewSensorButtonClicked()
    {
        OpenEditPanel(null);
    }

        // �����������򿪴������༭���
    public void OpenEditPanel(int? sensorId)
    {

        if (sensorId.HasValue)
        {
            currentlySelectedSensorId = sensorId.Value;
            GameObject selectedSensorGizmo = sensorGizmos[currentlySelectedSensorId];

            // ... (��ʾ SensorEditPanel �Ĵ��벻��)
            sensorEditPanel.SetActive(true);
            SensorEditController editPanelScript = sensorEditPanel.GetComponent<SensorEditController>();
            editPanelScript.PopulatePanel(allSensorsInfo[sensorId.Value], this, allSensorsInfo);

        }
        else
        {
            currentlySelectedSensorId = -1; // û��ѡ�д�����
            // ... (�½��������߼������ܲ���ҪGizmo��)
            // ����½�������ʱҲ��ҪGizmo�ᣬ����Ҫ������ʵ����һ��
            sensorEditPanel.SetActive(true);
            SensorEditController editPanelScript = sensorEditPanel.GetComponent<SensorEditController>();
            editPanelScript.PopulatePanel(null, this, allSensorsInfo);
        }
    }

    // ������������ȡָ��ID�Ĵ�����Gizmo
    public GameObject GetSensorGizmo(int sensorId)
    {
        if (sensorGizmos.ContainsKey(sensorId))
        {
            return sensorGizmos[sensorId];
        }
        return null;
    }

    // ���������������ڴ��еĴ�����Unity��׼λ��
    public void UpdateSensorUnityBaseline(int sensorId, Vector3 newPosition)
    {
        if (allSensorsInfo.ContainsKey(sensorId))
        {
            SensorsInfo sensorInfo = allSensorsInfo[sensorId];
            sensorInfo.unityBaselineX = newPosition.x;
            sensorInfo.unityBaselineY = newPosition.y;
            sensorInfo.unityBaselineZ = newPosition.z;

            // ʵʱ���� SensorEditPanel �е����������
            if (sensorEditPanel.activeSelf && currentlySelectedSensorId == sensorId)
            {
                SensorEditController editPanelScript = sensorEditPanel.GetComponent<SensorEditController>();
                editPanelScript.UpdateCoordinateInputs(newPosition);
            }
        }
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
                Debug.Log(webRequest.downloadHandler.text);

                // ��վ����ݺ�Gizmo���Ա����¼���
                allSensorsInfo.Clear();
                foreach (var gizmo in sensorGizmos.Values) Destroy(gizmo);
                sensorGizmos.Clear();

                List<SensorsInfo> sensors = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);

                foreach (var sensor in sensors)
                {
                    // ��������������Ϣ�����ֵ�
                    allSensorsInfo[sensor.sensorId] = sensor;

                    Vector3 position = new Vector3(
                        (float)sensor.unityBaselineX,
                        (float)sensor.unityBaselineY,
                        (float)sensor.unityBaselineZ
                    );

                    GameObject gizmo = null;
                    if (sensor.sensorType == "λ�ƴ�����")
                    {
                        gizmo = Instantiate(displacementSensorGizmoPrefab, position, Quaternion.identity);
                    }
                    else if (sensor.sensorType == "Ӧ��������")
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
                            gizmoClickScript.controller = this; // ����ǰ�� DeformableModelController ʵ����ֵ
                            gizmoClickScript.sensorId = sensor.sensorId;
                        }
                    }
                }
                // ���ݼ��غ�Gizmo������ɺ��ټ��������
                editModePanel.SetActive(true);
                // ���½���������ť�¼�
                editModePanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners(); // ��ֹ�ظ���
                editModePanel.GetComponentInChildren<Button>().onClick.AddListener(OnNewSensorButtonClicked);
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
                unityBaselineX = gizmo.Value.transform.position.x,
                unityBaselineY = gizmo.Value.transform.position.y,
                unityBaselineZ = gizmo.Value.transform.position.z
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


    // �����������ɱ༭�����ã����ڸ������ݺͱ���
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
                Debug.Log("������λ�ñ���ɹ���");
                // �ɹ��󣬸����ڴ��е� SensorsInfo �ֵ�
                if (allSensorsInfo.ContainsKey(sensorData.sensorId))
                {
                    allSensorsInfo[sensorData.sensorId] = sensorData;
                }

                // �ҵ���Ӧ�� Gizmo ���������Ļ�׼λ��
                GameObject gizmo = GetSensorGizmo(sensorData.sensorId);
                if (gizmo != null)
                {
                    // ��ȡ Gizmo �İ󶨽ű�
                    DisplacementSensorsBinding binding = gizmo.GetComponent<DisplacementSensorsBinding>();
                    if (binding != null)
                    {
                        // ���� Gizmo �󶨵� unityBaseline ����
                        binding.unityBaseline = new Vector3(
                            (float)sensorData.unityBaselineX,
                            (float)sensorData.unityBaselineY,
                            (float)sensorData.unityBaselineZ
                        );

                        // ǿ�Ƹ��� Gizmo ��λ�ã�ʹ�����������µĻ�׼��
                        gizmo.transform.position = binding.unityBaseline;
                    }
                }
            }
            else
            {
                Debug.LogError("������λ�ñ���ʧ��: " + webRequest.error);
            }
        }
    }

    // ������������ SensorEditController ���������� Gizmo λ��
    // ����ֻ���� Gizmo ��ʵʱ�ƶ������޸�����ģ��
    public void UpdateSensorGizmoPosition(int sensorId, Vector3 newPosition)
    {
        if (sensorGizmos.ContainsKey(sensorId))
        {
            GameObject gizmo = sensorGizmos[sensorId];

            // ֱ�Ӹ��� Gizmo ������λ��
            gizmo.transform.position = newPosition;
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
            //����һ����д�������ʵ������
            meshes[i] = Instantiate(meshFilters[i].mesh);
            meshFilters[i].mesh = meshes[i]; // ���µĿ�д������ֵ��MeshFilter

            originalVertices[i] = meshes[i].vertices;
            deformedVertices[i] = new Vector3[originalVertices[i].Length];
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
            // ��ԭʼ���㸴�Ƶ����ζ������飬ÿ�θ��¶���ԭʼ״̬��ʼ
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
                            // ����˥��Ӱ�죬��������˥��
                            float influence = 1.0f - (distance / influenceRadius);
                            deformedVertices[meshIndex][i] += displacementLocal * influence;
                        }
                    }
                }
            }
            // �����κ�Ķ��㸳ֵ������
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
        // ����Ϸ��ʼʱ�������ش���������
        StartCoroutine(LoadAndCreateSensors());
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
