using UnityEngine;

public class BigBubbleDestroyOnCollision : MonoBehaviour
{
    public AudioClip destroySound; // Assign the sound effect in the inspector
    private AudioSource audioSource;
    private BubbleManager bubbleManager; // Reference to the BubbleManager

    private void Start()
    {
        // Get or add an AudioSource component to play the sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get reference to BubbleManager
        bubbleManager = FindObjectOfType<BubbleManager>();
        if (bubbleManager == null)
        {
            Debug.LogError("No BubbleManager found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // 2D collision
    {
        // Check if the other GameObject has the tag "Player"
        if (other.CompareTag("Player"))
        {
            // Play the sound effect
            if (destroySound != null)
            {
                audioSource.PlayOneShot(destroySound);
            }

            // Debug.Log($"{gameObject.name} collided with {other.gameObject.name}");

            // Notify the BubbleManager that a small bubble has been destroyed
            if (bubbleManager != null)
            {
                bubbleManager.OnSmallBubbleDestroyed(); // Track the destruction of a small bubble
            }

            // Destroy the GameObject after the sound finishes playing
            Destroy(gameObject, destroySound != null ? destroySound.length : 0);
        }
    }
}
