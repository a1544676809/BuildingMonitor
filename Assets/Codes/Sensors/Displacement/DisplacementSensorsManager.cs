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
    // 存储已实例化的传感器对象，以便后续更新
    private Dictionary<int, GameObject> instantiatedSensors = new Dictionary<int, GameObject>();

    // 协程：获取所有传感器信息并实例化预制件
    IEnumerator FetchAndInstantiateDisplacementSensors()
    {
        string sensorsUrl = apiUrl + "/sensors";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(sensorsUrl))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("传感器数据获取失败: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                // 解析JSON字符串为SensorsInfo列表
                List<SensorsInfo> sensorsList = JsonConvert.DeserializeObject<List<SensorsInfo>>(jsonResponse);

                // 遍历列表，为每个传感器创建实例
                foreach (var sensorInfo in sensorsList)
                {
                    // 在场景中实例化预制件
                    GameObject newSensor = Instantiate(displacementSensorPrefab);
                    //// 获取 SensorBinding 组件
                    DisplacementSensorsBinding binding = newSensor.GetComponent<DisplacementSensorsBinding>();
                    if(binding != null)
                    {
                        // 在实例化时，将UI控制器的引用传递给传感器
                        binding.uiController = this.uiController;
                        // 绑定传感器ID
                        binding.sensorId = sensorInfo.sensorId;
                        // 将新创建的对象存储到字典中，以 sensorId 为键
                        instantiatedSensors[sensorInfo.sensorId] = newSensor;
                    }
                }
            }
        }
    }

    // 协程：定期更新每个传感器的数据
    IEnumerator UpdateDisplacementSensorDataLoop()
    {
        // 每隔5秒更新一次
        float updateInterval = 5.0f;
        while (true)
        {
            // 在这里添加延迟，等待一段时间再进行下一轮更新
            yield return new WaitForSeconds(updateInterval);
            // 遍历已实例化的传感器对象
            //kvp是Key-Value Pair（键值对）的缩写，通常用于表示字典中的一个元素。
            foreach (var kvp in instantiatedSensors)
            {
                DisplacementSensorsBinding binding = kvp.Value.GetComponent<DisplacementSensorsBinding>();
                // 获取传感器数据
                yield return StartCoroutine(FetchSensorLatestData(binding,displacementSensorPrefab));
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
                    binding.UpdateData(latestData); // <--- 在这里调用绑定脚本的更新方法
                }

                // 将后端坐标转换为Unity世界坐标
                Vector3 newPosition = new Vector3((float)latestData.currentX, (float)latestData.currentY, (float)latestData.currentZ);

                // 更新传感器对象的位置
                sensorObject.transform.position = newPosition;

                // 也可以在这里更新其他状态，例如颜色、UI等
                Debug.Log($"Sensor {binding.sensorId} updated to position: {newPosition}");
            }
        }

    }

    //当MonoBehaviour 创建后，第一次执行 Update 之前调用
    void Start()
    {
        // 在开始时检查UI控制器是否已绑定
        if (uiController == null)
        {
            Debug.LogError("UI 控制器未绑定！请在管理器上设置 UI 引用。");
            return;
        }

        // 启动一个协程，在程序开始时获取所有传感器列表
        StartCoroutine(FetchAndInstantiateDisplacementSensors());

        // 启动一个协程，定期更新传感器数据
        StartCoroutine(UpdateDisplacementSensorDataLoop());
    }

    //Update 方法在每一帧调用
    void Update()
    {

    }
}
