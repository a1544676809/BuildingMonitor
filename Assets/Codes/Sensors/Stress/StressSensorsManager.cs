using Assets.Codes.Entities;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StressSensorsManager : MonoBehaviour
{

    public GameObject stressSensorPrefab;
    private string apiUrl = "http://localhost:8080/api"; // API URL
    // �洢��ʵ�����Ĵ����������Ա��������
    private Dictionary<int, GameObject> instantiatedSensors = new Dictionary<int, GameObject>();

    IEnumerator FetchAndInstantiateStressSensors()
    {
        // ���ú�˻�ȡ���д�������API
        string sensorsUrl = apiUrl + "/sensors";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(sensorsUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                List<SensorsInfo> allSensorsList = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);

                // ɸѡ��Ӧ��������
                foreach (var sensorInfo in allSensorsList)
                {
                    if (sensorInfo.sensorType == "stress")
                    {
                        GameObject newSensor = Instantiate(stressSensorPrefab);

                        StressSensorsBinding binding = newSensor.GetComponent<StressSensorsBinding>();
                        if (binding != null)
                        {
                            binding.sensorId = sensorInfo.sensorId;
                            instantiatedSensors[sensorInfo.sensorId] = newSensor;
                        }
                    }
                }
            }
        }
    }

    IEnumerator UpdateStressSensorDataLoop()
    {
        // ÿ��5�����һ��
        float updateInterval = 5.0f;
        while (true)
        {
            // ����������ӳ٣��ȴ�һ��ʱ���ٽ�����һ�ָ���
            yield return new WaitForSeconds(updateInterval);
            foreach (var kvp in instantiatedSensors)
            {
                StressSensorsBinding binding = kvp.Value.GetComponent<StressSensorsBinding>();
                if (binding != null)
                {
                    StartCoroutine(FetchAndProcessLatestStressData(binding));
                }
            }
            yield return new WaitForSeconds(5.0f);
        }
    }

    IEnumerator FetchAndProcessLatestStressData(StressSensorsBinding binding)
    {
        // ע�⣬������Ҫ����Ӧ������������������API
        string latestDataUrl = apiUrl + $"/sensors/stress/{binding.sensorId}/latest";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(latestDataUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                StressSensorData latestData = JsonConvert.DeserializeObject<StressSensorData>(jsonResponse);


                if (binding != null)
                {
                    binding.UpdateData(latestData); // <--- ��������ð󶨽ű��ĸ��·���
                }
            }
        }
    }

    //��MonoBehaviour �����󣬵�һ��ִ�� Update ֮ǰ����
    void Start()
    {
        // ����һ��Э�̣��ڳ���ʼʱ��ȡ���д������б�
        StartCoroutine(FetchAndInstantiateStressSensors());

        // ����һ��Э�̣����ڸ��´���������
        StartCoroutine(UpdateStressSensorDataLoop());
    }

    //Update ������ÿһ֡����
    void Update()
    {
        
    }
}
