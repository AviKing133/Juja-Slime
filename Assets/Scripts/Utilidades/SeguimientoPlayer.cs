using UnityEngine;

public class SeguimientoPlayer : MonoBehaviour
{
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        transform.position = playerPosition.position + offset;
    }
}
