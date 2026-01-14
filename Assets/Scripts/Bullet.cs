using UnityEngine;

public class Bala : MonoBehaviour
{
    public float fuerza = 20f;
    public float tiempoVida = 3f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        float direccion = PlayerMovement.instance.mirandoDerecha ? 1f : -1f;
        rb.AddForce(new Vector2(direccion * fuerza, 0), ForceMode2D.Impulse);
        rb.AddForce(new Vector2(transform.up.y * fuerza, 0), ForceMode2D.Impulse);
        Destroy(gameObject, tiempoVida);
    }

}