using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject registerPanel;

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
    }
    public void ShowRegisterPanel()
    {
        registerPanel.SetActive(true);
    }
}
