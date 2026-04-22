using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginRegister : MonoBehaviour
{
    public string username;
    public string password;

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public void OnLoginButtonClicked()
    {
        username = usernameInput.text;
        password = passwordInput.text;

        GameManager.Instance.RecibirUsernamePassword(username, password);
        SceneManager.LoadScene("PantallaCarga");
    }
    public void OnRegisterButtonClicked()
    {
        username = usernameInput.text;
        password = passwordInput.text;
        GameManager.Instance.RegistrarUsuario(username, password);
    }

    public void CloseButton()
    {
        this.gameObject.SetActive(false);
    }
}
