using UnityEngine;
using System.Collections;

public class TriggerDialogo : MonoBehaviour
{
    public GameObject ShortCut;

    [Header("Configuración de Sonido")]
    public AudioSource audioSource;
    public AudioClip terremoto;

    private bool activado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Evitamos que se active varias veces si el player entra y sale rápido
        if (collision.CompareTag("player") && !activado)
        {
            activado = true;
            StartCoroutine(SecuenciaTerremoto());
        }
    }

    IEnumerator SecuenciaTerremoto()
    {
        // 1. Desactivar el ShortCut
        if (ShortCut != null) ShortCut.SetActive(false);

        // 2. Ejecutar el efecto de cámara
        var cam = Object.FindFirstObjectByType<CameraController>();
        if (cam != null)
        {
            cam.AplicarEfectoImpacto(1.5f, 3f, 0.4f);
        }

        // 3. Reproducir el sonido
        if (audioSource != null && terremoto != null)
        {
            audioSource.PlayOneShot(terremoto);

            // 4. ESPERAR a que el sonido termine (o un tiempo razonable)
            // Usamos la duración del clip para que sea exacto
            yield return new WaitForSeconds(terremoto.length);
        }
        else
        {
            // Si no hay audio, esperamos un frame para evitar errores
            yield return null;
        }

        // 5. Ahora que todo terminó, destruimos el objeto
        Destroy(gameObject);
    }
}