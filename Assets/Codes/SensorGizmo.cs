using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

public class SensorGizmo : MonoBehaviour, IPointerClickHandler
{
    private int sensorId;
    private DeformableModelController controller;
    public void SetSensorId(int id)
    {
        sensorId = id;
    }

    // 仅保留点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            controller.OpenEditPanel(sensorId);
        }
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
