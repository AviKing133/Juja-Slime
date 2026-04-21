using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    public AudioClip impactSound;
    public AudioSource audioSource;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        audioSource.PlayOneShot(impactSound);
        if (collision.gameObject.CompareTag("enemy"))
        {
            Enemigo1Controller enemigo = collision.gameObject.GetComponent<Enemigo1Controller>();
            if (enemigo != null)
            {
                enemigo.RecibirDaño();
            }
        }

        else if (collision.gameObject.CompareTag("player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.ammo < 3)
            {
                player.SumarAmmo();
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.ammo < 3)
            {
                player.SumarAmmo();
                Destroy(gameObject);
            }
        }
    }
}
