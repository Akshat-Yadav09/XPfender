using System.Collections;
using UnityEngine;

public class LogoManager : MonoBehaviour
{
    public LogoPart[] logoParts; // Array of all the parts of the XP logo
    public GameObject[] firstPartObjects; // Array of first objects for each logo part
    public AudioClip blinkSound; // Sound effect for the blink
    public Transform player; // Reference to the player
    public float blinkDuration = 0.1f; // Duration for the blink effect

    private AudioSource audioSource; // AudioSource to play the sound

    private void Start()
    {
        // Get the AudioSource component to play sounds
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            // Debug.LogError("AudioSource not found on this GameObject.");
        }

        // Assign the player reference to each logo part
        foreach (LogoPart part in logoParts)
        {
            part.player = player;
        }

        // Start the coroutine to animate logo parts
        StartCoroutine(AnimateLogoParts());
    }

    private IEnumerator AnimateLogoParts()
    {
        for (int i = 0; i < logoParts.Length; i++)
        {
            LogoPart part = logoParts[i];
            GameObject firstPartObject = firstPartObjects[i]; // Get the corresponding first object

            // Activate the first object for the part
            if (firstPartObject != null)
            {

                // Blink the sprite by toggling its active state
                StartCoroutine(BlinkObject(firstPartObject));

                // Play the blink sound effect
                if (audioSource != null && blinkSound != null)
                {
                    audioSource.PlayOneShot(blinkSound);
                }
                else
                {
                    Debug.LogWarning("Blink sound is missing or AudioSource is not set.");
                }
            }
            else
            {
                Debug.LogWarning("First object is not assigned for " + part.name);
            }

            // Deactivate all other parts (only keep the current part active)
            foreach (LogoPart otherPart in logoParts)
            {
                otherPart.gameObject.SetActive(false);
            }

            // Activate the current part and start its animation
            part.gameObject.SetActive(true);

            // Destroy the first part object before starting the animation (set it inactive)
            if (firstPartObject != null)
            {
                firstPartObject.SetActive(false); // Disable the first object
            }

            // Start animation for the current part
            part.StartAnimation();

            // Wait until the current logo part is destroyed
            while (!part.IsDestroyed)
            {
                yield return null; // Wait until the condition is met
            }
        }
    }

    // Coroutine to handle blinking the sprite
    private IEnumerator BlinkObject(GameObject obj)
    {
        // Disable the object briefly to create the blink effect
        obj.SetActive(false);
        yield return new WaitForSeconds(blinkDuration); // Wait for the blink duration
        obj.SetActive(true); // Enable the object again
    }
}
