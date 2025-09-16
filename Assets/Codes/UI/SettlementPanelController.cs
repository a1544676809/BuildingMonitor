using UnityEngine;
using UnityEngine.UI;
public class SettlementPanelController : MonoBehaviour
{

    public Button closeButton;
    public Button dataViewButton;
    public Button exportButton;


    public DataVisualizationPanel dataVisualizationPanelController;
    public GameObject dataVisualizationPanel;
    public GameObject exportResultsPanel;

    void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    void OnDataViewButtonClicked()
    {
        dataVisualizationPanel.SetActive(true);
        // �����·��������������˵�Ϊ����������
        dataVisualizationPanelController.SetDropdownSelections("λ�ƴ�����", "����");
    }

    void OnExportResultClicked()
    {
        exportResultsPanel.SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        dataViewButton.onClick.AddListener(OnDataViewButtonClicked);
        exportButton.onClick.AddListener(OnExportResultClicked);
        dataVisualizationPanelController = dataVisualizationPanel.GetComponent<DataVisualizationPanel>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
