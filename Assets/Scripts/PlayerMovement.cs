using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;

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

    [Header("Referencias")]
    public InterfaceBehaviour Interface;
    public GameObject prefabBullet;
    public GameObject prefabClone;
    public GameObject prefabPickupClone;
    public Transform shootPoint;
    public Animator anim;

    private float shootTimer = 0f;
    private float shootCooldown = 0.5f;

    private void Awake()
    {
        if (esElOriginal && instance == null) instance = this;
    }

    void Start()
    {
        Interface = Object.FindAnyObjectByType<InterfaceBehaviour>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (!esElOriginal) StartCoroutine(CicloDeVidaClon());
        ActualizarEscala();
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

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
        if (esElOriginal)
        {
            if (Input.GetKey(KeyCode.D)) movimientoHorizontal = velocidad;
            else if (Input.GetKey(KeyCode.A)) movimientoHorizontal = -velocidad;
            if (Input.GetKeyDown(KeyCode.Space) && enSuelo) Salto();
            if (Input.GetKey(KeyCode.S) && shootTimer >= shootCooldown && ammo > 0) Disparar();
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow)) movimientoHorizontal = velocidad;
            else if (Input.GetKey(KeyCode.LeftArrow)) movimientoHorizontal = -velocidad;
            if (Input.GetKeyDown(KeyCode.Space) && enSuelo) Salto();
            if (Input.GetKey(KeyCode.DownArrow) && shootTimer >= shootCooldown && ammo > 0) Disparar();
        }
        anim.SetBool("IsMoving", movimientoHorizontal != 0);
    }

    public void PerderVida()
    {
        if (ammo > 0) { ammo--; ActualizarEscala(); }
        else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SpawnearClon()
    {
        cloneIsAvailable = false;
        float offset = mirandoDerecha ? 1.2f : -1.2f;
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
        Interface.UpdateVidas(ammo);
        ActualizarEscala();
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
        Interface.UpdateVidas(ammo);
        GameObject balaObj = Instantiate(prefabBullet, shootPoint.position, Quaternion.identity);
        Bullet scriptBala = balaObj.GetComponent<Bullet>();
        if (scriptBala != null) scriptBala.dueno = this;
        ActualizarEscala();
    }

    void Salto() { rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto); enSuelo = false; }
    void Girar() { mirandoDerecha = !mirandoDerecha; ActualizarEscala(); }
    private void OnCollisionEnter2D(Collision2D c) 
    {
        if (c.gameObject.CompareTag("ground")) enSuelo = true; 
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