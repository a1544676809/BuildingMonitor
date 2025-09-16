using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InclinationPanelController : MonoBehaviour
{

    public Button closeButton;
    public Button computeButton;
    public Button dataViewButton;
    public Button exportResultButton;

    public DataVisualizationPanel dataVisualizationPanelController;
    public GameObject dataVisualizationPanel;
    public GameObject exportResultsPanel;
    public GameObject noticePanel;
    public TextMeshProUGUI noticeText;

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void OnComputeButtonClicked()
    {
        noticeText.text = ("后端已经计算完成！");
        noticePanel.SetActive(true);
    }

    void OnDataViewButtonClicked()
    {
        dataVisualizationPanel.SetActive(true);

        // 调用新方法，设置下拉菜单为“总挠度”
        dataVisualizationPanelController.SetDropdownSelections("位移传感器", "总挠度");
    }

    void OnExportResultClicked()
    {
        exportResultsPanel.SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        computeButton.onClick.AddListener(OnComputeButtonClicked);
        dataViewButton.onClick.AddListener(OnDataViewButtonClicked);
        exportResultButton.onClick.AddListener(OnExportResultClicked);
        dataVisualizationPanelController = dataVisualizationPanel.GetComponent<DataVisualizationPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
