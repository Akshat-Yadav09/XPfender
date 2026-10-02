using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectPrefab; // The prefab to spawn (small bubbles or chips)
    public GameObject[] bigBubbles; // Array of big bubbles to manage
    public int maxSpawnCount = 3; // Maximum number of objects to spawn
    public GameObject trigger; // Object used to start spawning
    public GameObject targetSpawner; // The spawner this spawner will move to
    public float moveSpeed = 5f; // Speed of movement to the target point
    public float rotateSpeed = 180f; // Rotation speed during the animation
    public AudioClip moveSound; // Sound effect during movement
    public GameObject targetToDestroy; // Object to destroy when the last spawner reaches the point

    private bool hasStartedSpawning = false; // Flag to check if spawning has started
    private bool isMovingToTarget = false; // Flag to indicate movement to the target point
    private AudioSource audioSource; // AudioSource component
    private Rigidbody2D rb; // Rigidbody2D component (if available)

    private void Start()
    {
        // Ensure big bubbles are assigned
        if (bigBubbles == null || bigBubbles.Length == 0)
        {
            Debug.LogError("Big bubbles array is empty or not assigned!");
            return;
        }

        // Ensure targetSpawner is assigned
        if (targetSpawner == null)
        {
            Debug.LogError("Target spawner is not assigned!");
            return;
        }

        // Disable all big bubbles' colliders initially
        foreach (GameObject bigBubble in bigBubbles)
        {
            if (bigBubble != null)
            {
                BoxCollider2D collider = bigBubble.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        // Set up the audio source for the sound effect
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = moveSound;
        audioSource.loop = true; // Loop the sound while moving

        // Get the Rigidbody2D component if available
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Start spawning objects if the trigger is null and it hasn't started yet
        if (!hasStartedSpawning && trigger == null)
        {
            SpawnAllObjects();
            hasStartedSpawning = true;
        }

        // Manage the colliders of big bubbles dynamically based on the presence of "Chip" objects
        ManageBigBubbleColliders();

        // Check if all big bubbles are destroyed, then start moving this spawner to the target
        CheckAndMoveIfAllBigBubblesDestroyed();

        // Handle movement if flagged
        if (isMovingToTarget)
        {
            MoveToTargetPoint();
        }
    }

    // Spawn all objects (chips) instantly
    private void SpawnAllObjects()
    {
        for (int i = 0; i < maxSpawnCount; i++)
        {
            // Spawn the object at the spawner's position
            Vector2 spawnPosition = new Vector2(transform.position.x, transform.position.y);
            GameObject spawnedObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

            // Set the tag for the spawned object to "Chip"
            spawnedObject.tag = "Chip";

            // Apply a random throw force to the spawned object
            float throwForce = Random.Range(5f, 10f); // Example values
            Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), -throwForce);
            }
        }
    }

    // Dynamically manage the colliders of big bubbles based on the presence of "Chip" objects
    private void ManageBigBubbleColliders()
    {
        // Check if any active objects in the scene have the tag "Chip"
        GameObject[] chips = GameObject.FindGameObjectsWithTag("Chip");
        bool hasActiveChips = chips.Length > 0;

        // Enable or disable the colliders on big bubbles based on the presence of "Chip" objects
        foreach (GameObject bigBubble in bigBubbles)
        {
            if (bigBubble != null)
            {
                BoxCollider2D collider = bigBubble.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.enabled = !hasActiveChips; // Disable colliders if there are active "Chip" objects
                }
            }
        }
    }

    // Trigger the spawner to move to the target point
    public void MoveToPoint()
    {
        isMovingToTarget = true;
        if (audioSource && moveSound)
        {
            audioSource.Play();
        }
    }

    // Move the spawner to the target spawner's position with a rotating animation
    private void MoveToTargetPoint()
    {
        // Check if targetSpawner is valid
        if (targetSpawner == null)
        {
            Debug.LogError("Target spawner is null!");
            return;
        }

        // Get the target position (the position of the target spawner)
        Vector2 targetPosition = targetSpawner.transform.position;

        // Rotate the spawner for the animation
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        // Move the spawner toward the target position
        if (rb != null)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }

        // Check if the spawner has reached the target position
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMovingToTarget = false;
            if (audioSource)
            {
                audioSource.Stop();
            }

            // Destroy the specified target object before destroying this spawner
            if (targetToDestroy != null)
            {
                Destroy(targetToDestroy);
            }

            // Destroy the spawner itself
            Destroy(gameObject);
        }
    }

    // Check if all big bubbles are destroyed and trigger movement
    private void CheckAndMoveIfAllBigBubblesDestroyed()
    {
        bool allDestroyed = true;

        // Check if all elements in the bigBubbles array are null
        foreach (GameObject bigBubble in bigBubbles)
        {
            if (bigBubble != null)
            {
                allDestroyed = false;
                break;
            }
        }

        Debug.Log("All destroyed: " + allDestroyed);

        // If all are destroyed and not already moving, start moving to the target point
        if (allDestroyed && !isMovingToTarget)
        {
            Debug.Log("Triggering movement to target.");
            MoveToPoint();
        }
    }
}
