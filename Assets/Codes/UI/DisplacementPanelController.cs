using UnityEngine;
using UnityEngine.UI;

public class DisplacementPanelController : MonoBehaviour
{

    public Button closeButton;

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
