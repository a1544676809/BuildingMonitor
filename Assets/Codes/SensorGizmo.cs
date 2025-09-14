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
        // ��������Ļ����ת��Ϊ��������
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(eventData.position);

        // ȷ�� Gizmo ������ģ�ͱ��棬����ֻ�Ǽ򵥵�����
        newPosition.z = transform.position.z;

        // ���� Gizmo ��λ��
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
