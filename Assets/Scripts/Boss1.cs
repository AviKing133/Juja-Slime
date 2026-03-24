using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class BossController : MonoBehaviour
{
    [SerializeField] private float health = 5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float duration = 3f;

    public bool isIdle = false;
    public bool isMovingRight = true;
    private bool isStunned = false;
    private Rigidbody2D rb;
    public Animator anim;


    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Move();
    }
    void Update()
    {
        if (!isStunned && !isIdle)
        {
            Move();
        }
        else if (isStunned)
        {
            GetStunned();
        }
    }

    private void Move()
    {
        rb.linearVelocityX = (isMovingRight ? moveSpeed : -moveSpeed);
    }
    private void GetStunned()
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("Stunned", true);
        StartCoroutine(StunDuration(duration));

    }
    IEnumerator StunDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        anim.SetBool("Stunned", false);
        Move();
    }
    public void Turn()
    {
        isMovingRight = !isMovingRight;
        Move();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player.haveDamage && health >= 1)
            {
                player.haveDamage = false;
                GetStunned();
                health -= 1;
            }
            else if (player.haveDamage && health < 1)
            {
                Destroy(gameObject);
            }
        }
        if (collision.gameObject.CompareTag("bullet") || collision.gameObject.CompareTag("pickupClone"))
        {
            Destroy(collision.gameObject);
        }
    }
}