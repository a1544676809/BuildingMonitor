using Assets.Codes.Entities;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.Codes.UI.Auth
{
    public class RegisterController: MonoBehaviour
    {
        public TMP_InputField usernameInputField;
        public TMP_InputField passwordInputField;
        public TMP_InputField confirmPasswordInputField;
        public TMP_InputField LicenseInputField;
        public Button closeButton;
        public Button registerButton;

        public GameObject loginPanel;
        public GameObject registerPanel;

        private string apiBaseUrl = "http://localhost:8080/api/account";

        void Start()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            registerButton.onClick.AddListener(OnRegisterButtonClicked);
            registerPanel.SetActive(false);
        }

        void OnCloseButtonClicked()
        {
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
        void OnRegisterButtonClicked() 
        {
            string username = usernameInputField.text;
            string password = passwordInputField.text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("用户名或密码不能为空");
                return;
            }
            if(password != confirmPasswordInputField.text)
            {
                Debug.LogError("两次输入的密码不一致");
                return;
            }
            StartCoroutine(Register(username, password));
        }
        IEnumerator Register(string username, string password)
        {
            string registerUrl = apiBaseUrl + "/register";
            RegistrationRequest registerRequest = new RegistrationRequest(username, password);
            string json = JsonUtility.ToJson(registerRequest);

            using (UnityWebRequest webRequest = new UnityWebRequest(registerUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("注册成功: " + webRequest.downloadHandler.text);
                    // 注册成功后，可以选择自动登录或提示用户登录
                }
                else
                {
                    Debug.LogError("注册失败: " + webRequest.error);
                }
            }
        }
    }
}
