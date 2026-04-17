using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Configuración de Suavizado")]
    public float smoothTimeDefault = 0.25f;
    public float smoothTimeCerca = 0.5f;
    public float maxSpeed = 20f;

    [Header("Retraso y Zona Muerta")]
    public float radioZonaMuerta = 1.5f; // Distancia que el player puede moverse sin que la cámara lo siga
    public float tiempoEsperaCentrado = 3.0f; // Segundos que espera antes de centrarse si el player no se mueve
    private float timerCentrado;
    private Vector3 ultimaPosicionTargetSeguida;

    private float currentSmoothTime;
    private Vector3 velocity = Vector3.zero;
    private Camera cam;

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

    private float tamañoOriginal;
    private Vector3 shakeOffset = Vector3.zero;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetActual = playerOriginal;
        currentSmoothTime = smoothTimeDefault;
        tamañoOriginal = cam.orthographicSize;
        ultimaPosicionTargetSeguida = targetActual.position;
    }

    void LateUpdate()
    {
        GestionarPrioridadTarget();
        if (targetActual == null) return;

        Rigidbody2D rb = targetActual.GetComponent<Rigidbody2D>();
        PlayerController scriptMov = targetActual.GetComponent<PlayerController>();

        // 1. Lógica de Desplazamiento (Look Ahead)
        Vector3 desplazamiento = Vector3.zero;
        if (rb != null && scriptMov != null)
        {
            desplazamiento.x = scriptMov.mirandoDerecha ? lookAheadX : -lookAheadX;
            if (rb.linearVelocity.y > 0.5f) desplazamiento.y = lookAheadYUp;
            else if (rb.linearVelocity.y < -1.0f) desplazamiento.y = -lookAheadYDown;
        }

        Vector3 targetPositionFinal = targetActual.position + offset + desplazamiento;

        // 2. LÓGICA DE ZONA MUERTA Y RETRASO
        float distanciaAlUltimoPunto = Vector3.Distance(targetActual.position, ultimaPosicionTargetSeguida);

        // Si el jugador se mueve más allá del radio, la cámara DEBE seguirlo
        if (distanciaAlUltimoPunto > radioZonaMuerta)
        {
            ultimaPosicionTargetSeguida = targetActual.position;
            timerCentrado = 0; // Resetear timer porque hay movimiento relevante
        }
        else
        {
            // Si el movimiento es pequeño, sumamos tiempo al timer
            timerCentrado += Time.deltaTime;

            // Si aún no han pasado los 3 segundos, mantenemos la cámara apuntando a la última posición relevante
            if (timerCentrado < tiempoEsperaCentrado)
            {
                targetPositionFinal = ultimaPosicionTargetSeguida + offset + desplazamiento;
            }
        }

        // 3. Suavizado de movimiento
        float distanciaCamara = Vector3.Distance(transform.position, targetPositionFinal);
        currentSmoothTime = (distanciaCamara < 2f) ? smoothTimeCerca : smoothTimeDefault;

        Vector3 nuevaPosicion = Vector3.SmoothDamp(
            transform.position,
            targetPositionFinal,
            ref velocity,
            currentSmoothTime,
            maxSpeed
        );

        transform.position = nuevaPosicion + shakeOffset;
    }

    // --- MÉTODOS DE EFECTOS ---
    public void AplicarEfectoImpacto(float duracion, float zoomDeseado, float fuerzaShake)
    {
        StopAllCoroutines();
        StartCoroutine(CorrutinaEfecto(duracion, zoomDeseado, fuerzaShake));
    }

    IEnumerator CorrutinaEfecto(float duracion, float zoomDeseado, float fuerza)
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float p = tiempo / duracion;
            float curvaZoom = Mathf.Sin(p * Mathf.PI);
            cam.orthographicSize = Mathf.Lerp(tamañoOriginal, zoomDeseado, curvaZoom);
            shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * fuerza * (1 - p);
            yield return null;
        }
        cam.orthographicSize = tamañoOriginal;
        shakeOffset = Vector3.zero;
    }

    void GestionarPrioridadTarget()
    {
        PlayerController[] todos = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        PlayerController clon = null;
        foreach (var p in todos) { if (!p.esElOriginal) clon = p; }

        if (clon == null) { targetActual = playerOriginal; return; }

        Rigidbody2D rbOriginal = playerOriginal.GetComponent<Rigidbody2D>();
        Rigidbody2D rbClon = clon.GetComponent<Rigidbody2D>();

        float velOriginal = rbOriginal != null ? rbOriginal.linearVelocity.magnitude : 0;
        float velClon = rbClon != null ? rbClon.linearVelocity.magnitude : 0;

        if (velClon > umbralMovimiento) targetActual = clon.transform;
        else if (velOriginal > umbralMovimiento) targetActual = playerOriginal;
    }
}