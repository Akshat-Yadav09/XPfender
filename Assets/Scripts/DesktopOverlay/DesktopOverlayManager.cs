using UnityEngine;
using UnityEngine.SceneManagement;

public class DesktopOverlayManager : MonoBehaviour
{
    private static DesktopOverlayManager instance;
    private Camera mainCamera;
    private CameraClearFlags originalClearFlags;
    private Color originalBackgroundColor;
    private bool isTransparentModeActive = false;

    // Scenes that should have a transparent background
    private string[] targetSceneNames = { "Antivirus", "BuggedWindows", "Main Fight Area", "Last Scene Wining", "AshKaScene" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("DesktopOverlayManager");
            instance = go.AddComponent<DesktopOverlayManager>();
            DontDestroyOnLoad(go);
            
            Application.runInBackground = true;
        }
    }

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScene(scene);
    }

    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        CheckScene(next);
    }

    private void CheckScene(Scene activeScene)
    {
        bool isTarget = false;
        foreach (string sceneName in targetSceneNames)
        {
            if (activeScene.name == sceneName)
            {
                isTarget = true;
                break;
            }
        }

        if (isTarget)
        {
            if (!isTransparentModeActive)
            {
                EnableOverlay();
            }
            // Transparent-to-transparent transitions still require a new scene scan.
            GetComponent<DesktopOverlayObjectManager>().RefreshForScene(activeScene);
        }
        else
        {
            if (isTransparentModeActive)
            {
                DisableOverlay();
            }
        }
    }

    private void EnableOverlay()
    {
        isTransparentModeActive = true;
        WindowsWindowController.EnableTransparentWindow();
        UpdateCamera(true);
        
        var objectManager = GetComponent<DesktopOverlayObjectManager>();
        if (objectManager == null)
        {
            objectManager = gameObject.AddComponent<DesktopOverlayObjectManager>();
        }
        objectManager.enabled = true;
    }

    private void DisableOverlay()
    {
        isTransparentModeActive = false;
        WindowsWindowController.DisableTransparentWindow();
        UpdateCamera(false);
        
        var objectManager = GetComponent<DesktopOverlayObjectManager>();
        if (objectManager != null)
        {
            objectManager.enabled = false;
            // Optionally clear existing bubbles when leaving transparent scenes
        }
    }

    private bool originalPostProcessingState;

    private void UpdateCamera(bool isTransparent)
    {
        if (!isTransparent)
        {
            // Best effort restore for the cached mainCamera when disabling
            if (mainCamera != null)
            {
                mainCamera.clearFlags = originalClearFlags;
                mainCamera.backgroundColor = originalBackgroundColor;

                var urpData = mainCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                if (urpData != null)
                {
                    urpData.renderPostProcessing = originalPostProcessingState;
                }
            }
        }
    }

    void Update()
    {
        if (isTransparentModeActive)
        {
            // Force ALL cameras in the scene to be transparent continuously
            Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
            foreach (Camera cam in allCameras)
            {
                if (cam.clearFlags != CameraClearFlags.SolidColor || cam.backgroundColor.a > 0)
                {
                    // Cache the original properties of the main camera in case it's a persistent camera
                    if (cam == Camera.main && mainCamera == null)
                    {
                        mainCamera = cam;
                        originalClearFlags = cam.clearFlags;
                        originalBackgroundColor = cam.backgroundColor;
                        var mainUrpData = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        if (mainUrpData != null) originalPostProcessingState = mainUrpData.renderPostProcessing;
                    }

                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = new Color(0, 0, 0, 0); // Transparent black

                    var urpData = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                    if (urpData != null && urpData.renderPostProcessing)
                    {
                        urpData.renderPostProcessing = false;
                    }
                }
            }
        }
    }
}
