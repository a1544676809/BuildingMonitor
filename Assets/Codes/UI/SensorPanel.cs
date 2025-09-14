using UnityEngine;
using UnityEngine.UI;

public class SensorPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Button closeButton;

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
