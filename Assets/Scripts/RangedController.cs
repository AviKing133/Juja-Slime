using System.Collections.Generic;
using UnityEngine;

public class RangedController : MonoBehaviour
{
    public string enemyTag = "Enemy";
    public Transform currentTarget;
    public GameObject bulletPrefab;

    private List<Transform> enemiesInRange = new List<Transform>();
    private Transform[] bulletSpawners; // Se llenará automáticamente con los hijos

    void Awake()
    {
        // Obtiene todos los hijos del objeto para usarlos como puntos de disparo
        // Esto evita tener que arrastrarlos manualmente al inspector
        bulletSpawners = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            bulletSpawners[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        UpdateNearestTarget();
    }

    void UpdateNearestTarget()
    {
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

    // --- MÉTODO PARA LLAMAR DESDE EL PLAYER ---
    public void OrderFire()
    {
        if (currentTarget == null)
        {
            Debug.Log("No disparo: No hay target.");
            return;
        }

        Transform bestSpawner = GetNearestSpawner(currentTarget.position);

        if (bestSpawner != null && bulletPrefab != null)
        {
            // 1. Instanciar
            GameObject bullet = Instantiate(bulletPrefab, bestSpawner.position, Quaternion.identity);

            // 2. Cálculo de dirección 2D
            Vector2 direccion = (currentTarget.position - bestSpawner.position).normalized;

            // 4. LOG DE CONTROL (Revisa la consola de Unity)
            Debug.Log($"Bala instanciada hacia {currentTarget.name} desde {bestSpawner.name}");

        }
        else
        {
            Debug.LogWarning("Falta el Prefab de la bala o los Spawners hijos.");
        }
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