using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static Enemigo1Controller;

public class HitBoxBoss : MonoBehaviour
{
    public float fuerzaEmpuje = 12f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("walls") || collision.gameObject.CompareTag("ground"))
        {
            GetComponentInParent<Boss1>().Turn();
        }
        if (collision.gameObject.CompareTag("bullet"))
        {
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("pickupClone"))
        {
            Destroy(collision.gameObject);
        }        
        if (collision.gameObject.CompareTag("enemy"))
        {
            Destroy(collision.gameObject);
        }
    }

    IEnumerator SecuenciaAtaque(GameObject playerObj)
    {
        PlayerController player = playerObj.GetComponent<PlayerController>();
        Rigidbody2D rbPlayer = playerObj.GetComponent<Rigidbody2D>();

        if (player != null && rbPlayer != null)
        {
            float direccionX = player.mirandoDerecha ? -1f : 1f;
            Vector2 fuerzaVector = new Vector2(direccionX, 0.5f).normalized * fuerzaEmpuje;
            rbPlayer.linearVelocity = Vector2.zero;
            rbPlayer.AddForce(fuerzaVector, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(0.2f); // Tiempo que dura el ataque
    }
}
