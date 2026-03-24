using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public List<PlomosBehaviour> objetos;
    public Door AttachedDoor;
    public GameObject PalancaAccionada;
    public GameObject PalancaOFF;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("player") && Input.GetKeyDown(KeyCode.E))
        {
            AccionarPalanca();
            if (AttachedDoor != null)
            {
                AttachedDoor.AbrirPuerta();
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
        PalancaAccionada.SetActive(true);
        PalancaOFF.SetActive(false);
    }
}
