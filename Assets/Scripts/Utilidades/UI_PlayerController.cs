using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_PlayerController : MonoBehaviour
{
    public CanvasGroup grupoGameOver;
    private void Start()
    {
        GameManager.Instance.grupoGameOver = grupoGameOver;
    }

    public void RetryButton()
    {
        SceneManager.LoadScene("PantallaCarga");
    }
}
