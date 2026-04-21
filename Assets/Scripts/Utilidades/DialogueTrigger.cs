using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea(3, 10)] public string pista;
    public float tiempoVisible = 3f;
    public bool disparado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player") && !disparado)
        {
            // Usamos la nueva función con tiempo
            DialogueManager.instance.ShowDialogueWithTimer(pista, tiempoVisible);

            disparado = true;
        }
    }
}
