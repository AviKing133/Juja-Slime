using UnityEngine;

public class MeleeController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("jujer"))
        {
            Destroy(collision.gameObject);
        }
    }
}
