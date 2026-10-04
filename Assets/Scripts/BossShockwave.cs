using UnityEngine;

public class BossShockwave : MonoBehaviour
{
    [Header("Shockwave Settings")]
    public float moveSpeed = 10f;
    public float lifeTime = 5f;
    public int damage = 1;

    [Header("Sans Attack Settings")]
    public float thrustDistance = 3f;
    public float thrustSpeed = 15f;
    public float holdTime = 1f;

    [Header("Effects (Optional)")]
    public AudioClip spawnSound;

    public void Initialize(Vector2 direction)
    {
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, transform.position);
        }

        // Start the Sans-like thrust animation
        StartCoroutine(SansThrustAnimation(direction.normalized));
    }

    private System.Collections.IEnumerator SansThrustAnimation(Vector2 direction)
    {
        Vector3 startPos = transform.position;
        // Thrust inwards
        Vector3 targetPos = startPos + (Vector3)(direction * thrustDistance);

        // Slide In
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, thrustSpeed * Time.deltaTime);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(holdTime);

        // Slide Out
        while (Vector3.Distance(transform.position, startPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, thrustSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlaneController2D player = collision.GetComponent<PlaneController2D>();
            if (player != null)
            {
                player.TriggerDestruction();
                Debug.Log("<color=red>Player killed by Shockwave bar!</color>");
            }
        }
    }
}
