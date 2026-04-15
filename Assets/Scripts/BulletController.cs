using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    public float speed = 15f;
    public int maxBounces = 3;

    [Header("Levitación")]
    public float floatHeight = 1.5f; // Cuánto se eleva tras el último rebote
    public float floatSpeed = 2f;    // Velocidad del vaivén
    public float floatAmplitude = 0.3f; // Amplitud del vaivén (sube y baja)

    private int currentBounces = 0;
    private Rigidbody rb;
    private bool isFloating = false;
    private Vector3 floatCenterPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Impulso inicial
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si ya estamos levitando, no procesamos más rebotes
        if (isFloating) return;

        currentBounces++;

        if (currentBounces <= maxBounces)
        {
            // --- Lógica de Rebote Realista ---
            // Calculamos la dirección de reflexión basada en la normal del impacto
            Vector3 reflectDir = Vector3.Reflect(rb.linearVelocity.normalized, collision.contacts[0].normal);
            rb.linearVelocity = reflectDir * speed;

            // Rotamos la bala para que mire hacia su nueva dirección
            transform.rotation = Quaternion.LookRotation(reflectDir);
        }
        else
        {
            // Al llegar al límite, detenemos la bala e iniciamos la levitación
            StartCoroutine(TransitionToFloating());
        }
    }

    IEnumerator TransitionToFloating()
    {
        isFloating = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // Desactivamos físicas para controlar la posición manualmente

        // 1. Animación: Elevarse desde el punto de impacto
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + Vector3.up * floatHeight;
        float elapsed = 0;
        float duration = 0.8f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. Establecer el punto base para el efecto de levitación (vaivén)
        floatCenterPosition = transform.position;
    }

    void Update()
    {
        if (isFloating)
        {
            // Efecto de levitación usando una onda Seno
            float newY = floatCenterPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(floatCenterPosition.x, newY, floatCenterPosition.z);

            // Opcional: Que la bala gire un poco sobre sí misma mientras flota
            transform.Rotate(Vector3.up * 50f * Time.deltaTime);
        }
    }
}
