using Assets.Codes.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorInfoPanelController : MonoBehaviour
{

    // UI 组件的引用，在Inspector中手动绑定
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI latestInfoText;
    public TextMeshProUGUI statusText;

    public Toggle editModeToggle;

    // 一个公共方法，用于接收传感器数据并更新UI
    public void ShowPanel(int sensorId, DisplacementSensorData data, Vector3 unityBaseLine)
    {
        // 确保面板是可见的
        gameObject.SetActive(true);

        Vector3 position = new Vector3(
            (float)(unityBaseLine.x + data.horizontalDisplacementX),
            (float)(unityBaseLine.y + data.horizontalDisplacementY),
            (float)(unityBaseLine.z + data.settlement)
            );

        // 更新文本内容
        nameText.text = "传感器 ID: " + sensorId;
        if(typeText != null)
            typeText.text = "类型: 位移传感器";
        string pos = $"X: {position.x:F2} Y: {position.y:F2} Z: {position.z:F2}";
        latestInfoText.text = pos;
        statusText.text = data.status.ToString();

        // 可选：将面板位置移动到屏幕中心或特定位置
        // transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }


    public void ShowPanel(int sensorId, StressSensorData data, Vector3 unityBaseLine)
    {
        // 确保面板是可见的
        gameObject.SetActive(true);

        Vector3 position = new Vector3(
            (float)(unityBaseLine.x),
            (float)(unityBaseLine.y),
            (float)(unityBaseLine.z)
            );

        // 更新文本内容
        nameText.text = "传感器 ID: " + sensorId;
        if (typeText != null)
            typeText.text = "类型: 应力传感器";
        string pos = $"应力量: {data.stressValue}";
        latestInfoText.text = pos;
        statusText.text = data.status.ToString();

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
