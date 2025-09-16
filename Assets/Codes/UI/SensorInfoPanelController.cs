using Assets.Codes.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorInfoPanelController : MonoBehaviour
{

    // UI ��������ã���Inspector���ֶ���
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI latestInfoText;
    public TextMeshProUGUI statusText;

    public Toggle editModeToggle;

    // һ���������������ڽ��մ��������ݲ�����UI
    public void ShowPanel(int sensorId, DisplacementSensorData data, Vector3 unityBaseLine)
    {
        // ȷ������ǿɼ���
        gameObject.SetActive(true);

        Vector3 position = new Vector3(
            (float)(unityBaseLine.x + data.horizontalDisplacementX),
            (float)(unityBaseLine.y + data.horizontalDisplacementY),
            (float)(unityBaseLine.z + data.settlement)
            );

        // �����ı�����
        nameText.text = "������ ID: " + sensorId;
        if(typeText != null)
            typeText.text = "����: λ�ƴ�����";
        string pos = $"X: {position.x:F2} Y: {position.y:F2} Z: {position.z:F2}";
        latestInfoText.text = pos;
        statusText.text = data.status.ToString();

        // ��ѡ�������λ���ƶ�����Ļ���Ļ��ض�λ��
        // transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }


    public void ShowPanel(int sensorId, StressSensorData data, Vector3 unityBaseLine)
    {
        // ȷ������ǿɼ���
        gameObject.SetActive(true);

        Vector3 position = new Vector3(
            (float)(unityBaseLine.x),
            (float)(unityBaseLine.y),
            (float)(unityBaseLine.z)
            );

        // �����ı�����
        nameText.text = "������ ID: " + sensorId;
        if (typeText != null)
            typeText.text = "����: Ӧ��������";
        string pos = $"Ӧ����: {data.stressValue}";
        latestInfoText.text = pos;
        statusText.text = data.status.ToString();

        // ��ѡ�������λ���ƶ�����Ļ���Ļ��ض�λ��
        // transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }


    // �������ķ���
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
