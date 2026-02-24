using UnityEngine;

public class JujerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 3f;
    public bool mirandoDerecha = true;
    public int vida = 3;

    [Header("Detección")]
    public float distanciaAbajo = 0.5f;
    public float distanciaFrente = 0.2f;
    public string tagSuelo = "ground";

    [Header("Configuración de Ataque")]
    public float empujeHorizontal = 8f;
    public float empujeVertical = 10f;

    private Rigidbody2D rb;
    private Transform detector;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        detector = transform.Find("GroundCheck");

        if (detector == null) Debug.LogError("No se encontró el objeto hijo GroundCheck en " + gameObject.name);
    }

    void Update()
    {
        // Movimiento horizontal constante
        rb.linearVelocity = new Vector2(mirandoDerecha ? velocidad : -velocidad, rb.linearVelocity.y);

        // Rayos de detección
        RaycastHit2D hitSuelo = Physics2D.Raycast(detector.position, Vector2.down, distanciaAbajo);
        Vector2 direccionFrente = mirandoDerecha ? Vector2.right : Vector2.left;
        RaycastHit2D hitFrente = Physics2D.Raycast(detector.position, direccionFrente, distanciaFrente);

        // Lógica de giro: Gira si no hay suelo O si hay una pared (que no sea el jugador)
        bool noHaySuelo = hitSuelo.collider == null || !hitSuelo.collider.CompareTag(tagSuelo);
        bool hayPared = hitFrente.collider != null && !hitFrente.collider.CompareTag("player");

        if (noHaySuelo || hayPared)
        {
            Girar();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ATAQUE AL JUGADOR
        if (collision.gameObject.CompareTag("player"))
        {
            // 1. Llamamos a los métodos del script que te pasé antes
            PlayerMovement.instance.PerderVida();
            PlayerMovement.instance.RecibirGolpe(); // Activa el aturdimiento

            // 2. Aplicamos el empuje físico
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Reset de velocidad para que el impacto sea seco
                playerRb.linearVelocity = Vector2.zero;

                // Calculamos dirección (lado opuesto a la rata)
                float dirX = (collision.transform.position.x > transform.position.x) ? 1f : -1f;

                // Aplicamos fuerza de impulso
                playerRb.AddForce(new Vector2(dirX * empujeHorizontal, empujeVertical), ForceMode2D.Impulse);
            }
        }

        // DAÑO POR BALA
        if (collision.gameObject.CompareTag("bullet"))
        {
            vida--;
            if (vida <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    void Girar()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    private void OnDrawGizmos()
    {
        if (detector != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(detector.position, Vector2.down * distanciaAbajo);

            Gizmos.color = Color.red;
            Vector2 dir = mirandoDerecha ? Vector2.right : Vector2.left;
            Gizmos.DrawRay(detector.position, dir * distanciaFrente);
        }
    }
}