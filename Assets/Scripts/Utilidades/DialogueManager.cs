using UnityEngine;
using TMPro; // Necesitas TextMeshPro instalado
using System.Collections;
using UnityEngine.Audio;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;
    
    [Header("Audio")]
    public AudioClip typingSound;


    void Awake()
    {
        if (instance == null) instance = this;
        dialoguePanel.SetActive(false);
    }

    public void ShowDialogue(string message)
    {
        dialoguePanel.SetActive(true);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeMessage(message));
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    public void ShowDialogueWithTimer(string message, float delay)
    {
        ShowDialogue(message);
        StartCoroutine(TimerToClose(delay));
    }

    private IEnumerator TimerToClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseDialogue();
    }

    IEnumerator TypeMessage(string message)
    {
        dialogueText.text = "";
        AudioSource playerAudio = null;
        if (PlayerController.instance != null)
        {
            playerAudio = PlayerController.instance.audioSourceEffects;
        }

        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;

            // Si hay una letra (que no sea espacio) y tenemos el AudioSource del player...
            if (letter != ' ' && playerAudio != null && typingSound != null)
            {
                // Variamos un poco el pitch para que no sea monótono
                playerAudio.pitch = Random.Range(0.9f, 1.1f);
                playerAudio.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        // Resetear el pitch al terminar para no afectar a otros sonidos del player
        if (playerAudio != null) playerAudio.pitch = 1f;
    }
}