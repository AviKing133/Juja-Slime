using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    public float smoothTime = 0.3f;
    public Transform target;
    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;
    void Start()
    {
        offset = gameObject.transform.position - target.position;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;

        float extraLookAhead = 2.0f;
        if (PlayerMovement.instance.mirandoDerecha)
        {
            targetPosition.x += extraLookAhead;
        }
        else
        {
            targetPosition.x -= extraLookAhead;
        }

        // SmoothDamp para mover la cámara suavemente hacia la posición objetivo
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
