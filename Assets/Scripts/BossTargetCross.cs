using UnityEngine;

public class BossTargetCross : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Sound played when the player collides with the cross.")]
    public AudioClip collectSound;

    [Tooltip("Optional particle effect to spawn when collected.")]
    public GameObject collectEffectPrefab;

    private BossPhaseTwoController bossController;

    public void Initialize(BossPhaseTwoController controller)
    {
        bossController = controller;
    }

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;
        
        // Disable collider so player doesn't hit it again
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(MoveToBossRoutine());
    }

    private System.Collections.IEnumerator MoveToBossRoutine()
    {
        if (bossController != null)
        {
            float speed = 20f; // Speed at which the cross flies to the boss
            
            // Animate flying towards the boss
            while (bossController != null && Vector3.Distance(transform.position, bossController.transform.position) > 0.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, bossController.transform.position, speed * Time.deltaTime);
                yield return null;
            }

            // Tell the boss it was hit
            if (bossController != null)
            {
                bossController.OnCrossCollected();
            }
        }

        // Play sound - spawned at the Camera's position so it's always max volume (not distant)
        if (collectSound != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(collectSound, soundPos, 1.0f);
        }

        // Spawn effect at impact location
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        // Destroy the cross
        Destroy(gameObject);
    }
}
