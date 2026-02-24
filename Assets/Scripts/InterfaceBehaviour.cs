using Unity.VisualScripting;
using UnityEngine;

public class InterfaceBehaviour : MonoBehaviour
{
    public GameObject vidas1;
    public GameObject vidas2;
    public GameObject vidas3;
    void Start()
    {
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
