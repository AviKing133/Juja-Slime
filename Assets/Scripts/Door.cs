using UnityEditor.SearchService;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject puertaCerrada;
    public bool isOpened = false;

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("player") && isOpened)
        {
            // Cambio de nivel
        }
    }
    public void AbrirPuerta()
    {
        puertaCerrada.SetActive(false);
    }
}
