using UnityEngine;

public class TeleportToTopLeft : MonoBehaviour
{
    public Camera mainCamera; // Reference to the Camera
    public Transform objectToTeleport; // Reference to the object you want to teleport

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use the main camera if no reference is set
        }

        TeleportToTopLeftCorner();
    }

    void TeleportToTopLeftCorner()
    {
        // Get the top-left corner in world space
        Vector3 topLeftCorner = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane));

        // Set the object's position to that point
        objectToTeleport.position = topLeftCorner;
    }
}
