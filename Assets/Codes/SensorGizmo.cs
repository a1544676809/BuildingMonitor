using UnityEngine;
using UnityEngine.EventSystems;

public class SensorGizmo : MonoBehaviour, IDragHandler
{
    private int sensorId;
    public void SetSensorId(int id)
    {
        sensorId = id;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 将鼠标的屏幕坐标转换为世界坐标
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(eventData.position);

        // 确保 Gizmo 保持在模型表面，这里只是简单的例子
        newPosition.z = transform.position.z;

        // 更新 Gizmo 的位置
        transform.position = newPosition;
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
