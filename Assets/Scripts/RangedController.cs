using System.Collections.Generic;
using UnityEngine;

public class RangedController : MonoBehaviour
{
    public string enemyTag = "Enemy";
    public Transform currentTarget;
    public GameObject bulletPrefab;
    public PlayerController player;

    public Transform defaultSpawner;

    private List<Transform> enemiesInRange = new List<Transform>();
    private Transform[] bulletSpawners;

    void Awake()
    {
        bulletSpawners = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            bulletSpawners[i] = transform.GetChild(i);
        }

        // Si no asignaste uno manualmente, intentamos usar el primero por defecto
        if (defaultSpawner == null && bulletSpawners.Length > 0)
            defaultSpawner = bulletSpawners[0];
    }

    void Update()
    {
        UpdateNearestTarget();
    }

    void UpdateNearestTarget()
    {
        player = GetComponentInParent<PlayerController>();
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        enemiesInRange.RemoveAll(e => e == null);

        foreach (Transform enemy in enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        currentTarget = closestEnemy;
    }

    public void OrderFire()
    {
        PlayerController player = GetComponentInParent<PlayerController>();
        if (player != null && player.ammo <= 0) return;
        if (bulletPrefab == null) return;

        float angle = 0;
        Transform spawnerToUse = null;

        if (currentTarget != null)
        {
            // --- DISPARO A ENEMIGO ---
            spawnerToUse = GetNearestSpawner(currentTarget.position);
            Vector2 direccion = (currentTarget.position - spawnerToUse.position).normalized;
            angle = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        }
        else
        {
            // --- DISPARO POR DEFECTO (HACIA ADELANTE) ---
            spawnerToUse = defaultSpawner;
            if (spawnerToUse == null) return;

            // Si el player mira a la derecha el ángulo es 0, si mira a la izquierda es 180
            angle = (player != null && !player.mirandoDerecha) ? 180f : 0f;
        }

        // Instanciar la bala
        Instantiate(bulletPrefab, spawnerToUse.position, Quaternion.Euler(0, 0, angle));

        if (player != null) player.ammo--;
    }

    private Transform GetNearestSpawner(Vector3 targetPos)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform spawner in bulletSpawners)
        {
            float dist = Vector3.Distance(spawner.position, targetPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = spawner;
            }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag))
        {
            if (!enemiesInRange.Contains(other.transform))
                enemiesInRange.Add(other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag))
            enemiesInRange.Remove(other.transform);
    }
}