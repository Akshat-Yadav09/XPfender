using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BigBubbleDestroyOnCollision : MonoBehaviour
{
    public AudioClip destroySound; // Assign the sound effect in the inspector
    private AudioSource audioSource;
    private BubbleManager bubbleManager; // Reference to the BubbleManager

    [Header("Radius Pop Settings")]
    [Tooltip("If other bubbles are within this radius (in Unity units), they pop together with this bubble.")]
    public float nearbyPopRadius = 1.6f;

    public static float globalPopRadius = 1.6f;

    [HideInInspector]
    public bool isPopping = false;

    private void Start()
    {
        // Get or add an AudioSource component to play the sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get reference to BubbleManager
        bubbleManager = FindAnyObjectByType<BubbleManager>();
    }

    private void OnTriggerEnter2D(Collider2D other) // 2D collision
    {
        // Check if the other GameObject has the tag "Player"
        if (other.CompareTag("Player") && !isPopping)
        {
            // If active chips exist in the scene or boss fight has already started,
            // bubbles cannot be popped!
            if (Spawner.HasActiveChips() || Spawner.isBossFightStarted)
            {
                BoxCollider2D col = GetComponent<BoxCollider2D>();
                if (col != null) col.enabled = false;
                return;
            }

            PopWithNearbyRadius();
        }
    }

    public void PopWithNearbyRadius()
    {
        if (isPopping) return;

        // Double check chip lockout
        if (Spawner.HasActiveChips() || Spawner.isBossFightStarted)
        {
            BoxCollider2D c = GetComponent<BoxCollider2D>();
            if (c != null) c.enabled = false;
            return;
        }

        isPopping = true;

        float radius = nearbyPopRadius > 0f ? nearbyPopRadius : globalPopRadius;

        // Find all other active big bubbles within radius
        BigBubbleDestroyOnCollision[] all = FindObjectsByType<BigBubbleDestroyOnCollision>(FindObjectsInactive.Exclude);
        if (all != null)
        {
            var nearby = all
                .Where(b => b != null && b != this && !b.isPopping && b.gameObject.activeInHierarchy
                    && Vector2.Distance(transform.position, b.transform.position) <= radius)
                .ToList();

            // Pop any neighbor bubbles in radius
            foreach (var neighbor in nearby)
            {
                neighbor.PopDirect();
            }

            // Immediately turn off colliders on ALL other remaining bubbles in scene
            // so the player cannot activate or pop them all at once!
            foreach (var otherBubble in all)
            {
                if (otherBubble != null && !otherBubble.isPopping)
                {
                    BoxCollider2D col = otherBubble.GetComponent<BoxCollider2D>();
                    if (col != null) col.enabled = false;
                }
            }
        }

        // Pop this bubble
        PopDirect();

        // Check remaining bubbles: if none left, InitiateBossSequence(); if bubbles still remain, spawn chips and lock out other bubbles!
        if (Spawner.AreAnyBigBubblesRemainingInScene())
        {
            Spawner.OnAnyBubblePopped(transform.position);
        }
        else
        {
            Spawner.InitiateBossSequence();
        }
    }

    public void PopDirect()
    {
        if (this == null || gameObject == null) return;
        isPopping = true;

        // Disable collider immediately so it cannot trigger multiple times
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null) collider.enabled = false;

        // Hide sprite immediately for crisp visual pop response
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // Play the sound effect
        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound);
        }

        // Notify the BubbleManager that a bubble has been destroyed
        if (bubbleManager != null)
        {
            bubbleManager.OnSmallBubbleDestroyed();
        }

        // Destroy the GameObject after the sound finishes playing
        float delay = (destroySound != null && audioSource != null) ? destroySound.length : 0f;
        Destroy(gameObject, delay);
    }

    public static void CheckIfAllBubblesCleared()
    {
        if (!Spawner.AreAnyBigBubblesRemainingInScene())
        {
            Spawner.InitiateBossSequence();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, nearbyPopRadius > 0 ? nearbyPopRadius : globalPopRadius);
    }
#endif
}
