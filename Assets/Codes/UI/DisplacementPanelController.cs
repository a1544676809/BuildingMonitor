using UnityEngine;
using UnityEngine.UI;

public class DisplacementPanelController : MonoBehaviour
{

    public Button closeButton;
    public Button monitorPointSettingButton;
    public Button dataViewButton;
    public Button exportResultButton;

    public DataVisualizationPanel dataVisualizationPanelController;
    public GameObject dataVisualizationPanel;
    public GameObject exportResultsPanel;
    public Toggle editModeToggle;
    public GameObject noticePanel;
    public GameObject noticeText;
    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void OnMonitorPointSettingButtonClicked()
    {
        if (editModeToggle != null)
        {
            if(editModeToggle.isOn==false)
            {
                editModeToggle.isOn = true;
            }
        }
        else
        {
            Debug.LogError("切换失败！");
        }
    }
    void OnDataViewButtonClicked()
    {
        dataVisualizationPanel.SetActive(true);
        // 调用新方法，设置下拉菜单为“水平位移”
        dataVisualizationPanelController.SetDropdownSelections("位移传感器", "水平位移");
    }

    void OnExportResultClicked()
    {
        exportResultsPanel.SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        monitorPointSettingButton.onClick.AddListener(OnMonitorPointSettingButtonClicked);
        dataViewButton.onClick.AddListener(OnDataViewButtonClicked);
        exportResultButton.onClick.AddListener(OnExportResultClicked);
        dataVisualizationPanelController = dataVisualizationPanel.GetComponent<DataVisualizationPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
