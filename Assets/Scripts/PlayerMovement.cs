using System.Collections;

using UnityEngine;

using UnityEngine.Rendering;

using UnityEngine.SceneManagement;



public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;


    [Header("Configuración Raycast")]
    public LayerMask capasObstaculos;
    public float distanciaSpawn = 0.5f;

    [Header("Configuración de Escala")]
    public float escalaBaseOriginal = 0.6f;
    public float valorMasaClon = 0.4f;
    public float valorPorBala = 0.1f;

    [Header("Estado")]
    public bool esElOriginal = true;
    public bool cloneIsAvailable = true;
    public int ammo = 0;
    public bool mirandoDerecha = true;

    [SerializeField] private bool enSuelo;

    private float timerAturdimiento = 0f;
    private Rigidbody2D rb;
    private float movimientoHorizontal = 0f;

    [Header("Configuración Movimiento")]
    public float velocidad = 8f;
    public float fuerzaSalto = 12f;

    [Header("Configuración del Salto Cargado")]
    public float fuerzaMinima = 5f;
    public float fuerzaMaxima = 20f;
    public float tiempoCargaMax = 1.5f;
    private float tiempoPresionado = 0f;
    private bool cargandoSalto = false;

    [Header("Dirección del Salto")]
    public Vector2 direccionSalto = new Vector2(-1f, 1f);

    [Header("Referencias")]
    public InterfaceBehaviour Interface;
    public GameObject prefabBullet;
    public GameObject prefabClone;
    public GameObject prefabPickupClone;
    public Transform shootPoint;
    public Animator anim;

    private float dmgTimer = 0f;
    private float dmgCooldown = 1.5f;
    private float shootTimer = 0f;
    private float shootCooldown = 0.5f;
    public bool haveDamage = false;
    private void Awake()

    {
        if (esElOriginal && instance == null) instance = this;
    }

    void Start()
    {
        Interface = FindAnyObjectByType<InterfaceBehaviour>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (!esElOriginal) StartCoroutine(CicloDeVidaClon());
        ActualizarEscala();
        if (esElOriginal)
            Interface.UpdateVidas(ammo);
    }

    void Update()
    {
        shootTimer += Time.deltaTime;
        dmgTimer += Time.deltaTime;
        // Reducimos el timer de aturdimiento cada frame
        if (timerAturdimiento > 0)
        {
            timerAturdimiento -= Time.deltaTime;
        }

        if (esElOriginal && Input.GetKeyDown(KeyCode.F) && cloneIsAvailable)
        {
            SpawnearClon();
        }
        ManejarInputs();
        if (movimientoHorizontal > 0 && !mirandoDerecha) Girar();
        else if (movimientoHorizontal < 0 && mirandoDerecha) Girar();
    }



    void FixedUpdate()

    {
        // SI NO ESTAMOS ATURDIDOS: Controlamos la velocidad normalmente
        // SI ESTAMOS ATURDIDOS: No tocamos el Rigidbody
        if (timerAturdimiento <= 0)
        {
            rb.linearVelocity = new Vector2(movimientoHorizontal, rb.linearVelocity.y);
        }
    }
    public void RecibirGolpe()
    {
        timerAturdimiento = 0.3f;
    }
    void ManejarInputs()
    {
        movimientoHorizontal = 0;

        // 1. Definir Teclas según quién sea este objeto
        KeyCode teclaSalto = esElOriginal ? KeyCode.S : KeyCode.DownArrow;
        KeyCode teclaDisparo = esElOriginal ? KeyCode.Q : KeyCode.E;

        // 2. Movimiento Horizontal (Solo si no está cargando salto)
        if (!cargandoSalto)
        {
            if (esElOriginal)
            {
                if (Input.GetKey(KeyCode.D)) movimientoHorizontal = velocidad;
                else if (Input.GetKey(KeyCode.A)) movimientoHorizontal = -velocidad;
            }
            else
            {
                if (Input.GetKey(KeyCode.RightArrow)) movimientoHorizontal = velocidad;
                else if (Input.GetKey(KeyCode.LeftArrow)) movimientoHorizontal = -velocidad;
            }
        }

        // 3. Lógica de Disparo
        if (Input.GetKeyDown(teclaDisparo))
        {
            Disparar(); // Llama a tu función de disparo existente
        }

        // 4. Lógica de Salto Cargado
        // Iniciar carga
        if (Input.GetKeyDown(teclaSalto) && enSuelo)
        {
            cargandoSalto = true;
            tiempoPresionado = 0f;
        }

        // Acumular fuerza mientras mantiene pulsado
        if (Input.GetKey(teclaSalto) && cargandoSalto && enSuelo)
        {
            tiempoPresionado += Time.deltaTime;
        }

        // Ejecutar salto al soltar
        if (Input.GetKeyUp(teclaSalto) && cargandoSalto)
        {
            EjecutarSaltoCargado();
            cargandoSalto = false;
        }

        // 5. Animaciones
        anim.SetBool("IsMoving", movimientoHorizontal != 0);
    }

    public void PerderVida()
    {
        if (ammo > 0) { ammo--; ActualizarEscala(); }
        else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Interface.UpdateVidas(ammo);
    }
    void EjecutarSaltoCargado()
    {
        float porcentajeCarga = Mathf.Clamp01(tiempoPresionado / tiempoCargaMax);
        float fuerzaFinal = Mathf.Lerp(fuerzaMinima, fuerzaMaxima, porcentajeCarga);

        // Ajustar dirección según hacia donde mira
        float dirX = mirandoDerecha ? -direccionSalto.x : direccionSalto.x;
        Vector2 direccionFinal = new Vector2(dirX, direccionSalto.y);

        rb.linearVelocity = Vector2.zero; // Evita que la inercia previa arruine el salto
        rb.AddForce(direccionFinal.normalized * fuerzaFinal, ForceMode2D.Impulse);
    }
    void SpawnearClon()
    {
        Vector2 direccion = mirandoDerecha ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, distanciaSpawn, capasObstaculos);
        // Si el hit es null, significa que el rayo NO tocó ninguna pared
        if (hit.collider == null)
        {
            cloneIsAvailable = false;
            float offset = mirandoDerecha ? distanciaSpawn : -distanciaSpawn;
            Vector3 spawnPos = transform.position + new Vector3(offset, 0, 0);
            GameObject nuevoClonObj = Instantiate(prefabClone, spawnPos, Quaternion.identity);
            PlayerMovement scriptClon = nuevoClonObj.GetComponent<PlayerMovement>();

            if (scriptClon != null)
            {
                scriptClon.esElOriginal = false;
                scriptClon.mirandoDerecha = this.mirandoDerecha;
                if (this.ammo >= 2) { this.ammo--; scriptClon.ammo = 1; }
                else { scriptClon.ammo = 0; }
                scriptClon.ActualizarEscala();
            }
            ActualizarEscala();
        }
        else
        {
            Debug.Log("Bloqueado por: " + hit.collider.name);
        }
        Interface.UpdateVidas(ammo);
    } 

    public void ActualizarEscala()
    {
        float nuevaEscalaY;
        if (esElOriginal)
        {
            float bonoClon = cloneIsAvailable ? valorMasaClon : 0f;
            nuevaEscalaY = escalaBaseOriginal + (ammo * valorPorBala) + bonoClon;
        }
        else
        {
            nuevaEscalaY = valorMasaClon + (ammo * valorPorBala);
        }
        float signoX = mirandoDerecha ? 1 : -1;
        transform.localScale = new Vector3(nuevaEscalaY * signoX, nuevaEscalaY, 1);
    }
    void Disparar()
    {
        shootTimer = 0f;
        ammo--;
        if (esElOriginal)
        {
            Interface.UpdateVidas(ammo);
        }

        GameObject balaObj = Instantiate(prefabBullet, shootPoint.position, Quaternion.identity);
        Bullet scriptBala = balaObj.GetComponent<Bullet>();
        if (scriptBala != null) scriptBala.dueno = this;
        ActualizarEscala();
    }
    void Girar() { mirandoDerecha = !mirandoDerecha; ActualizarEscala(); }
    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("ground")) enSuelo = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("threat") && esElOriginal && dmgTimer > dmgCooldown) enSuelo = true; Interface.UpdateVidas(ammo); ActualizarEscala(); dmgTimer = 0f;
    }

    private void OnCollisionExit2D(Collision2D c)

    {
        if (c.gameObject.CompareTag("ground")) enSuelo = false;
    }

    IEnumerator CicloDeVidaClon()

    {
        yield return new WaitForSeconds(10f);
        if (prefabPickupClone != null)
        {
            GameObject pickup = Instantiate(prefabPickupClone, transform.position, Quaternion.identity);
            pickup.GetComponent<PickupController>().storedAmmo = ammo;
        }
        Destroy(gameObject);
    }

}