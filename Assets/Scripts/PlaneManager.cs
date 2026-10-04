using UnityEngine;

public class PlaneManager : MonoBehaviour
{
    public static PlaneManager Instance; // Singleton instance
    private PlaneController2D[] allPlanes;
    private int currentPlaneIndex = 0;
    
    public int TotalPlanes => allPlanes != null ? allPlanes.Length : 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the manager alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Cache all planes at the start, including inactive ones
        allPlanes = FindObjectsByType<PlaneController2D>(FindObjectsInactive.Include);

        // Ensure planes are ordered by index
        System.Array.Sort(allPlanes, (a, b) => a.planeIndex.CompareTo(b.planeIndex));

        // Disable all planes so they don't collide with each other at 0,0
        foreach (var plane in allPlanes)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(false);
            }
        }

        // Enable the first plane
        ActivatePlane(0);
    }

    public void ActivateNextPlane()
    {
        // Disable the current plane and set its tag to default
        if (currentPlaneIndex < allPlanes.Length && allPlanes[currentPlaneIndex] != null)
        {
            allPlanes[currentPlaneIndex].EnableControl(false);
            // Set the current plane tag to default or another tag
            allPlanes[currentPlaneIndex].gameObject.tag = "Untagged"; // Or use another tag if needed
        }

        // Move to the next plane
        currentPlaneIndex++;
        if (currentPlaneIndex < allPlanes.Length)
        {
            ActivatePlane(currentPlaneIndex);
        }
        else
        {
            Debug.LogWarning("No more planes to activate.");
        }
    }

    private void ActivatePlane(int index)
    {
        if (index < 0 || index >= allPlanes.Length || allPlanes[index] == null)
        {
            Debug.LogError("Invalid plane index.");
            return;
        }

        currentPlaneIndex = index;
        
        // Turn the GameObject on!
        allPlanes[index].gameObject.SetActive(true);
        
        allPlanes[index].EnableControl(true);
        // Set the active plane's tag to "Player"
        allPlanes[index].gameObject.tag = "Player"; // Set the Player tag

        Debug.Log($"Plane {index} activated.");
    }

    public int GetCurrentPlaneIndex()
    {
        return currentPlaneIndex;
    }
}
