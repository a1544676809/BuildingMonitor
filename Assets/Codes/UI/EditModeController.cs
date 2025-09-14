using UnityEngine;
using UnityEngine.UI;

public class EditModeController : MonoBehaviour
{

    public Button createSensorButton;

    void OnCreateSensorButton()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createSensorButton.onClick.AddListener(OnCreateSensorButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
