using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BoxColliderClickHandler : MonoBehaviour
{
    // Example operation: Set a target GameObject active
    public GameObject targetObject;
    public GameObject targetObject1;
    public GameObject targetObject2;
    public GameObject targetObject3;
    public GameObject targetObject4;
    public GameObject targetObject5;
    public GameObject targetObject6;
    public GameObject targetObject7; // Assign the object to activate in the Inspector
    
    public GameObject targetObject8; // Assign the object to activate in the Inspector
    
    public GameObject targetObject9; // Assign the object to activate in the Inspector
    
    public GameObject targetObject10; // Assign the object to activate in the Inspector
    
    public GameObject targetObject11;
    
    public GameObject targetObject12; // Assign the object to activate in the Inspector

    public GameObject targetObject13; // Assign the object to activate in the Inspector

    public GameObject Victory;

    [SerializeField] private float delay = 0.5f;

    [Header("Desktop Mapping")]
    [Tooltip("Adjust this if the bubbles are slightly offset from the real desktop icons.")]
    public Vector2 desktopMappingOffset = Vector2.zero;

    private static List<DesktopObject> cachedDesktopObjects = null;
    private static int nextDesktopObjectIndex = 0;
    private static string lastSceneName = "";
    private static int totalMappedIcons = 0;

    private static int GetPriority(DesktopObject obj)
    {
        switch (obj.type)
        {
            case DesktopObjectType.ThisPC:
            case DesktopObjectType.RecycleBin:
            case DesktopObjectType.FileExplorer:
                return 1;
            case DesktopObjectType.Folder:
                return 2;
            case DesktopObjectType.Browser:
                return 3;
            case DesktopObjectType.Shortcut:
                return 4;
            case DesktopObjectType.File:
            default:
                return 5;
        }
    }

    private void Start()
    {
#if UNITY_STANDALONE_WIN
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        Camera cam = Camera.main;
        if (cam == null) cam = FindAnyObjectByType<Camera>();

        // Reset cache if we loaded a new scene
        if (cachedDesktopObjects == null || lastSceneName != currentScene)
        {
            cachedDesktopObjects = WindowsDesktopScanner.ScanDesktop();
            
            // Clean up list by filtering out garbage
            if (cachedDesktopObjects != null)
            {
                cachedDesktopObjects.RemoveAll(o => string.IsNullOrEmpty(o.displayName));
                
                // Sort the list so This PC, Recycle Bin, and Folders are at the very beginning!
                cachedDesktopObjects.Sort((a, b) => GetPriority(a).CompareTo(GetPriority(b)));
            }

            nextDesktopObjectIndex = 0;
            totalMappedIcons = 0;
            lastSceneName = currentScene;
        }

        // Enforce maximum of 5 icons
        if (totalMappedIcons >= 5)
        {
            if (!Application.isEditor) gameObject.SetActive(false);
            return;
        }

        if (cachedDesktopObjects != null)
        {
            bool successfullyMapped = false;

            // Keep iterating through desktop objects until we find one that successfully maps to the screen
            while (nextDesktopObjectIndex < cachedDesktopObjects.Count && !successfullyMapped)
            {
                DesktopObject obj = cachedDesktopObjects[nextDesktopObjectIndex++];
                
                if (cam != null)
                {
                    // Use z = 0f as a safe default for 2D objects if the transform's Z is weird
                    float zDepth = transform.position.z != cam.transform.position.z ? transform.position.z : 0f;

                    if (DesktopCoordinateConverter.TryWindowsToUnityWorld(obj.screenPosition, cam, zDepth, out Vector3 unityWorldPos))
                    {
                        // Apply the manual offset to fix any pivot issues
                        transform.position = unityWorldPos + (Vector3)desktopMappingOffset;
                        successfullyMapped = true;
                        totalMappedIcons++;
                        
                        Debug.Log($"[BoxColliderClickHandler] Mapped {gameObject.name} to Desktop Icon '{obj.displayName}' at {transform.position}");
                    }
                }
            }

            // If we checked all icons and couldn't map this one, hide it
            if (!successfullyMapped && !Application.isEditor)
            {
                gameObject.SetActive(false);
            }
        }
#endif
    }

    private IEnumerator OnMouseDown()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {

            if (targetObject != null)
            {
                targetObject.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject1.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject2.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject3.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject4.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject5.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject6.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject7.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject8.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject9.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject10.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject11.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject12.SetActive(true);

                yield return new WaitForSeconds(2f);
                targetObject13.SetActive(true);
            

            }
        }
    }
}

