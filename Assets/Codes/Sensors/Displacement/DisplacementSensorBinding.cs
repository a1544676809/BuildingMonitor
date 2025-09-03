using Assets.Codes.Entities;
using UnityEngine;

public class DisplacementSensorsBinding : MonoBehaviour
{
    // 这个字段用于存储与该预制件绑定的传感器ID
    public int sensorId;

    // 这个字段用于存储传感器最近的数据
    public DisplacementSensorData latestData;

    // 缓存对UI控制器的引用
    public SensorInfoPanelController uiController;

    void OnMouseDown()
    {
        if (uiController != null)
        {
            // 如果UI已经可见，则隐藏它
            if (uiController.gameObject.activeSelf)
            {
                uiController.HidePanel();
            }
            else
            {
                // 调用UI控制器的方法，将当前传感器的数据传递给它
                uiController.ShowPanel(sensorId, latestData);
            }
        }
    }

    // 一个公共方法，用于从后端数据中更新预制件的状态
    public void UpdateData(DisplacementSensorData newData)
    {
        this.latestData = newData;

        // 根据最新数据更新对象的位置
        Vector3 newPosition = new Vector3(
            (float)newData.currentX,
            (float)newData.currentY,
            (float)newData.currentZ
        );
        transform.position = newPosition;

        // 如果有子对象需要更新，可以在这里进行
        Renderer renderer = transform.Find("DisplacementSensorModel")?.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 可以在这里根据数据值来改变材质颜色、显示UI文本等
            // 例如：如果总位移量超过某个阈值，将预制件颜色变为红色
            if (newData.totalDisplacement > 0.5)
            {
                renderer.material.color = Color.red;
            }
            else
            {
                renderer.material.color = Color.green;
            }
        }
    }
}