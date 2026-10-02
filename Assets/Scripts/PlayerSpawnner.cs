using UnityEngine;

public class TeleportToTopRightCorner : MonoBehaviour
{
    public Camera mainCamera; // Reference to the Camera
    public Transform objectToTeleport; // Reference to the object you want to teleport
    public float xOffset = -1f; // Offset to push the object slightly to the left
    public float yOffset = 0f;  // Optional y-offset if needed

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use the main camera if no reference is set
        }

        TeleportToCorner();
    }

    void TeleportToCorner()
    {
        // Get the top-right corner in world space
        Vector3 topRightCorner = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        // Apply the offset
        topRightCorner.x += xOffset;
        topRightCorner.y += yOffset;

        // Set the object's position to that point
        objectToTeleport.position = topRightCorner;
    }
}
