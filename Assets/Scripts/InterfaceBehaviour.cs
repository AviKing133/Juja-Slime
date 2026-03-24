using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InterfaceBehaviour : MonoBehaviour
{
    public GameObject vidas1;
    public GameObject vidas2;
    public GameObject vidas3;

    private bool isPaused = false;
    void Start()
    {
    }

    public void PauseMenu()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
        else
        {
            Time.timeScale = 0f;
            isPaused = true;
        }
    }
    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void UpdateVidas(int vida)
    {
        if (vida > 0)
        {
            vidas1.SetActive(true);
        }
        else
        {
            vidas1.SetActive(false);
        }
        
        if (vida > 1)
        {
            vidas2.SetActive(true);
        }
        else
        {
            vidas2.SetActive(false);
        }

        if (vida > 2)
        {
            vidas3.SetActive(true);
        }
        else
        {
            vidas3.SetActive(false);
        }
    }
}
