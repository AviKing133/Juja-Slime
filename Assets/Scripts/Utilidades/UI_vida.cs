using System.Collections.Generic;
using UnityEngine;

public class UI_vida : MonoBehaviour
{
    public int vidasIniciales = 3;
    public List<GameObject> Vidas;
    public GameObject Clone;
    public int vidaMaxima = 5;

    private void Start()
    {
        UpdateVidas(vidasIniciales);
    }

    public void UpdateVidas(int vidaActual)
    {
        int indiceParaActivar = vidaMaxima - vidaActual;

        if (indiceParaActivar < 0) indiceParaActivar = 0;
        if (indiceParaActivar >= Vidas.Count) indiceParaActivar = Vidas.Count - 1;

        for (int i = 0; i < Vidas.Count; i++)
        {
            Vidas[i].SetActive(i == indiceParaActivar);
        }
    }
    public void CloneAvailable()
    {
        Clone.SetActive(true);
    }
    public void CloneUnavailable()
    {
        Clone.SetActive(false);
    }
}