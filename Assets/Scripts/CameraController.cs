using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Configuración de Suavizado")]
    public float smoothTimeDefault = 0.25f; // Tiempo normal
    public float smoothTimeCerca = 0.5f;   // Más lento al estar cerca para no oscilar
    public float maxSpeed = 20f;           // Límite de velocidad de la cámara

    private float currentSmoothTime;
    private Vector3 velocity = Vector3.zero;

    [Header("Seguimiento")]
    public Transform playerOriginal;
    private Transform targetActual;

    [Header("Offsets")]
    public Vector3 offset = new Vector3(0, 1, -10);
    public float lookAheadX = 3.0f;
    public float lookAheadYUp = 2.0f;
    public float lookAheadYDown = 5.0f;

    [Header("Umbral de Movimiento")]
    public float umbralMovimiento = 0.2f;

    void Start()
    {
        targetActual = playerOriginal;
        currentSmoothTime = smoothTimeDefault;
    }

    void LateUpdate()
    {
        GestionarPrioridadTarget();
        if (targetActual == null) return;

        Rigidbody2D rb = targetActual.GetComponent<Rigidbody2D>();
        PlayerController scriptMov = targetActual.GetComponent<PlayerController>();

        Vector3 desplazamiento = Vector3.zero;

        if (rb != null && scriptMov != null)
        {
            desplazamiento.x = scriptMov.mirandoDerecha ? lookAheadX : -lookAheadX;

            if (rb.linearVelocity.y > 0.5f)
                desplazamiento.y = lookAheadYUp;
            else if (rb.linearVelocity.y < -1.0f)
                desplazamiento.y = -lookAheadYDown;
        }

        Vector3 targetPosition = targetActual.position + offset + desplazamiento;

        // --- LÓGICA DE ANTIOSCILACIÓN ---
        float distancia = Vector3.Distance(transform.position, targetPosition);

        // Si la cámara está a menos de 2 unidades, aumentamos el smoothTime
        // para que "frene" sutilmente en lugar de pasarse de largo.
        currentSmoothTime = (distancia < 2f) ? smoothTimeCerca : smoothTimeDefault;

        // Aplicamos el movimiento con límite de velocidad (maxSpeed)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            currentSmoothTime,
            maxSpeed
        );
    }

    void GestionarPrioridadTarget()
    {
        PlayerController[] todos = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        PlayerController clon = null;
        foreach (var p in todos) { if (!p.esElOriginal) clon = p; }

        if (clon == null)
        {
            targetActual = playerOriginal;
            return;
        }

        Rigidbody2D rbOriginal = playerOriginal.GetComponent<Rigidbody2D>();
        Rigidbody2D rbClon = clon.GetComponent<Rigidbody2D>();

        float velOriginal = rbOriginal != null ? rbOriginal.linearVelocity.magnitude : 0;
        float velClon = rbClon != null ? rbClon.linearVelocity.magnitude : 0;

        if (velClon > umbralMovimiento)
            targetActual = clon.transform;
        else if (velOriginal > umbralMovimiento)
            targetActual = playerOriginal;
    }
}