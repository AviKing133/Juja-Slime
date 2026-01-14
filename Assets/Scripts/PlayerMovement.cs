using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;

    [Header("Configuración de Movimiento")]
    public float velocidad = 8f;
    public float fuerzaSalto = 12f;

    [Header("Estado")]
    [SerializeField] private bool enSuelo;
    public bool esElOriginal = true;

    private Rigidbody2D rb;
    private float movimientoHorizontal = 0f;
    public bool mirandoDerecha = true;

    [Header("Referencias")]
    public GameObject prefabBullet;
    public GameObject prefabClone;
    public Transform shootPoint;

    private bool cloneIsActive = false;
    private float shootCooldown = 0.5f;
    private float shootTimer = 0f;

    private void Awake()
    {
        // Solo el Juja original debe ser la instancia persistente
        if (esElOriginal)
        {
            if (instance == null) instance = this;
            // Quitamos el Destroy para que el clon pueda existir
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Si soy un clon, programo mi destrucción una sola vez
        if (!esElOriginal)
        {
            Destroy(gameObject, 10f);
        }
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        // Lógica de control de existencia del clon
        if (esElOriginal && cloneIsActive)
        {
            if (GameObject.FindWithTag("clone") == null)
            {
                cloneIsActive = false;
            }
        }

        // Crear clon (Solo el original puede)
        if (esElOriginal && Input.GetKeyDown(KeyCode.F) && !cloneIsActive)
        {
            GameObject nuevoClon = Instantiate(prefabClone, new Vector3(transform.position.x + 1, transform.position.y, 0), Quaternion.identity);

            // IMPORTANTE: Aseguramos que el clon sepa que es un clon
            PlayerMovement scriptClon = nuevoClon.GetComponent<PlayerMovement>();
            if (scriptClon != null) scriptClon.esElOriginal = false;

            cloneIsActive = true;
        }

        movimientoHorizontal = 0;

        if (esElOriginal)
        {
            // Movimiento Juja (WASD)
            if (Input.GetKey(KeyCode.D)) movimientoHorizontal = velocidad;
            else if (Input.GetKey(KeyCode.A)) movimientoHorizontal = -velocidad;

            if (Input.GetKeyDown(KeyCode.Space) && enSuelo)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            }

            // Disparo con corrección de >=
            if (Input.GetKey(KeyCode.S) && shootTimer >= shootCooldown)
            {
                Disparar();
                shootTimer = 0f;
            }
        }
        else
        {
            // Movimiento Clon (Flechas)
            if (Input.GetKey(KeyCode.RightArrow)) movimientoHorizontal = velocidad;
            else if (Input.GetKey(KeyCode.LeftArrow)) movimientoHorizontal = -velocidad;

            if (Input.GetKeyDown(KeyCode.Space) && enSuelo)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            }
        }

        if (movimientoHorizontal > 0 && !mirandoDerecha) Girar();
        else if (movimientoHorizontal < 0 && mirandoDerecha) Girar();
    }

    void Disparar()
    {
        Instantiate(prefabBullet, shootPoint.position, Quaternion.identity);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(movimientoHorizontal, rb.linearVelocity.y);
    }

    // --- Colisiones ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground")) enSuelo = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground")) enSuelo = false;
    }

    void Girar()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }
}