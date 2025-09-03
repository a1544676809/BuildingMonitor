using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public InputField UsernameInput;
    public InputField PasswordInput;

    // ��ק��ֵ���� Unity �༭���н� Login �� GameObject �����ϵ���Щ�ֶ���
    public GameObject loginPanel;    // ��Ӧ Canvas �� Login ����
    public GameObject mainGamePanel; // ��Ӧ Canvas �� GameObject ����

    public void OnLoginButtonClick()
    {
        string username = UsernameInput.text;
        string password = PasswordInput.text;

        // ����֤�û��������루ʵ��Ӧ����Ӧʹ�ø���ȫ�ķ�����
        if (username == "111" && password == "222")
        {
            Debug.Log("��¼�ɹ�");

            // ���õ�ǰ Login ���
            loginPanel.SetActive(false);

            // ��������Ϸ���棨GameObject��
            mainGamePanel.SetActive(true);
        }
        else
        {
            Debug.Log("�û������������");
        }
    }
}