using UnityEngine;
using System.Collections;

public class Enemigo1Controller : MonoBehaviour
{
    public enum EnemyState { Idle = 0, Run = 1, Hit = 2, Attack = 3, Death = 4 }

    [Header("Estadísticas")]
    public int vida = 2;
    public EnemyState estadoActual = EnemyState.Run;

    [Header("Configuración de Movimiento")]
    public float velocidad = 2f;
    public float fuerzaEmpuje = 12f;

    [Header("Detección")]
    public Transform detectorSuelo;
    public Transform detectorPared;
    public float distanciaRayoSuelo = 0.5f;
    public float distanciaRayoPared = 0.4f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool moviendoDerecha = true;
    private bool bloqueado = false; // Para que no se mueva durante Hit o Attack
    private bool puedeRecibirDaño = true;
    [Header("Ajustes de Seguridad")]
    public float cooldownDaño = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        CambiarEstado(EnemyState.Run);
    }

    void Update()
    {
        if (estadoActual == EnemyState.Death || bloqueado) return;

        Patrullar();
    }

    void Patrullar()
    {
        // Movimiento
        transform.Translate(Vector2.right * velocidad * Time.deltaTime);
        if (estadoActual != EnemyState.Run) CambiarEstado(EnemyState.Run);

        // 1. DETECCIÓN DE SUELO
        RaycastHit2D hitSuelo = Physics2D.Raycast(detectorSuelo.position, Vector2.down, distanciaRayoSuelo);
        bool detectaSueloValido = false;
        if (hitSuelo.collider != null && hitSuelo.collider.CompareTag("ground") && hitSuelo.collider.gameObject != gameObject)
        {
            detectaSueloValido = true;
        }

        // 2. DETECCIÓN DE PARED
        RaycastHit2D hitPared = Physics2D.Raycast(detectorPared.position, transform.right, distanciaRayoPared);
        bool detectaParedValida = false;
        if (hitPared.collider != null && hitPared.collider.gameObject != gameObject)
        {
            if (hitPared.collider.CompareTag("ground") || hitPared.collider.CompareTag("walls") || hitPared.collider.CompareTag("enemy"))
            {
                detectaParedValida = true;
            }
        }

        if (!detectaSueloValido || detectaParedValida)
        {
            GirarEnemigo();
        }
    }

    void GirarEnemigo()
    {
        moviendoDerecha = !moviendoDerecha;
        transform.eulerAngles = moviendoDerecha ? new Vector3(0, 0, 0) : new Vector3(0, -180, 0);
    }

    // --- RECIBIR DAÑO ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (estadoActual == EnemyState.Death) return;

        // Añadimos el check de "puedeRecibirDaño"
        if (puedeRecibirDaño && (collision.CompareTag("bullet") || collision.CompareTag("melee")))
        {
            RecibirDaño();
        }
    }

    public void RecibirDaño()
    {
        if (estadoActual == EnemyState.Death || !puedeRecibirDaño) return;

        vida--;
        StartCoroutine(ActivarCooldownDaño()); // Iniciamos el tiempo de espera

        if (vida <= 0)
        {
            CambiarEstado(EnemyState.Death);
            StartCoroutine(SecuenciaMuerte());
        }
        else
        {
            StartCoroutine(SecuenciaHit());
        }
    }

    // --- COLISIONES (ATAQUE) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (estadoActual == EnemyState.Death) return;

        if (collision.gameObject.CompareTag("player"))
        {
            StartCoroutine(SecuenciaAtaque(collision.gameObject));
        }
    }

    // --- CORRUTINAS DE ESTADOS ---

    IEnumerator SecuenciaHit()
    {
        bloqueado = true;
        CambiarEstado(EnemyState.Hit);
        yield return new WaitForSeconds(0.4f); // Duración de la animación de hit

        if (vida > 0)
        {
            bloqueado = false;
            CambiarEstado(EnemyState.Run);
        }
    }
    IEnumerator ActivarCooldownDaño()
    {
        puedeRecibirDaño = false;
        yield return new WaitForSeconds(cooldownDaño);
        // Solo volvemos a activar si no está muerto
        if (estadoActual != EnemyState.Death)
        {
            puedeRecibirDaño = true;
        }
    }
    IEnumerator SecuenciaAtaque(GameObject playerObj)
    {
        bloqueado = true;
        CambiarEstado(EnemyState.Attack);

        // Lógica de empuje que ya tenías
        PlayerController player = playerObj.GetComponent<PlayerController>();
        Rigidbody2D rbPlayer = playerObj.GetComponent<Rigidbody2D>();

        if (player != null && rbPlayer != null)
        {
            float direccionX = player.mirandoDerecha ? -1f : 1f;
            Vector2 fuerzaVector = new Vector2(direccionX, 0.5f).normalized * fuerzaEmpuje;
            rbPlayer.linearVelocity = Vector2.zero;
            rbPlayer.AddForce(fuerzaVector, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(0.2f); // Tiempo que dura el ataque
        bloqueado = false;
        CambiarEstado(EnemyState.Run);
    }

    IEnumerator SecuenciaMuerte()
    {
        bloqueado = true;
        CambiarEstado(EnemyState.Death);

        // Desactivamos colisionadores para que no moleste al morir
        GetComponent<Collider2D>().enabled = false;
        if (rb != null) rb.simulated = false;

        yield return new WaitForSeconds(1.5f); // Tiempo para ver la animación de muerte
        Destroy(gameObject);
    }

    void CambiarEstado(EnemyState nuevoEstado)
    {
        estadoActual = nuevoEstado;
        anim.SetInteger("State", (int)nuevoEstado);
    }

    private void OnDrawGizmos()
    {
        if (detectorSuelo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(detectorSuelo.position, detectorSuelo.position + Vector3.down * distanciaRayoSuelo);
        }
        if (detectorPared != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(detectorPared.position, detectorPared.position + (transform.right * distanciaRayoPared));
        }
    }
}