using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginRegister : MonoBehaviour
{
    public string username;
    public string password;

    public TMP_InputField usernameInputLogin;
    public TMP_InputField passwordInputLogin;
    public TMP_InputField usernameInputRegister;
    public TMP_InputField passwordInputRegister;

    public GameObject loading;
    public GameObject UserPassVacio;
    public GameObject TooShort;

    public void OnLoginButtonClicked()
    {
        username = usernameInputLogin.text;
        password = passwordInputLogin.text;

        GameManager.Instance.RecibirUsernamePassword(username, password);
        SceneManager.LoadScene("PantallaCarga");
    }
    public void OnRegisterButtonClicked()
    {
        if (String.IsNullOrWhiteSpace(usernameInputRegister.text) || String.IsNullOrWhiteSpace(passwordInputRegister.text))
        {
            StartCoroutine(MostrarYEsconder(UserPassVacio));
        }
        else if (passwordInputRegister.text.Length < 6)
        {
            StartCoroutine(MostrarYEsconder(TooShort));
        }
        else
        {
            username = usernameInputRegister.text;
            password = passwordInputRegister.text;
            loading.SetActive(true);
            StartCoroutine(GameManager.Instance.RegistrarUsuario(username, password));
        }
    }
    public void CloseButton()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator MostrarYEsconder(GameObject objeto)
    {
        // 1. Hacemos el objeto visible
        if (objeto != null)
        {
            objeto.SetActive(true);
        }

        // 2. Esperamos los segundos indicados
        yield return new WaitForSeconds(5f);

        // 3. Lo hacemos invisible
        if (objeto != null)
        {
            objeto.SetActive(false);
        }
    }
}
