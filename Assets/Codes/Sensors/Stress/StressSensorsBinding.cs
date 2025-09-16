using Assets.Codes.Entities;
using UnityEngine;

public class StressSensorsBinding : MonoBehaviour
{
    public int sensorId;
    public StressSensorData latestData;

    // 缓存对UI控制器的引用
    public SensorInfoPanelController uiController;
    // 新增：用于存储传感器的Unity基准点
    public Vector3 unityBaseline;


    void OnMouseDown()
    {
        Debug.Log("传感器被点击了!");
        if (uiController != null)
        {
            // 如果UI已经可见，则隐藏它
            if (uiController.gameObject.activeSelf)
            {
                uiController.HidePanel();
            }
            else
            {
                if (uiController.editModeToggle.isOn == false)
                {
                    // 调用UI控制器的方法，将当前传感器的数据传递给它
                    uiController.ShowPanel(sensorId, latestData, unityBaseline);
                }
            }
        }
    }


    public void UpdateData(StressSensorData newData)
    {
        this.latestData = newData;

        // 根据应力值来改变预制件的颜色
        // 假设当应力值超过 100 MPa 时，颜色变为红色
        Renderer renderer = transform.Find("StressSensorModel")?.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (newData.stressValue > 100.0)
            {
                renderer.material.color = Color.red;    
            }
            else
            {
                renderer.material.color = Color.green;
            }
        }
    }

    public void Start()
    {
        // 自动查找并填充 uiController 变量
        if (uiController == null)
        {
            uiController = FindAnyObjectByType<SensorInfoPanelController>();
            if (uiController == null)
            {
                Debug.LogError("在场景中找不到 SensorInfoPanelController 实例！");
            }
        }
    }

}
