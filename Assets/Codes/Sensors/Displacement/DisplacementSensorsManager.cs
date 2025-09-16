using Assets.Codes.Entities;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class DisplacementSensorsManager : MonoBehaviour
{

    public GameObject displacementSensorPrefab;
    public SensorInfoPanelController uiController;


    private string apiUrl = "http://localhost:8080/api"; // API URL
    // �洢��ʵ�����Ĵ����������Ա��������
    private Dictionary<int, GameObject> instantiatedSensors = new Dictionary<int, GameObject>();
    private Dictionary<int, SensorsInfo> allSensorsInfo = new Dictionary<int, SensorsInfo>();
    public DeformableModelController deformableModelController; // �������� DeformableModelController ������

    // Э�̣���ȡ���д�������Ϣ��ʵ����Ԥ�Ƽ�
    IEnumerator FetchAndInstantiateDisplacementSensors()
    {
        string sensorsUrl = apiUrl + "/sensors";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(sensorsUrl))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("���������ݻ�ȡʧ��: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                // ����JSON�ַ���ΪSensorsInfo�б�
                List<SensorsInfo> sensorsList = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);


                // ��վ�����
                allSensorsInfo.Clear();
                foreach (var kvp in instantiatedSensors)
                {
                    Destroy(kvp.Value);
                }
                instantiatedSensors.Clear();

                // �����б�Ϊÿ������������ʵ��
                foreach (var sensorInfo in sensorsList)
                {
                    // ��������Ϣ�洢���������� unity_baseline
                    allSensorsInfo[sensorInfo.sensorId] = sensorInfo;
                    // �ڳ�����ʵ����Ԥ�Ƽ�
                    GameObject newSensor = Instantiate(displacementSensorPrefab);
                    //// ��ȡ SensorBinding ���
                    DisplacementSensorsBinding binding = newSensor.GetComponent<DisplacementSensorsBinding>();
                    if(binding != null)
                    {
                        // ��ʵ����ʱ����UI�����������ô��ݸ�������
                        binding.uiController = this.uiController;
                        // �󶨴�����ID
                        binding.sensorId = sensorInfo.sensorId;
                        // ���´����Ķ���洢���ֵ��У��� sensorId Ϊ��
                        binding.unityBaseline = new Vector3(
                        (float)sensorInfo.unityBaselineX,
                        (float)sensorInfo.unityBaselineY,
                        (float)sensorInfo.unityBaselineZ);
                        instantiatedSensors[sensorInfo.sensorId] = newSensor;
                    }
                }
            }
        }
    }

    // Э�̣����ڸ���ÿ��������������
    IEnumerator UpdateDisplacementSensorDataLoop()
    {
        // ÿ��5�����һ��
        float updateInterval = 5.0f;
        while (true)
        {
            // ����������ӳ٣��ȴ�һ��ʱ���ٽ�����һ�ָ���
            yield return new WaitForSeconds(updateInterval);
            // ������ʵ�����Ĵ���������
            //kvp��Key-Value Pair����ֵ�ԣ�����д��ͨ�����ڱ�ʾ�ֵ��е�һ��Ԫ�ء�
            foreach (var kvp in instantiatedSensors)
            {
                DisplacementSensorsBinding binding = kvp.Value.GetComponent<DisplacementSensorsBinding>();
                // ��ȡ����������
                yield return StartCoroutine(FetchSensorLatestData(binding, kvp.Value));
            }
        }
    }

    IEnumerator FetchSensorLatestData(DisplacementSensorsBinding binding, GameObject sensorObject)
    {
        string latestDataUrl = apiUrl + $"/sensors/displacement/{binding.sensorId}/latest";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(latestDataUrl))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                DisplacementSensorData latestData = JsonConvert.DeserializeObject<DisplacementSensorData>(jsonResponse);

                if (binding != null)
                {
                    binding.UpdateData(latestData); // <--- ��������ð󶨽ű��ĸ��·���
                }

                //����λ�Ʋ�Ӧ�õ� Unity ��׼��
                Vector3 displacement = new Vector3(
                    (float)(latestData.horizontalDisplacementX),
                    (float)(latestData.horizontalDisplacementY),
                    (float)(latestData.settlement)
                );

                // �� binding �л�ȡ unityBaseline
                Vector3 unityBaseline = binding.unityBaseline;

                // ���´����������λ��
                sensorObject.transform.position = unityBaseline + displacement;

                // Ҳ�����������������״̬��������ɫ��UI��
                Debug.Log($"Sensor {binding.sensorId} updated to position: {binding.unityBaseline + displacement}");

                // ��������λ�����ݴ��ݸ�ģ�Ϳ�����
                if (deformableModelController != null)
                {
                    deformableModelController.UpdateDisplacementData(binding.sensorId, displacement);
                }
            }
        }

    }


    //��MonoBehaviour �����󣬵�һ��ִ�� Update ֮ǰ����
    void Start()
    {
        // �ڿ�ʼʱ���UI�������Ƿ��Ѱ�
        if (uiController == null)
        {
            Debug.LogError("UI ������δ�󶨣����ڹ����������� UI ���á�");
            return;
        }

        // ����һ��Э�̣��ڳ���ʼʱ��ȡ���д������б�
        StartCoroutine(FetchAndInstantiateDisplacementSensors());

        // ����һ��Э�̣����ڸ��´���������
        StartCoroutine(UpdateDisplacementSensorDataLoop());
    }

    //Update ������ÿһ֡����
    void Update()
    {

    }
}
