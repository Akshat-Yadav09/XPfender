using UnityEngine;

public class OscillatingObject : MonoBehaviour
{
    [Header("Oscillation Settings")]
    public float speed = 2f; // Speed of oscillation

    private Transform[] patrolPoints; // Array to store points with tag "Patrol"
    private Vector3 currentTarget; // Current target position

    private void Start()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("Patrol");

        if (points.Length < 2)
        {
            Debug.LogError("At least two GameObjects with the 'Patrol' tag are required!");
            return;
        }

        patrolPoints = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            patrolPoints[i] = points[i].transform;
        }

        SelectNewTarget();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.isTrigger = true;
    }

    private void Update()
    {
        if (patrolPoints == null || patrolPoints.Length < 2) return;

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
        {
            SelectNewTarget();
        }
    }

    private void SelectNewTarget()
    {
        Transform randomPoint;
        do
        {
            randomPoint = patrolPoints[Random.Range(0, patrolPoints.Length)];
        }
        while (randomPoint.position == currentTarget);

        currentTarget = randomPoint.position;
    }
}
