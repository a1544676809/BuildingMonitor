using Assets.Codes.Entities;
using TMPro;
using UnityEngine;

public class SensorInfoPanelController : MonoBehaviour
{

    // UI ��������ã���Inspector���ֶ���
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI latestInfoText;
    public TextMeshProUGUI statusText;


    // һ���������������ڽ��մ��������ݲ�����UI
    public void ShowPanel(int sensorId, DisplacementSensorData data)
    {
        // ȷ������ǿɼ���
        gameObject.SetActive(true);

        // �����ı�����
        nameText.text = "������ ID: " + sensorId;
        if(typeText != null)
            typeText.text = "����: λ�ƴ�����";
        string pos = $"X: {data.currentX:F2}\nY: {data.currentY:F2}\nZ: {data.currentZ:F2}";
        latestInfoText.text = pos;

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
