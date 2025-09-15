using UnityEngine;
using UnityEngine.EventSystems;

public class SensorGizmoClick : MonoBehaviour, IPointerClickHandler
{

    public DeformableModelController controller;
    public int sensorId;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller != null)
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
