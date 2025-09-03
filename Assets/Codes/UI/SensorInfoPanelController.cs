using Assets.Codes.Entities;
using TMPro;
using UnityEngine;

public class SensorInfoPanelController : MonoBehaviour
{

    // UI 组件的引用，在Inspector中手动绑定
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI latestInfoText;
    public TextMeshProUGUI statusText;


    // 一个公共方法，用于接收传感器数据并更新UI
    public void ShowPanel(int sensorId, DisplacementSensorData data)
    {
        // 确保面板是可见的
        gameObject.SetActive(true);

        // 更新文本内容
        nameText.text = "传感器 ID: " + sensorId;
        if(typeText != null)
            typeText.text = "类型: 位移传感器";
        string pos = $"X: {data.currentX:F2}\nY: {data.currentY:F2}\nZ: {data.currentZ:F2}";
        latestInfoText.text = pos;

        // 可选：将面板位置移动到屏幕中心或特定位置
        // transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }

    // 隐藏面板的方法
    public void HidePanel()
    {
        gameObject.SetActive(false);
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
