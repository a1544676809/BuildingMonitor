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
    // 存储已实例化的传感器对象，以便后续更新
    private Dictionary<int, GameObject> instantiatedSensors = new Dictionary<int, GameObject>();

    IEnumerator FetchAndInstantiateStressSensors()
    {
        // 调用后端获取所有传感器的API
        string sensorsUrl = apiUrl + "/sensors";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(sensorsUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                List<SensorsInfo> allSensorsList = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);

                // 筛选出应力传感器
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
        // 每隔5秒更新一次
        float updateInterval = 5.0f;
        while (true)
        {
            // 在这里添加延迟，等待一段时间再进行下一轮更新
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
        // 注意，这里需要调用应力传感器的最新数据API
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
                    binding.UpdateData(latestData); // <--- 在这里调用绑定脚本的更新方法
                }
            }
        }
    }

    //当MonoBehaviour 创建后，第一次执行 Update 之前调用
    void Start()
    {
        // 启动一个协程，在程序开始时获取所有传感器列表
        StartCoroutine(FetchAndInstantiateStressSensors());

        // 启动一个协程，定期更新传感器数据
        StartCoroutine(UpdateStressSensorDataLoop());
    }

    //Update 方法在每一帧调用
    void Update()
    {
        
    }
}
