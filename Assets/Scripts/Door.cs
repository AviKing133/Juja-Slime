using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public GameObject puertaCerrada;
    public bool isOpened = false;
    public int nivelADondeVa = 2;

    public void OnTriggerStay2D(Collider2D collision)
    {
        // IMPORTANTE: Verifica que tu personaje tenga el Tag "player" en el inspector
        if (collision.CompareTag("player") && isOpened)
        {
            if (GameManager.Instance == null)
            {
                GameManager.Instance = Object.FindFirstObjectByType<GameManager>();
            }

            if (GameManager.Instance != null)
            {
                isOpened = false;
                Debug.Log("<color=cyan>Puerta:</color> Guardando progreso hacia nivel " + nivelADondeVa);
                GameManager.Instance.GuardarProgreso(nivelADondeVa);
            }
        }
    }

    public void AbrirPuerta()
    {
        if (puertaCerrada != null)
            puertaCerrada.SetActive(false);

        isOpened = true;
        Debug.Log("Puerta abierta.");
    }
}