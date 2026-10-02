using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10; // Damage dealt to the player
    public float speed = 10f; // Speed at which the projectile travels
    private Rigidbody2D rb; // Reference to the Rigidbody2D component

    // Reference to the player's position
    private Transform player;

    private void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();

        // Find the player (ensure it's tagged "Player" in the scene)
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null && rb != null)
        {
            // Calculate direction towards the player
            Vector2 direction = (player.position - transform.position).normalized;

            // Apply force to the Rigidbody2D to move the projectile towards the player
            rb.linearVelocity = direction * speed;

            // Destroy the projectile after 10 seconds
            Destroy(gameObject, 10f);
        }
        else
        {
            Debug.LogWarning("Player or Rigidbody2D not found!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Deal damage to the player here (e.g., reduce health)
            Debug.Log("Player hit by projectile!");

            // Destroy the projectile on impact
            Destroy(gameObject);
        }
    }
}
