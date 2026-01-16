using System.Collections;
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
    public GameObject prefabPickupClone;
    public Transform shootPoint;

    public bool cloneIsAvailable = false;
    private float shootCooldown = 0.5f;
    private float shootTimer = 0f;

    public int ammo = 1;

    private void Awake()
    {
        if (esElOriginal)
        {
            if (instance == null) instance = this;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!esElOriginal) StartCoroutine(CicloDeVidaClon());
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        if (esElOriginal && Input.GetKeyDown(KeyCode.F) && cloneIsAvailable)
        {
            GameObject nuevoClon = Instantiate(prefabClone, new Vector3(transform.position.x + 1, transform.position.y, 0), Quaternion.identity);
            PlayerMovement scriptClon = nuevoClon.GetComponent<PlayerMovement>();
            if (scriptClon != null) scriptClon.esElOriginal = false;
            cloneIsAvailable = false;
        }

        movimientoHorizontal = 0;

        if (esElOriginal)
        {
            if (Input.GetKey(KeyCode.D)) movimientoHorizontal = velocidad;
            else if (Input.GetKey(KeyCode.A)) movimientoHorizontal = -velocidad;

            if (Input.GetKeyDown(KeyCode.Space) && enSuelo)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);

            if (Input.GetKey(KeyCode.S) && shootTimer >= shootCooldown && ammo > 0)
            {
                Disparar();
                shootTimer = 0f;
                ammo--;
                disminuirEscala();
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow)) movimientoHorizontal = velocidad;
            else if (Input.GetKey(KeyCode.LeftArrow)) movimientoHorizontal = -velocidad;

            if (Input.GetKeyDown(KeyCode.Space) && enSuelo)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
                enSuelo = false;
            }

            if (Input.GetKey(KeyCode.DownArrow) && shootTimer >= shootCooldown && ammo > 0)
            {
                Disparar();
                shootTimer = 0f;
                ammo--;
                disminuirEscala();
            }
        }

        if (movimientoHorizontal > 0 && !mirandoDerecha) Girar();
        else if (movimientoHorizontal < 0 && mirandoDerecha) Girar();
    }

    void Disparar()
    {
        GameObject balaObj = Instantiate(prefabBullet, shootPoint.position, Quaternion.identity);
        Bala scriptBala = balaObj.GetComponent<Bala>();
        if (scriptBala != null)
        {
            scriptBala.dueno = this;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(movimientoHorizontal, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("player") && gameObject.CompareTag("clone")) enSuelo = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground")) enSuelo = false;
    }
    IEnumerator CicloDeVidaClon()
    {
        yield return new WaitForSeconds(10f);
        // Al morir, instancia el pickup en su posición actual
        if (prefabPickupClone != null)
        {
            Instantiate(prefabPickupClone, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
    void Girar()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }
    public void aumentarEscala()
    {
        transform.localScale += transform.localScale * 0.15f;
    }

    public void disminuirEscala()
    {
        transform.localScale -= transform.localScale * 0.15f;
    }
}