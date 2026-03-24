using UnityEngine;

public class Threats : MonoBehaviour
{
    public BossController boss;
    public float fuerzaEmpuje = 10f;
    public Vector2 direccionDiagonalzquierda = new Vector2(-1f, 1f);
    public Vector2 direccionDiagonalDerecha = new Vector2(1, -1f);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            PlayerMovement.instance.PerderVida();
            PlayerMovement.instance.RecibirGolpe();

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                if (PlayerMovement.instance.mirandoDerecha)
                    playerRb.AddForce(direccionDiagonalzquierda.normalized * fuerzaEmpuje, ForceMode2D.Impulse);
                else
                    playerRb.AddForce(direccionDiagonalDerecha.normalized * fuerzaEmpuje, ForceMode2D.Impulse);
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            PlayerMovement.instance.PerderVida();
            PlayerMovement.instance.RecibirGolpe();
            Rigidbody2D player = collision.gameObject.GetComponent<Rigidbody2D>();
            if (player != null)
            {
                player.linearVelocity = Vector2.zero;
                if (PlayerMovement.instance.mirandoDerecha)
                    player.AddForce(direccionDiagonalzquierda.normalized * fuerzaEmpuje, ForceMode2D.Impulse);
                else
                    player.AddForce(direccionDiagonalDerecha.normalized * fuerzaEmpuje, ForceMode2D.Impulse);
            }
        }
        if (collision.gameObject.CompareTag("walls"))
        {
            if (boss != null)
            {
                boss.Turn();
            }
        }
    }
}
