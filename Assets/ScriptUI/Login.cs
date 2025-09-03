using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public InputField UsernameInput;
    public InputField PasswordInput;

    // 拖拽赋值：在 Unity 编辑器中将 Login 和 GameObject 对象拖到这些字段上
    public GameObject loginPanel;    // 对应 Canvas → Login 对象
    public GameObject mainGamePanel; // 对应 Canvas → GameObject 对象

    public void OnLoginButtonClick()
    {
        string username = UsernameInput.text;
        string password = PasswordInput.text;

        // 简单验证用户名和密码（实际应用中应使用更安全的方法）
        if (username == "111" && password == "222")
        {
            Debug.Log("登录成功");

            // 禁用当前 Login 面板
            loginPanel.SetActive(false);

            // 启用主游戏界面（GameObject）
            mainGamePanel.SetActive(true);
        }
        else
        {
            Debug.Log("用户名或密码错误");
        }
    }
}