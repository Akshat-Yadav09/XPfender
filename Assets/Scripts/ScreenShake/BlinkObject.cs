using UnityEngine;

public class BlinkAndDisableCollider : MonoBehaviour
{
    public GameObject targetObject; // Reference to the GameObject you want to blink
    public float blinkInterval = 0.2f; // Time between blinks
    public float blinkDuration = 3f; // Total duration of the blinking

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    void Start()
    {
        // Ensure the target object is set
        if (targetObject == null)
        {
            Debug.LogError("Target object not assigned!");
            return;
        }

        // Get the SpriteRenderer and BoxCollider2D components from the target object
        spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        boxCollider = targetObject.GetComponent<BoxCollider2D>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on the target object!");
            return;
        }

        if (boxCollider == null)
        {
            Debug.LogError("No BoxCollider2D found on the target object!");
            return;
        }

        // Start the blink routine
        StartCoroutine(BlinkRoutine());
    }

    private System.Collections.IEnumerator BlinkRoutine()
    {
        float elapsedTime = 0f;

        // Disable the BoxCollider2D during blinking
        boxCollider.enabled = false;

        while (elapsedTime < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle the SpriteRenderer
            yield return new WaitForSeconds(blinkInterval); // Wait for the blink interval
            elapsedTime += blinkInterval;
        }

        // Ensure the SpriteRenderer and BoxCollider2D are re-enabled after blinking stops
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
    }
}
