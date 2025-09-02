using Assets.Codes.Entities;
using UnityEngine;

public class StressSensorsBinding : MonoBehaviour
{
    public int sensorId;
    public StressSensorData latestData;

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
}
