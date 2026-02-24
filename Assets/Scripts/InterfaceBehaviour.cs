using Unity.VisualScripting;
using UnityEngine;

public class InterfaceBehaviour : MonoBehaviour
{
    GameObject[] vidas;
    void Start()
    {
        vidas = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            vidas[i] = transform.GetChild(i).gameObject;
        }
    }

    public void UpdateVidas(int vida)
    {
        if (vida == 1)
        {
            vidas[0].gameObject.SetActive(true);
            vidas[2].gameObject.SetActive(false);
            vidas[2].gameObject.SetActive(false);
        }
        else if (vida == 2)
        {
            vidas[0].gameObject.SetActive(true);
            vidas[1].gameObject.SetActive(true);
            vidas[2].gameObject.SetActive(false);  
        }
        else if (vida == 3)
        {
            vidas[0].gameObject.SetActive(true);
            vidas[1].gameObject.SetActive(true);
            vidas[2].gameObject.SetActive(true);
        }
    }
}
