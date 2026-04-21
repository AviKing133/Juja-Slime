using UnityEngine;
using System.Collections;

public class PickupController : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Configuración de Rebote")]
    public int rebotesMaximos = 2;
    private int rebotesRestantes;
    private bool haFrenado = false;

    [Header("Ajustes de Flotación")]
    public float floatingAmplitude = 0.2f;
    public float floatingFrequency = 2f;
    public float hoverHeight = 0.5f;

    [Header("Ajustes de Impacto/Squash")]
    [Range(0.1f, 0.5f)] public float squashFactor = 0.3f;
    public float squashRecoverSpeed = 8f;

    private Vector3 originalScale;

    [Header("Datos de Contenido")]
    public int storedAmmo = 1; // Asegúrate de que no sea 0
    private bool isFloating = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        // Sincronizamos la variable de rebotes
        rebotesRestantes = rebotesMaximos;

        // Impulso inicial aleatorio
        rb.AddForce(new Vector2(Random.Range(-2f, 2f), 4f), ForceMode2D.Impulse);
    }

    void Update()
    {
        // Recuperar la escala poco a poco si ha sido deformado
        if (transform.localScale != originalScale && !isFloating)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * squashRecoverSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground") && !haFrenado)
        {
            ApplyImpactEffect();

            rebotesRestantes--;

            if (rebotesRestantes <= 0)
            {
                haFrenado = true;
                StartCoroutine(PrepareAndFloat());
            }
        }
    }

    IEnumerator PrepareAndFloat()
    {
        yield return new WaitForFixedUpdate();

        // 1. Detener física
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 2. Resetear rotación y escala para que flote derecho
        transform.rotation = Quaternion.identity;
        transform.localScale = originalScale;

        // 3. Elevación suave
        Vector3 targetHoverPos = transform.position + Vector3.up * hoverHeight;
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetHoverPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. ACTIVAR BUCLE (Vital)
        isFloating = true;
        Vector3 basePos = transform.position;

        while (isFloating)
        {
            float newY = basePos.y + Mathf.Sin(Time.time * floatingFrequency) * floatingAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }
    }

    private void ApplyImpactEffect()
    {
        // Deformación simple: se aplasta en Y, se ensancha en X
        transform.localScale = new Vector3(
            originalScale.x * 1.3f,
            originalScale.y * squashFactor,
            originalScale.z
        );
    }
}