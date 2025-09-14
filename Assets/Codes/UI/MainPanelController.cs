using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Button sensorManageButton;
    public Button stressButton;
    public Button displacementButton;
    public Button subsidenceButton;
    public Button inclinationButton;
    public Toggle editModeToggle;

    public GameObject sensorPanel;
    public GameObject stressPanel;
    public GameObject displacementPanel;
    public GameObject subsidencePanel;
    public GameObject inclinationPanel;
    public GameObject SensorEditPanel;
    public DeformableModelController modelController;

    void OnSensorManageButtonClicked()
    {
        if (sensorPanel != null)
        {
            sensorPanel.SetActive(true);
        }
    }

    void OnStressButtonClicked()
    {
        if (stressPanel != null)
        {
            stressPanel.SetActive(true);
        }
    }

    void OnDisplacementButtonClicked()
    {
        if (displacementPanel != null)
        {
            displacementPanel.SetActive(true);
        }
    }

    void OnSubsidenceButtonClicked()
    {
        if (subsidencePanel != null)
        {
            subsidencePanel.SetActive(true);
        }
    }

    void OnInclinationButtonClicked()
    {
        if (inclinationPanel != null)
        {
            inclinationPanel.SetActive(true);
        }
    }

    void OnEditModeToggleChanged(Toggle toggle)
    {
        modelController.ToggleEditMode(toggle.isOn);
        Debug.Log("Edit Mode: " + toggle.isOn);
    }

    void Start()
    {
        sensorManageButton.onClick.AddListener(OnSensorManageButtonClicked);
        stressButton.onClick.AddListener(OnStressButtonClicked);
        displacementButton.onClick.AddListener(OnDisplacementButtonClicked);
        subsidenceButton.onClick.AddListener(OnSubsidenceButtonClicked);
        inclinationButton.onClick.AddListener(OnInclinationButtonClicked);
        editModeToggle.onValueChanged.AddListener(delegate { OnEditModeToggleChanged(editModeToggle); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
