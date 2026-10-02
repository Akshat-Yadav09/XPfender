using System.Collections;
using UnityEngine;

public class ColliderChecker : MonoBehaviour
{
    [SerializeField] private BoxCollider2D targetCollider; // Reference to the BoxCollider2D
    [SerializeField] private float checkInterval = 0.1f; // Interval to check if the collider is disabled
    [SerializeField] private float reEnableDelay = 5f; // Delay before re-enabling the collider

    private void Start()
    {
        if (targetCollider == null)
        {
            // Auto-assign the BoxCollider2D if not set
            targetCollider = GetComponent<BoxCollider2D>();
        }

        if (targetCollider == null)
        {
            Debug.LogError("No BoxCollider2D assigned or found on the object!");
            return;
        }

        // Start the checking process
        StartCoroutine(CheckAndReEnableCollider());
    }

    private IEnumerator CheckAndReEnableCollider()
    {
        while (true) // Continuously check
        {
            yield return new WaitForSeconds(checkInterval); // Wait for the defined interval

            if (targetCollider != null && !targetCollider.enabled) // If the collider is disabled
            {
                Debug.Log("Collider is disabled. Re-enabling after delay...");
                yield return new WaitForSeconds(reEnableDelay); // Wait before re-enabling

                if (targetCollider != null) // Double-check the collider still exists
                {
                    targetCollider.enabled = true; // Re-enable the collider
                    Debug.Log("Collider re-enabled.");
                }
            }
        }
    }
}
