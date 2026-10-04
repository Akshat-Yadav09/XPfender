using System.Collections;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private GameObject[] objects; // Array of objects to monitor
    [SerializeField] private int allowedDestructionsBeforeBlock = 3; // Allow up to 3 destructions before blocking
    private int destructionCount = 0;
    private bool isDestructionBlocked = false; // Flag to block further destruction

    // Runtime registration uses the same existing destruction/collider handling.
    public bool ManagesObject(GameObject obj)
    {
        return objects != null && System.Array.IndexOf(objects, obj) >= 0;
    }

    public void RegisterObject(GameObject obj)
    {
        if (obj == null || ManagesObject(obj)) return;
        int count = objects != null ? objects.Length : 0;
        System.Array.Resize(ref objects, count + 1);
        objects[count] = obj;
    }

    public void UnregisterObject(GameObject obj)
    {
        if (objects == null) return;
        int index = System.Array.IndexOf(objects, obj);
        if (index >= 0) objects = RemoveFromArray(objects, index);
    }

    public void ResetForCheat(GameObject singleRemainingObject)
    {
        CancelInvoke(nameof(ReEnableColliders));
        isDestructionBlocked = false;
        destructionCount = 0;
        objects = singleRemainingObject != null ? new GameObject[] { singleRemainingObject } : new GameObject[0];
        if (singleRemainingObject != null)
        {
            BoxCollider2D collider = singleRemainingObject.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.enabled = true;
                collider.isTrigger = true;
            }
        }
    }

    private void Start()
    {
        if (objects == null || objects.Length == 0)
        {
            Debug.LogWarning("No objects assigned to the ObjectManager!");
        }
    }

    private void Update()
    {
        if (isDestructionBlocked) return; // Skip if destruction is currently blocked

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) // If any object is destroyed
            {
                objects = RemoveFromArray(objects, i);
                destructionCount++;
                if (destructionCount >= allowedDestructionsBeforeBlock)
                {
                    StartBlockingDestruction();
                }
                break; // Stop checking once a destroyed object is handled
            }
        }
    }

    private void StartBlockingDestruction()
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
    }

    private void ReEnableColliders()
    {
        // If chips are currently active or boss fight has started, delay re-enabling until chips are cleared
        if (Spawner.HasActiveChips() || Spawner.isBossFightStarted)
        {
            Invoke(nameof(ReEnableColliders), 0.5f);
            return;
        }

        destructionCount = 0;
        // Re-enable BoxCollider2D and set it to trigger mode immediately
        foreach (var obj in objects)
        {
            if (obj != null) // Only re-enable colliders for existing objects
            {
                var comp = obj.GetComponent<BigBubbleDestroyOnCollision>();
                if (comp != null && comp.isPopping) continue;

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
