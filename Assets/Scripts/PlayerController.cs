using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum SlimeState
    {
        Spawn = 0, Idle = 1, Movement = 2, PreCharge = 3, Charge = 4,
        Jump = 5, FallingAir = 6, MeleeAttack = 7, Shoot = 8, Hit = 9,
        Death = 10, Despawn = 11, Falling = 12
    }

    public static PlayerController instance;

    [Header("Identidad")]
    public bool esElOriginal = true;
    public SlimeState estadoActual = SlimeState.Spawn;

    [Header("Movimiento y Física")]
    public float velocidad = 8f;
    public bool mirandoDerecha = true;
    private Rigidbody2D rb;
    private float inputHorizontal;
    private bool enSuelo = false;

    [Header("Configuración Salto Cargado")]
    public float fuerzaMinima = 5f;
    public float fuerzaMaxima = 18f;
    public float tiempoCargaMax = 1.2f;
    // Angulo de salto: X es avance, Y es altura. 
    // Un valor de (0.5f, 1f) hará que salte más hacia arriba que hacia adelante.
    public Vector2 anguloSalto = new Vector2(0.5f, 1f);

    private float tiempoPresionado = 0f;
    private bool estaCargando = false;

    [Header("Configuración de Escala (Valores Planos)")]
    public float escalaActual = 0.6f;    
    public float perdidaPorClon = 0.2f;    
    public float perdidaPorBala = 0.1f;    
    public float escalaMinima = 0.3f;

    [Header("Clonación")]
    public GameObject prefabClone;
    public GameObject prefabPickupClone;
    public bool cloneIsAvailable = true;
    public float distanciaSpawn = 0.8f;
    public LayerMask capasObstaculos;

    [Header("Configuración Melee")]
    public Transform controladorGolpe; // Un objeto vacío frente al Slime
    public float radioGolpe = 0.5f;    // Tamaño de la hitbox circular
    public float dañoMelee = 10f;
    public float cadenciaMelee = 0.6f;
    public LayerMask capaEnemigos;    // Selecciona a quién quieres pegar
    private float tiempoUltimoMelee;

    [Header("Referencias")]
    public Animator anim;
    public InterfaceBehaviour Interface;

    void Awake()
    {
        // Singleton solo para el original
        if (esElOriginal)
        {
            if (instance == null) instance = this;
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        Interface = FindFirstObjectByType<InterfaceBehaviour>();

        if (esElOriginal)
        {
            anim.SetBool("IsAlive", false);
            CambiarEstado(SlimeState.Spawn);
        }
        else
        {
            // EL CLON: Empieza vivo directamente al ser instanciado por el ataque
            anim.SetBool("IsAlive", true);
            CambiarEstado(SlimeState.Idle);
            StartCoroutine(CicloDeVidaClon());
        }

        ActualizarEscala();
    }

    void Update()
    {
        // 1. Si estamos en Spawn, esperamos a que termine para pasar a Idle
        if (estadoActual == SlimeState.Spawn)
        {
            ManejarLogicaSpawn();
            return;
        }

        // 2. Solo si IsAlive es true, permitimos el resto de acciones
        if (anim.GetBool("IsAlive"))
        {
            ManejarInput();
            DeterminarEstadoFisico();
            ActualizarAnimator();
        }
        // CLON
        if (esElOriginal && Input.GetKeyDown(KeyCode.F) && cloneIsAvailable && estadoActual != SlimeState.MeleeAttack)
        {
            StartCoroutine(SecuenciaSpawnClon());
        }
    }

    void FixedUpdate()
    {
        if (PuedeMoverse())
        {
            rb.linearVelocity = new Vector2(inputHorizontal * velocidad, rb.linearVelocity.y);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Usamos Stay por si el Slime aterriza y se queda quieto, 
        // para asegurar que la variable se mantenga activa.
        if (collision.gameObject.CompareTag("ground"))
        {
            enSuelo = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            enSuelo = false;
        }
    }

    private void ManejarLogicaSpawn()
    {
        // Aquí puedes detectar si la animación de Spawn terminó
        // Por ahora, lo activamos manualmente o tras un tiempo
        // Ejemplo: Si el usuario presiona cualquier tecla o pasa un segundo:
        if (Input.anyKeyDown)
        {
            anim.SetBool("IsAlive", true);
            CambiarEstado(SlimeState.Idle);
        }
    }

    private void ManejarInput()
    {
        float h = 0;

        // --- MOVIMIENTO DIFERENCIADO ---
        if (esElOriginal)
        {
            if (Input.GetKey(KeyCode.D) && !estaCargando && enSuelo) h = 1;
            else if (Input.GetKey(KeyCode.A) && !estaCargando && enSuelo) h = -1;

            inputHorizontal = h;
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow) && !estaCargando && enSuelo) h = 1;
            else if (Input.GetKey(KeyCode.LeftArrow) && !estaCargando && enSuelo) h = -1;

            inputHorizontal = h;
        }
        
        KeyCode teclaMelee = esElOriginal ? KeyCode.Q : KeyCode.LeftShift;

        if (Input.GetKeyDown(teclaMelee) && Time.time > tiempoUltimoMelee + cadenciaMelee && enSuelo)
        {
            tiempoUltimoMelee = Time.time;
            StartCoroutine(SecuenciaMelee());
        }

        // --- LÓGICA DE SALTO CARGADO (PreCharge -> Charge -> Jump) ---
        if (esElOriginal)
        {
            if (Input.GetKeyDown(KeyCode.S) && enSuelo)
            {
                estaCargando = true;
                tiempoPresionado = 0f;
                CambiarEstado(SlimeState.PreCharge);
            }

            if (Input.GetKey(KeyCode.S) && estaCargando)
            {
                tiempoPresionado += Time.deltaTime;

                // Transición automática de Pre-carga a Carga en bucle
                if (tiempoPresionado > 0.15f && estadoActual == SlimeState.PreCharge)
                {
                    CambiarEstado(SlimeState.Charge);
                }
            }

            if (Input.GetKeyUp(KeyCode.S) && estaCargando)
            {
                CambiarEstado(SlimeState.Jump); // Estado 5: El impulso
                EjecutarSalto();
                estaCargando = false;
                ActualizarEscala(); // Resetea cualquier deformación si la hubiera
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) && enSuelo)
            {
                estaCargando = true;
                tiempoPresionado = 0f;
                CambiarEstado(SlimeState.PreCharge);
            }

            if (Input.GetKey(KeyCode.DownArrow) && estaCargando)
            {
                tiempoPresionado += Time.deltaTime;

                // Transición automática de Pre-carga a Carga en bucle
                if (tiempoPresionado > 0.15f && estadoActual == SlimeState.PreCharge)
                {
                    CambiarEstado(SlimeState.Charge);
                }
            }

            if (Input.GetKeyUp(KeyCode.DownArrow) && estaCargando)
            {
                CambiarEstado(SlimeState.Jump); // Estado 5: El impulso
                EjecutarSalto();
                estaCargando = false;
                ActualizarEscala(); // Resetea cualquier deformación si la hubiera
            }
        }

        // --- GIRO (FLIP) ---
        // Solo giramos si no estamos cargando el salto para evitar saltos hacia atrás raros
        if (!estaCargando)
        {
            if (inputHorizontal > 0 && !mirandoDerecha) Girar();
            else if (inputHorizontal < 0 && mirandoDerecha) Girar();
        }
    }

    private void DeterminarEstadoFisico()
    {
        if (estaCargando) return;

        // 1. Bloqueos críticos (Solo estados que duran hasta que el código diga lo contrario)
        // HE QUITADO MeleeAttack de aquí para que el código pueda sacarlo de ese estado
        if (estadoActual == SlimeState.Shoot || estadoActual == SlimeState.Spawn ||
            estadoActual == SlimeState.Hit)
            return;

        // 2. Lógica de AIRE
        if (!enSuelo)
        {
            if (estadoActual != SlimeState.Jump || rb.linearVelocity.y < -0.1f)
            {
                CambiarEstado(SlimeState.FallingAir); // Estado 6
            }
        }
        else // Lógica de SUELO
        {
            // 1. Gestión de aterrizaje
            if (estadoActual == SlimeState.FallingAir)
            {
                anim.SetTrigger("Land");
                CambiarEstado(SlimeState.Idle);
                return;
            }

            // 2. Si estamos moviéndonos, pasamos a Movement
            if (Mathf.Abs(inputHorizontal) > 0.1f)
            {
                CambiarEstado(SlimeState.Movement);
            }
            else
            {
                // 3. PARA VOLVER A IDLE:
                // Solo volvemos a Idle si NO estamos atacando, 
                // O si estamos en el estado de ataque pero ya NO estamos pulsando el botón (la corrutina debería haber terminado)
                if (estadoActual != SlimeState.MeleeAttack)
                {
                    CambiarEstado(SlimeState.Idle);
                }
            }
        }
    }
    private void EjecutarSalto()
    {
        // 1. Calculamos la fuerza basada en el tiempo de carga
        float porcentaje = Mathf.Clamp01(tiempoPresionado / tiempoCargaMax);
        float fuerzaFinal = Mathf.Lerp(fuerzaMinima, fuerzaMaxima, porcentaje);

        // 2. Calculamos la dirección (Salto parabólico hacia adelante)
        // Usamos el signo de la escala para saber hacia dónde mirar, 
        // o simplemente la variable mirandoDerecha.
        float dirX = mirandoDerecha ? anguloSalto.x : -anguloSalto.x;
        Vector2 direccionFinal = new Vector2(dirX, anguloSalto.y).normalized;

        // 3. Aplicamos la física
        rb.linearVelocity = Vector2.zero; // Limpiamos inercia para un salto limpio
        rb.AddForce(direccionFinal * fuerzaFinal, ForceMode2D.Impulse);

        // 4. Salimos del estado de carga
        estaCargando = false;
        enSuelo = false; // Forzamos que ya no está en el suelo para evitar dobles saltos

        // El método DeterminarEstadoFisico() en el Update se encargará 
        // de poner el Estado 4 (Fall) automáticamente al detectar velocidad en Y
    }
    private void RealizarAtaqueMelee()
    {
        // Creamos una "esfera" invisible que detecta colliders
        Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe, capaEnemigos);

        foreach (Collider2D enemigo in objetosGolpeados)
        {
            // Aquí llamarías al sistema de daño del enemigo
            Debug.Log("Golpeaste a: " + enemigo.name);

            // Ejemplo: enemigo.GetComponent<Enemigo>().RecibirDaño(dañoMelee);
        }
    }
    public void ActualizarEscala()
    {
        // Nos aseguramos de no bajar de un mínimo
        escalaActual = Mathf.Max(escalaActual, escalaMinima);

        // Aplicamos la escala (el signo de X gestiona hacia donde mira)
        float direccionX = mirandoDerecha ? 1 : -1;
        transform.localScale = new Vector3(escalaActual * direccionX, escalaActual, 1);
    }

    public void Clonarse()
    {
        if (Input.GetKeyDown(KeyCode.F) && cloneIsAvailable && enSuelo && !estaCargando && esElOriginal)
        {
            StartCoroutine(SecuenciaSpawnClon());
        }
    }

    // Corrutinas
    private IEnumerator SecuenciaMelee()
    {
        CambiarEstado(SlimeState.MeleeAttack);

        yield return new WaitForSeconds(0.2f);
        RealizarAtaqueMelee();

        yield return new WaitForSeconds(0.3f);

        // --- EL DESBLOQUEO FINAL ---
        if (Mathf.Abs(inputHorizontal) > 0.1f)
            CambiarEstado(SlimeState.Movement);
        else
            CambiarEstado(SlimeState.Idle);
    }
    IEnumerator SecuenciaSpawnClon()
    {
        if (!esElOriginal) yield break;

        yield return new WaitForSeconds(0.2f); // Tiempo para el impacto

        // 2. Comprobamos espacio
        Vector2 direccion = mirandoDerecha ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, distanciaSpawn, capasObstaculos);

        if (hit.collider == null)
        {
            cloneIsAvailable = false;

            // RESTA PLANA AL ORIGINAL
            escalaActual -= perdidaPorClon;

            Vector3 spawnPos = transform.position + new Vector3(mirandoDerecha ? distanciaSpawn : -distanciaSpawn, 0, 0);
            GameObject nuevoClonObj = Instantiate(prefabClone, spawnPos, Quaternion.identity);

            // ¡OJO! Asegúrate de que el componente buscado es el nuevo (SlimeController)
            PlayerController scriptClon = nuevoClonObj.GetComponent<PlayerController>();

            if (scriptClon != null)
            {
                scriptClon.esElOriginal = false;
                scriptClon.mirandoDerecha = this.mirandoDerecha;

                // ASIGNACIÓN PLANA AL CLON
                // Le damos una escala inicial fija (ej: 0.4)
                scriptClon.escalaActual = 2f;

                scriptClon.ActualizarEscala();
            }

            ActualizarEscala();
        }
        CambiarEstado(SlimeState.Idle);
    }
    IEnumerator CicloDeVidaClon()
    {
        // Espera activo 10 segundos
        yield return new WaitForSeconds(10f);

        // 1. Iniciar Despawn (Animación Estado 8)
        CambiarEstado(SlimeState.Despawn);
        ActualizarAnimator();

        // 2. Esperar a que la animación de Despawn termine (ajusta según tu clip)
        yield return new WaitForSeconds(1f);

        // 3. Crear el Pickup con la munición sobrante
        if (prefabPickupClone != null)
        {
            GameObject pickup = Instantiate(prefabPickupClone, transform.position, Quaternion.identity);
            var controller = pickup.GetComponent<PickupController>();
        }

        // 4. Avisar al original que ya puede spawnear otro y morir
        if (PlayerMovement.instance != null) PlayerMovement.instance.cloneIsAvailable = true;
        Destroy(gameObject);
    }

    // METODOS DE APOYO

    public void FinalizarSpawn() { anim.SetBool("IsAlive", true); CambiarEstado(SlimeState.Idle); }
    public void CambiarEstado(SlimeState nuevo) { estadoActual = nuevo; }
    private void ActualizarAnimator() { anim.SetInteger("State", (int)estadoActual); }
    private bool PuedeMoverse()
    {
        return estadoActual == SlimeState.Idle ||
               estadoActual == SlimeState.Movement ||
               estadoActual == SlimeState.FallingAir ||
               estadoActual == SlimeState.Jump ||
               estadoActual == SlimeState.Falling;
    }
    private void Girar() { mirandoDerecha = !mirandoDerecha; ActualizarEscala(); }
}