using UnityEngine;

public class PlomosBehaviour : MonoBehaviour
{
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.CompareTag("player"))
    //    {
    //        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
    //        if (!player.haveDamage)
    //        {
    //            player.haveDamage = true;
    //            this.gameObject.SetActive(false);
    //        }
    //    }
    //}

    public void ResetPlomo()
    {
        this.gameObject.SetActive(true);
    }
}
