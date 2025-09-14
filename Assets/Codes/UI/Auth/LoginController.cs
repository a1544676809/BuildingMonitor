using Assets.Codes.Entities;
using System.Collections;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{

    // ��Unity�༭������ק����ЩUI�ؼ�
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public Button loginButton;
    public Button registerButton;

    //UI�������
    public GameObject loginPanel;
    public GameObject registerPanel;

    private string apiBaseUrl = "http://localhost:8080/api/account";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ϊ��ť��ӵ���¼�������
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerButton.onClick.AddListener(OnRegisterButtonClicked);

        // ȷ����¼����ڿ�ʼʱ�ǿɼ��ģ���Ӧ����������ص�
        loginPanel.SetActive(true);
    }

    // ��¼��ť�ĵ���¼�������
    void OnLoginButtonClicked()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("�û��������벻��Ϊ��");
            return;
        }
        StartCoroutine(Login(username, password));
    }

    // ע�ᰴť�ĵ���¼�������
    void OnRegisterButtonClicked()
    {
        registerPanel.SetActive(true);
    }

    IEnumerator Login(string username, string password)
    {
        string loginUrl = apiBaseUrl + "/login";
        LoginRequest loginRequest = new LoginRequest(username, password);
        string json = JsonUtility.ToJson(loginRequest);

        using (UnityWebRequest webRequest = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("��¼�ɹ�: " + webRequest.downloadHandler.text);
                // ��¼�ɹ��󣬼���������
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");
                while (!asyncLoad.isDone)
                {
                    // ������Ը��¼��ؽ�����
                    // float progress = asyncLoad.progress;
                    // Debug.Log("���ؽ���: " + progress);
                    yield return null;
                }
            }
            else
            {
                Debug.LogError("��¼ʧ��: " + webRequest.error);
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
