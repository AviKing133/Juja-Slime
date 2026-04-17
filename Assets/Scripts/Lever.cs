using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public List<PlomosBehaviour> objetos;
    public Door AttachedDoor;
    public GameObject PalancaAccionada;
    public GameObject PalancaOFF;

    // Cambios de terreno
    public GameObject terrenoMostrar;
    public GameObject terrenoOcultar;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("player") && Input.GetKeyDown(KeyCode.E))
        {
            AccionarPalanca();
            if (AttachedDoor != null)
            {
                AttachedDoor.AbrirPuerta();
            }
            else
            {
                foreach (PlomosBehaviour plomo in objetos)
                {
                    plomo.ResetPlomo();
                }
            }
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("bullet"))
        {
            AccionarPalanca();
            if (AttachedDoor != null)
            {
                AttachedDoor.AbrirPuerta();
            }
            else
            {
                foreach (PlomosBehaviour plomo in objetos)
                {
                    plomo.ResetPlomo();
                }
            }
        }
    }

    private void AccionarPalanca()
    {
        terrenoMostrar.SetActive(true);
        terrenoOcultar.SetActive(false);
        PalancaAccionada.SetActive(true);
        PalancaOFF.SetActive(false);
    }
}
