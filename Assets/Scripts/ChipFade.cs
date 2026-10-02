using UnityEngine;
using System.Collections;


public class ChildObjectHandler : MonoBehaviour
{
    private bool isProcessing = false;

    void Update()
    {
        // Check if the object has no more child objects
        if (!isProcessing && transform.childCount == 0)
        {
            StartCoroutine(HandleObject());
        }
    }

    private IEnumerator HandleObject()
    {
        isProcessing = true;

        // Disable motion by removing Rigidbody (if any) and freezing the object's transform
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Make it static
        }

        // Rotation and scaling down
        float rotationSpeed = 1000f; // Adjust the speed of rotation
        float scaleSpeed = 3f;       // Adjust the speed of scaling
        Vector3 originalScale = transform.localScale;

        while (transform.localScale.x > 0.01f)
        {
            // Rotate the object very fast on the Y-axis
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Scale the object down
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, scaleSpeed * Time.deltaTime);

            yield return null; // Wait for the next frame
        }

        // Ensure the scale is set to zero
        transform.localScale = Vector3.zero;

        // Destroy the object
        Destroy(gameObject);
    }
}
