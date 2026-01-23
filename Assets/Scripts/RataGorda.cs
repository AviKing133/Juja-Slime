using System.Collections;
using UnityEngine;

public class RataGorda : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadRodar = 12f;
    public float tiempoAturdimiento = 2f;

    [Header("Detección y Visión")]
    public float rangoVision = 6f;       // Distancia a la que te ve
    public string tagSuelo = "ground";
    public Transform detector;

    [Header("Combate")]
    public int vida = 5;
    public float empujeAlJugador = 15f;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Color colorOriginal;
    private bool mirandoDerecha = true;
    private bool esInvulnerable = false;
    private bool estaAturdida = false;

    private enum Estado { Patrulla, Rodando, Aturdida }
    private Estado estadoActual = Estado.Patrulla;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        colorOriginal = sprite.color; // Guardamos el color rojo/original

        if (detector == null) detector = transform.Find("GroundCheck");
    }

    void Update()
    {
        if (estaAturdida) return;

        ManejarMovimiento();

        // Solo buscamos al jugador si estamos patrullando
        if (estadoActual == Estado.Patrulla && !estaAturdida)
        {
            BuscarJugador();
        }

        DetectarObstaculos();
    }

    void ManejarMovimiento()
    {
        float velocidadActual = (estadoActual == Estado.Rodando) ? velocidadRodar : velocidadPatrulla;
        rb.linearVelocity = new Vector2(mirandoDerecha ? velocidadActual : -velocidadActual, rb.linearVelocity.y);
    }

    void BuscarJugador()
    {
        // 1. Calculamos la dirección
        Vector2 direccionMina = mirandoDerecha ? Vector2.right : Vector2.left;

        float offsetX = mirandoDerecha ? 0.8f : -0.8f;
        Vector3 puntoDeOrigen = transform.position + new Vector3(offsetX, -0.2f, 0);

        // 3. Lanzamos el rayo
        RaycastHit2D hit = Physics2D.Raycast(puntoDeOrigen, direccionMina, rangoVision);

        // Dibujamos el rayo en la ventana Scene para que veas exactamente de dónde sale
        Debug.DrawRay(puntoDeOrigen, direccionMina * rangoVision, Color.cyan);

        // 4. Filtramos por TAG
        if (hit.collider != null && hit.collider.CompareTag("player"))
        {
            EmpezarACargar();
        }
    }

    void EmpezarACargar()
    {
        // Solo empezamos si estamos patrullando (para no repetir el aviso si ya está cargando)
        if (estadoActual == Estado.Patrulla)
        {
            StartCoroutine(AvisoYEmbestida());
        }
    }

    void DetectarObstaculos()
    {
        RaycastHit2D hitSuelo = Physics2D.Raycast(detector.position, Vector2.down, 0.5f);
        Vector2 dirFrente = mirandoDerecha ? Vector2.right : Vector2.left;
        RaycastHit2D hitFrente = Physics2D.Raycast(detector.position, dirFrente, 0.4f);

        bool noHaySuelo = hitSuelo.collider == null || !hitSuelo.collider.CompareTag(tagSuelo);
        bool hayPared = hitFrente.collider != null && !hitFrente.collider.CompareTag("player");

        if (noHaySuelo || hayPared)
        {
            if (estadoActual == Estado.Rodando) EntrarEnAturdimiento();
            else Girar();
        }
    }

    void EntrarEnAturdimiento()
    {
        StartCoroutine(AturdimientoCo());
    }

    IEnumerator AturdimientoCo()
    {
        estadoActual = Estado.Aturdida;
        estaAturdida = true;
        esInvulnerable = false;
        rb.linearVelocity = Vector2.zero;

        StartCoroutine(EfectoParpadeo());

        yield return new WaitForSeconds(tiempoAturdimiento);

        estaAturdida = false;
        estadoActual = Estado.Patrulla;
        Girar();
    }
    IEnumerator AvisoYEmbestida()
    {
        // 1. Entramos en fase de aviso (se queda quieta)
        estadoActual = Estado.Aturdida;
        rb.linearVelocity = Vector2.zero;

        // 2. Parpadeo de aviso en ROJO durante 1 segundo
        float tiempoAviso = 1f;
        while (tiempoAviso > 0)
        {
            sprite.color = Color.red; // Se pone roja
            yield return new WaitForSeconds(0.1f);
            sprite.color = colorOriginal; // Vuelve a su color
            yield return new WaitForSeconds(0.1f);
            tiempoAviso -= 0.2f;
        }

        sprite.color = colorOriginal; // Nos aseguramos de restaurar el color
        estadoActual = Estado.Rodando;
        esInvulnerable = true;
    }
    IEnumerator EfectoParpadeo()
    {
        while (estaAturdida)
        {
            sprite.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, 0.4f);
            yield return new WaitForSeconds(0.1f);
            sprite.color = colorOriginal;
            yield return new WaitForSeconds(0.1f);
        }
        sprite.color = colorOriginal;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            PlayerMovement.instance.PerderVida();
            PlayerMovement.instance.RecibirGolpe();

            Rigidbody2D pRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (pRb != null)
            {
                float dir = (collision.transform.position.x > transform.position.x) ? 1 : -1;
                pRb.linearVelocity = Vector2.zero;
                pRb.AddForce(new Vector2(dir * empujeAlJugador, empujeAlJugador / 2f), ForceMode2D.Impulse);
            }

            if (estadoActual == Estado.Rodando) EntrarEnAturdimiento();
        }

        if (collision.gameObject.CompareTag("bullet") && !esInvulnerable)
        {
            vida--;
            if (vida <= 0) Destroy(gameObject);
        }
    }

    void Girar()
    {
        mirandoDerecha = !mirandoDerecha;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1);
    }

    // Dibujamos el rango de visión en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector2 dir = mirandoDerecha ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, dir * rangoVision);
    }
}