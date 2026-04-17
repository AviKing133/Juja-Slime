using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public List<PlomosBehaviour> objetos;
    public Door AttachedDoor;
    public GameObject PalancaAccionada;
    public GameObject PalancaOFF;

    [Header("Efecto de impacto")]
    public AudioSource audioSource;
    public AudioClip terremoto;

    [Header("Cambios de terreno (Listas)")]
    public List<GameObject> terrenosAMostrar;
    public List<GameObject> terrenosAOcultar;

    private bool jugadorEnRango = false;
    private bool yaAccionada = false;

    void Update()
    {
        if (jugadorEnRango && !yaAccionada && Input.GetKeyDown(KeyCode.E))
        {
            EjecutarLogicaPalanca();
        }
    }

    private void EjecutarLogicaPalanca()
    {
        yaAccionada = true;
        AccionarPalanca();

        if (AttachedDoor != null)
        {
            AttachedDoor.AbrirPuerta();
        }
        else
        {
            foreach (PlomosBehaviour plomo in objetos)
            {
                if (plomo != null) plomo.ResetPlomo();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player")) jugadorEnRango = true;

        if (collision.CompareTag("bullet") && !yaAccionada)
        {
            EjecutarLogicaPalanca();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("player")) jugadorEnRango = false;
    }

    private void AccionarPalanca()
    {
        if (audioSource != null && terremoto != null)
        {
            audioSource.PlayOneShot(terremoto);
        }
        GestionarListasTerreno();

        if (PalancaAccionada != null) PalancaAccionada.SetActive(true);
        if (PalancaOFF != null) PalancaOFF.SetActive(false);

        var cam = Object.FindFirstObjectByType<CameraController>();
        if (cam != null)
        {
            cam.AplicarEfectoImpacto(1.5f, 3f, 0.4f);
        }
    }

    // MÉTODO PARA PROCESAR LAS LISTAS
    private void GestionarListasTerreno()
    {
        // Activamos todos los de la lista de mostrar
        foreach (GameObject obj in terrenosAMostrar)
        {
            if (obj != null) obj.SetActive(true);
        }

        // Desactivamos todos los de la lista de ocultar
        foreach (GameObject obj in terrenosAOcultar)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
}