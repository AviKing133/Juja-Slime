using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Bullet : MonoBehaviour
{
    public float fuerzaHorizontal = 20f;
    public float fuerzaVertical = 10f;

    [Header("Ajustes de Rebote")]
    public int rebotesMaximos = 3;
    private int rebotesRestantes;

    public InterfaceBehaviour Interface;
    private Rigidbody2D rb;
    private Animator anim;
    private bool haTocadoSuelo = false;

    [HideInInspector] public PlayerMovement dueno;
    private Collider2D col;

    void Start()
    {
        Interface = Object.FindAnyObjectByType<InterfaceBehaviour>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rebotesRestantes = rebotesMaximos;
        col = GetComponent<Collider2D>();

        float direccion = dueno.mirandoDerecha ? 1f : -1f;
        Vector2 vectorDisparo = new Vector2(direccion * fuerzaHorizontal, fuerzaVertical);
        rb.AddForce(vectorDisparo, ForceMode2D.Impulse);
    }

    private void LateUpdate()
    {
        // Si el jugador original está lleno, las balas del suelo se vuelven triggers para no estorbar
        if (PlayerMovement.instance != null && PlayerMovement.instance.ammo >= 3 && gameObject.CompareTag("bullet"))
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
        PlayerMovement scriptTocado = collision.gameObject.GetComponent<PlayerMovement>();

        // Solo se recoge si ha tocado el suelo y el que la toca tiene espacio de munición
        if (collision.gameObject.CompareTag("player") && haTocadoSuelo)
        {
            if (scriptTocado.ammo < 3)
            {
                scriptTocado.ammo++;
                scriptTocado.ActualizarEscala();
                Interface.UpdateVidas(scriptTocado.ammo);
                Destroy(gameObject);
            }
        }

        if (collision.gameObject.CompareTag("ground") && !haTocadoSuelo)
        {
            rebotesRestantes--;
            if (rebotesRestantes <= 0) FrenarBala();
        }
    }

    void FrenarBala()
    {
        haTocadoSuelo = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Subir un poco para que no se entierre en el suelo
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);

        StartCoroutine(ElevarBala(0.1f, 1f));
        if (anim != null) anim.SetTrigger("tocaSuelo");
    }

    IEnumerator ElevarBala(float distancia, float tiempo)
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