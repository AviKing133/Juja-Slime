using System.Collections;
using UnityEngine;

public class Bala : MonoBehaviour
{
    public float fuerzaHorizontal = 20f;
    public float fuerzaVertical = 10f;
    public float tiempoVida = 10f;

    [Header("Ajustes de Rebote")]
    public int rebotesMaximos = 3;
    private int rebotesRestantes;

    private Rigidbody2D rb;
    private Animator anim;
    private bool haTocadoSuelo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rebotesRestantes = rebotesMaximos;

        float direccion = PlayerMovement.instance.mirandoDerecha ? 1f : -1f;
        Vector2 vectorDisparo = new Vector2(direccion * fuerzaHorizontal, fuerzaVertical);

        rb.AddForce(vectorDisparo, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("player") || collision.gameObject.CompareTag("clone"))
        {
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("ground") && !haTocadoSuelo)
        {
            rebotesRestantes--;

            if (rebotesRestantes <= 0)
            {
                FrenarBala();
            }
            else
            {
                Debug.Log("Rebote número: " + (rebotesMaximos - rebotesRestantes));
            }
        }
    }

    void FrenarBala()
    {
        haTocadoSuelo = true;
        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        StartCoroutine(ElevarBala(0.1f, 1f));

        if (anim != null)
        {
            anim.SetTrigger("tocaSuelo");
        }
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