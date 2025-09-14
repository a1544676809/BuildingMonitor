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

    // 在Unity编辑器中拖拽绑定这些UI控件
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public Button loginButton;
    public Button registerButton;

    //UI面板引用
    public GameObject loginPanel;
    public GameObject registerPanel;

    private string apiBaseUrl = "http://localhost:8080/api/account";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 为按钮添加点击事件监听器
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerButton.onClick.AddListener(OnRegisterButtonClicked);

        // 确保登录面板在开始时是可见的，主应用面板是隐藏的
        loginPanel.SetActive(true);
    }

    // 登录按钮的点击事件处理方法
    void OnLoginButtonClicked()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("用户名或密码不能为空");
            return;
        }
        StartCoroutine(Login(username, password));
    }

    // 注册按钮的点击事件处理方法
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
                Debug.Log("登录成功: " + webRequest.downloadHandler.text);
                // 登录成功后，加载主场景
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");
                while (!asyncLoad.isDone)
                {
                    // 这里可以更新加载进度条
                    // float progress = asyncLoad.progress;
                    // Debug.Log("加载进度: " + progress);
                    yield return null;
                }
            }
            else
            {
                Debug.LogError("登录失败: " + webRequest.error);
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
