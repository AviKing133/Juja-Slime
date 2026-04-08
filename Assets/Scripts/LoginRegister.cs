using UnityEngine;
using TMPro;

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

        StartCoroutine(GameManager.Instance.CargarJuego(username, password));
    }
    public void OnRegisterButtonClicked()
    {
        username = usernameInput.text;
        password = passwordInput.text;
        StartCoroutine(GameManager.Instance.RegistrarUsuario(username, password));
    }

    public void CloseButton()
    {
        this.gameObject.SetActive(false);
    }
}
