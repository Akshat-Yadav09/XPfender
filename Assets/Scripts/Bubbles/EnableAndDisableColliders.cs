using System.Collections;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private GameObject[] objects; // Array of objects to monitor
    private bool isDestructionBlocked = false; // Flag to block further destruction

    private void Start()
    {
        if (objects == null || objects.Length == 0)
        {
            Debug.LogError("No objects assigned to the ObjectManager!");
        }
    }

    private void Update()
    {
        if (isDestructionBlocked) return; // Skip if destruction is currently blocked

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) // If any object is destroyed
            {
                StartBlockingDestruction(i); // Handle the first detected destruction
                break; // Stop checking once a destroyed object is handled
            }
        }
    }

    private void StartBlockingDestruction(int destroyedIndex)
    {
        if (isDestructionBlocked) return; // Skip if already blocked

        isDestructionBlocked = true; // Block further destruction
        Debug.Log("Destruction blocked for 10 seconds!");

        // Disable BoxCollider2D immediately for all objects
        foreach (var obj in objects)
        {
            if (obj != null) // Only disable colliders for existing objects
            {
                BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.enabled = false; // Disable the collider immediately
                    Debug.Log($"Collider on {obj.name} disabled.");
                }
            }
        }

        // Immediately re-enable BoxCollider2D and set it to trigger mode after the block period
        Invoke(nameof(ReEnableColliders), 10f); // Call ReEnableColliders after 10 seconds

        // Remove the destroyed object from the list
        objects = RemoveFromArray(objects, destroyedIndex);
    }

    private void ReEnableColliders()
    {
        // Re-enable BoxCollider2D and set it to trigger mode immediately
        foreach (var obj in objects)
        {
            if (obj != null) // Only re-enable colliders for existing objects
            {
                BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.enabled = true; // Re-enable the collider immediately
                    collider.isTrigger = true; // Enable trigger mode
                    Debug.Log($"Collider on {obj.name} re-enabled and set to trigger.");
                }
            }
        }

        isDestructionBlocked = false; // Allow destruction again
        Debug.Log("Destruction unblocked, objects can be destroyed again!");
    }

    private GameObject[] RemoveFromArray(GameObject[] array, int index)
    {
        if (array == null || index < 0 || index >= array.Length) return array;

        GameObject[] newArray = new GameObject[array.Length - 1];
        int newArrayIndex = 0;

        for (int i = 0; i < array.Length; i++)
        {
            if (i != index) // Skip the element at the given index
            {
                newArray[newArrayIndex++] = array[i];
            }
        }

        return newArray;
    }
}
