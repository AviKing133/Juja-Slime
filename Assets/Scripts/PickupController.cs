using UnityEngine;
using System.Collections;

public class PickupController : MonoBehaviour
{
    private Collider2D col;
    private Rigidbody2D rb;

    [Header("Configuración de Rebote")]
    public int rebotesMaximos = 2;
    private int rebotesRestantes;
    private bool haFrenado = false;

    void Start()
    {
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
        if (PlayerMovement.instance.ammo == 3 && gameObject.CompareTag("bullet"))
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
        if (collision.gameObject.CompareTag("ground") && !haFrenado)
        {
            rebotesRestantes--;
            if (rebotesRestantes <= 0)
            {
                FrenarPickup();
            }
        }

        PlayerMovement scriptTocado = collision.gameObject.GetComponent<PlayerMovement>();

        if (collision.gameObject.CompareTag("player"))
        {
            if (gameObject.CompareTag("bullet") && PlayerMovement.instance.ammo < 3)
            {
                Destroy(gameObject);
                scriptTocado.ammo++;
                scriptTocado.aumentarEscala();
            }
            else if (gameObject.CompareTag("pickupClone"))
            {
                PlayerMovement.instance.cloneIsAvailable = true;
                Destroy(gameObject);
            }
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