using Assets.Codes.Entities;
using UnityEngine;

public class StressSensorsBinding : MonoBehaviour
{
    public int sensorId;
    public StressSensorData latestData;

    // �����UI������������
    public SensorInfoPanelController uiController;
    // ���������ڴ洢��������Unity��׼��
    public Vector3 unityBaseline;


    void OnMouseDown()
    {
        Debug.Log("�������������!");
        if (uiController != null)
        {
            // ���UI�Ѿ��ɼ�����������
            if (uiController.gameObject.activeSelf)
            {
                uiController.HidePanel();
            }
            else
            {
                if (uiController.editModeToggle.isOn == false)
                {
                    // ����UI�������ķ���������ǰ�����������ݴ��ݸ���
                    uiController.ShowPanel(sensorId, latestData, unityBaseline);
                }
            }
        }
    }


    public void UpdateData(StressSensorData newData)
    {
        this.latestData = newData;

        // ����Ӧ��ֵ���ı�Ԥ�Ƽ�����ɫ
        // ���赱Ӧ��ֵ���� 100 MPa ʱ����ɫ��Ϊ��ɫ
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
        // �Զ����Ҳ���� uiController ����
        if (uiController == null)
        {
            uiController = FindAnyObjectByType<SensorInfoPanelController>();
            if (uiController == null)
            {
                Debug.LogError("�ڳ������Ҳ��� SensorInfoPanelController ʵ����");
            }
        }
    }

}
