using UnityEngine;
using System.Collections;

public class PickupController : MonoBehaviour
{
    private Collider2D col;
    private Rigidbody2D rb;

    public InterfaceBehaviour Interface;

    [Header("Configuración de Rebote")]
    public int rebotesMaximos = 2;
    private int rebotesRestantes;
    private bool haFrenado = false;

    [Header("Datos de Contenido")]
    public int storedAmmo = 0;

    void Start()
    {
        Interface = Object.FindAnyObjectByType<InterfaceBehaviour>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        rebotesRestantes = rebotesMaximos;

        if (gameObject.CompareTag("pickupClone"))
        {
            rb.AddForce(new Vector2(Random.Range(-1f, 1f), 2f), ForceMode2D.Impulse);
        }
    }

    void LateUpdate()
    {
        // Evitar que el jugador recoja si ya está lleno de munición (solo para balas sueltas)
        if (PlayerMovement.instance.ammo >= 3 && gameObject.CompareTag("bullet"))
        {
            col.isTrigger = true;
        }
        else
        {
            col.isTrigger = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // --- LÓGICA DE REBOTE Y FRENADO ---
        if (collision.gameObject.CompareTag("ground") && !haFrenado)
        {
            rebotesRestantes--;
            if (rebotesRestantes <= 0)
            {
                FrenarPickup();
            }
        }

        // --- LÓGICA DE RECOLECCIÓN ---
        if (collision.gameObject.CompareTag("player") || collision.gameObject.CompareTag("clone"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player == null) return;

            // Caso A: Es una bala
            if (gameObject.CompareTag("bullet") && player.ammo < 3)
            {
                player.ammo++;
                player.ActualizarEscala(); // Recalcula escala total limpia
                Destroy(gameObject);
            }
            // Caso B: Es el clon regresando (solo se recoge si ya frenó en el suelo)
            else if (gameObject.CompareTag("pickupClone") && haFrenado)
            {
                player.cloneIsAvailable = true; // El original recupera su "masa de clon"
                player.ammo += storedAmmo;      // Recupera la munición que tenía el clon
                

                // Limitar la munición máxima por seguridad
                if (player.ammo > 3) player.ammo = 3;

                player.ActualizarEscala(); // Recalcula escala total limpia
                Destroy(gameObject);
            }
            Interface.UpdateVidas(player.ammo);
        }
    }

    void FrenarPickup()
    {
        haFrenado = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        StartCoroutine(ElevarEfecto(0.25f, 1f));
    }

    IEnumerator ElevarEfecto(float distancia, float tiempo)
    {
        Vector3 posicionInicial = transform.position;
        Vector3 posicionFinal = posicionInicial + new Vector3(0, distancia, 0);
        float tiempoTranscurrido = 0;

        while (tiempoTranscurrido < tiempo)
        {
            transform.position = Vector3.Lerp(posicionInicial, posicionFinal, tiempoTranscurrido / tiempo);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }
        transform.position = posicionFinal;
    }
}