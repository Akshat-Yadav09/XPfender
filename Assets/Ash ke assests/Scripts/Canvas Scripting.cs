using System.Collections;
using UnityEngine;

public class CanvasScripting : MonoBehaviour
{
    [SerializeField] private GameObject TeleCanvas;
    [SerializeField] private GameObject WindowsCanvas;

    [SerializeField] private GameObject Tele;
    [SerializeField] private float canvasSwitchDelay = 2f; // Duration of the delay for canvas switch

    [SerializeField] private AudioSource audioSource; // Reference to the AudioSource component
    [SerializeField] private AudioClip buttonClickSound; // Reference to the audio clip for the button click sound

    [Header("Delays")]
    [SerializeField] public float soundDelay = 0f; // Duration of the delay before the sound plays, made public

    public void OnClick()
    {
        StartCoroutine(HandleButtonClick()); // Handle both sound and canvas switching with delay
    }

    private IEnumerator HandleButtonClick()
    {
        // Optionally delay the sound first
        if (soundDelay > 0f)
        {
            yield return new WaitForSeconds(soundDelay);
        }

        PlayButtonClickSound(); // Play the sound after the delay

        // Now delay the canvas switching
        yield return StartCoroutine(SwitchCanvasWithDelay());
    }

    private IEnumerator SwitchCanvasWithDelay()
    {
        WindowsCanvas.SetActive(false); // Hide Windows Canvas

        Tele.SetActive(true); // Show Tele GameObject

        yield return new WaitForSeconds(canvasSwitchDelay); // Wait for the specified delay

        Tele.SetActive(false); // Hide Tele GameObject

        TeleCanvas.SetActive(true); // Show TeleCanvas
    }

    private void PlayButtonClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.clip = buttonClickSound; // Assign the sound clip
            audioSource.Play(); // Play the sound after the delay
        }
        else
        {
            Debug.LogError("AudioSource or ButtonClickSound is not assigned!");
        }
    }
}
