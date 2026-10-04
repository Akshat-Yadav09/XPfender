using System.Collections.Generic;
using UnityEngine;

public class BubbleManager : MonoBehaviour
{
    public List<GameObject> bigBubbles; // List of all big bubbles
    private int currentBubbleIndex = 0; // Tracks which big bubble can currently be destroyed
    private int smallBubblesDestroyed = 0; // Counter for small bubbles destroyed
    private bool collidersDisabled = false; // Flag to check if colliders are temporarily disabled

    private void Start()
    {
        // Ensure all bubbles are properly initialized
        if (bigBubbles == null || bigBubbles.Count == 0)
        {
            Debug.LogWarning("BubbleManager: No big bubbles assigned in the list.");
            return;
        }

        // Allow up to 3 big bubbles to have enabled colliders at start
        for (int i = 3; i < bigBubbles.Count; i++)
        {
            if (bigBubbles[i] != null)
            {
                BoxCollider2D collider = bigBubbles[i].GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
    }

    // Call this method when a small bubble is destroyed
    public void OnSmallBubbleDestroyed()
    {
        if (collidersDisabled)
        {
            Debug.Log("Colliders are temporarily disabled. Ignoring small bubble destruction.");
            return;
        }

        smallBubblesDestroyed++;

        // Once 5 small bubbles are destroyed, allow the next big bubble to be destroyed
        if (smallBubblesDestroyed >= 5)
        {
            // Enable the collider for the next big bubble
            if (currentBubbleIndex < bigBubbles.Count && bigBubbles[currentBubbleIndex] != null)
            {
                BoxCollider2D nextCollider = bigBubbles[currentBubbleIndex].GetComponent<BoxCollider2D>();
                if (nextCollider != null)
                {
                    nextCollider.enabled = true;
                }
            }
        }
    }

    // Call this method to try destroying the big bubble
    public void TryDestroyBigBubble(GameObject bubble)
    {
        if (collidersDisabled)
        {
            Debug.Log("Cannot destroy bubbles while colliders are temporarily disabled.");
            return;
        }

        // Check if the bubble matches the current one in the destruction order
        if (currentBubbleIndex < bigBubbles.Count && bigBubbles[currentBubbleIndex] == bubble)
        {
            Destroy(bubble);
            bigBubbles[currentBubbleIndex] = null; // Remove the reference to the destroyed bubble
            currentBubbleIndex++; // Move to the next bubble in the list
            smallBubblesDestroyed = 0; // Reset small bubble counter
            Debug.Log($"Big bubble destroyed: {bubble.name}");

            // Enable the collider for the next big bubble
            if (currentBubbleIndex < bigBubbles.Count && bigBubbles[currentBubbleIndex] != null)
            {
                BoxCollider2D nextCollider = bigBubbles[currentBubbleIndex].GetComponent<BoxCollider2D>();
                if (nextCollider != null)
                {
                    nextCollider.enabled = true;
                }
            }

            // Check if all bubbles have been cleared
            if (currentBubbleIndex >= bigBubbles.Count)
            {
                Debug.Log("All big bubbles have been destroyed!");
            }
        }
        else
        {
            Debug.Log($"Cannot destroy bubble: {bubble.name}. Destroy bubbles in order!");
        }
    }
}
