using UnityEngine;

public class LogoPart : MonoBehaviour
{
    public bool IsDestroyed { get; private set; } = false; // Tracks if this part is destroyed
    public float chaseSpeed = 2f; // Speed at which the part chases the player
    public float stopDistance = 1.5f; // Distance at which the part stops chasing the player
    public Transform player; // Reference to the player
    public GameObject projectilePrefab; // The projectile prefab to fire
    public float fireRate = 2f; // Rate at which projectiles are fired (in seconds)
    public float projectileSpeed = 10f; // Speed of the projectiles
    public Transform firePoint; // Point from where the projectiles will be fired

    public float scaleTime = 1.04f; // Time after which scale and collider will change
    private float scaleTimer = 0f; // Timer for scaling

    private float timeSinceLastFire = 0f; // Timer to track firing intervals

    private Collider2D logoCollider; // Reference to the collider of the logo

    private void Start()
    {
        IsDestroyed = false;

        // Try to find the player object by its tag
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Get the collider component
        logoCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!IsDestroyed)
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player")?.transform;

            }
            ChasePlayer();
            timeSinceLastFire += Time.deltaTime;

            // Fire projectiles at the specified rate
            if (timeSinceLastFire >= fireRate)
            {
                FireProjectile();
                timeSinceLastFire = 0f; // Reset timer
            }

            // Handle the scale change and collider disabling after the specified time
            if (scaleTimer >= scaleTime)
            {
                ScaleLogo();
                EnableCollider();
            }
            else
            {
                scaleTimer += Time.deltaTime; // Increase the scale timer
                DisableCollider(); // Disable collider during the first 1.04 seconds
            }
        }
    }

    public void StartAnimation()
    {
        // Trigger shape-shifting or other animation logic here
        Debug.Log($"{gameObject.name} is animating and chasing the player!");
        IsDestroyed = false; // Reset the destroyed state
    }

    private void ChasePlayer()
    {
        // Calculate distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stopDistance)
        {
            // Move towards the player
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * chaseSpeed * Time.deltaTime;
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // Instantiate the projectile at the fire point
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Get the direction towards the player and set the projectile's velocity
            Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>(); // Use Rigidbody2D for 2D physics
            if (projectileRb != null)
            {
                projectileRb.linearVelocity = directionToPlayer * projectileSpeed;
            }

            // Log the firing action (optional)
            Debug.Log($"{gameObject.name} fired a projectile towards the player!");
        }
        else
        {
            Debug.LogWarning("Projectile prefab or fire point is missing!");
        }
    }

    public void DestroyPart()
    {
        // Logic to destroy the part (e.g., health system or player attack)
        Debug.Log($"{gameObject.name} has been destroyed!");
        IsDestroyed = true;
        gameObject.SetActive(false); // Deactivate this logo part

        // The next logo part will be activated by LogoManager, so we don't need to handle that here
    }

    // Detect collision with player in 2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DestroyPart();
        }
    }

    private void ScaleLogo()
    {
        // Double the scale of the logo part on the X and Y axes
        transform.localScale = new Vector3(2f, 2f, 1f);
        Debug.Log($"{gameObject.name} has scaled up!");
    }

    private void DisableCollider()
    {
        if (logoCollider != null)
        {
            logoCollider.enabled = false; // Disable the collider during the first 1.04 seconds
        }
    }

    private void EnableCollider()
    {
        if (logoCollider != null)
        {
            logoCollider.enabled = true; // Enable the collider after 1.04 seconds
        }
    }
}
