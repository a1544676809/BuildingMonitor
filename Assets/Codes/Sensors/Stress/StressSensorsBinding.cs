using Assets.Codes.Entities;
using UnityEngine;

public class StressSensorsBinding : MonoBehaviour
{
    public int sensorId;
    public StressSensorData latestData;

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
}
