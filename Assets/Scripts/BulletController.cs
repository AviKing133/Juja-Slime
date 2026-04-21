using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    public float speed = 20f;
    public int maxBounces = 3; // Límite de rebotes

    [Header("Ajustes de Impacto/Squash")]
    [Range(0.1f, 0.5f)] public float squashFactor = 0.3f;
    public float squashRecoverSpeed = 8f;

    [Header("Ajustes de Flotación")]
    public float floatingAmplitude = 0.2f;
    public float floatingFrequency = 2f;
    public float hoverHeight = 0.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 originalScale;
    private int bounceCount;
    private bool isFloating = false;
    private bool firstHitDone = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        originalScale = transform.localScale;
        bounceCount = maxBounces;
        rb.gravityScale = 0f;
    }

    void Start()
    {
        if (this.gameObject.CompareTag("bullet"))
        {
            rb.linearVelocity = transform.right * speed;
        }
    }

    void Update()
    {
        // Recuperar forma original poco a poco
        if (!isFloating && transform.localScale != originalScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * squashRecoverSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFloating) return;

        // 1. Activar gravedad en el primer impacto
        if (!firstHitDone)
        {
            firstHitDone = true;
            rb.gravityScale = 1f;
        }

        // 2. Efecto visual de impacto (Giro y Aplastado)
        ApplyImpactEffect(collision);

        // 3. Lógica de contador de rebotes
        bounceCount--;

        if (bounceCount <= 0)
        {
            StartCoroutine(PrepareAndFloat());
        }
    }

    private void ApplyImpactEffect(Collision2D collision)
    {
        Vector2 contactPoint = collision.contacts[0].point;
        Vector2 directionToHit = (contactPoint - (Vector2)transform.position).normalized;

        if (directionToHit != Vector2.zero)
        {
            float angle = Mathf.Atan2(directionToHit.y, directionToHit.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Aplicar deformación
        transform.localScale = new Vector3(
            originalScale.x * squashFactor,
            originalScale.y * 1.4f,
            originalScale.z
        );
    }

    IEnumerator PrepareAndFloat()
    {
        isFloating = true;

        // Esperar un instante para que el último rebote físico termine de procesarse
        yield return new WaitForFixedUpdate();

        // Desactivar física para control total por script
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;

        // Asegurar escala final
        transform.localScale = originalScale;

        // --- ELEVACIÓN (0.5f para no clipear) ---
        Vector3 targetHoverPos = transform.position + Vector3.up * hoverHeight;
        float elapsed = 0f;
        float duration = 0.4f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetHoverPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- BUCLE DE FLOTACIÓN ---
        Vector3 basePos = transform.position;
        while (isFloating)
        {
            float newY = basePos.y + Mathf.Sin(Time.time * floatingFrequency) * floatingAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }
    }
}