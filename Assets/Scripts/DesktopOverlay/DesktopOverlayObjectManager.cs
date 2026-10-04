using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DesktopOverlayObjectManager : MonoBehaviour
{
    public const string CanonicalSceneObjectName = "png-transparent-soap-bubble-blue-water-bubble-blue-bubble-drop-speech-balloon-color-thumbnail-removebg-preview";
    public List<DesktopMatchRule> rules = new List<DesktopMatchRule>();
    private List<GameObject> activeBubbles = new List<GameObject>();

    public bool enableDiagnosticMode = true;

    [Header("Anti-Clutter & Prioritization Settings")]
    [Tooltip("Enable prioritization of key system icons (This PC, Recycle Bin, Folders) over loose files when cluttered.")]
    public bool prioritizeKeyIcons = true;

    [Tooltip("If the total number of desktop icons found exceeds this threshold, lower-priority icons (loose files) are filtered out.")]
    public int clutterThreshold = 20;

    [Tooltip("Maximum number of bubbles to spawn on screen at any time.")]
    public int maxBubblesToSpawn = 5;

    [Tooltip("Minimum distance in Unity world units between spawned bubbles to prevent overlapping.")]
    public float minBubbleDistance = 0.8f;

    public int TotalFound { get; private set; }
    public int TotalMatched { get; private set; }
    public int TotalSpawned { get; private set; }

    private void Awake()
    {
        // Enforce maximum of 5 bubbles as requested, overriding any old Inspector values
        maxBubblesToSpawn = 5;
    }
    private UnityEngine.SceneManagement.Scene scannedScene;
    private bool hasScannedScene;
    private BubbleManager registeredManager;
    private readonly List<Spawner> registeredSpawners = new List<Spawner>();
    private readonly List<ObjectManager> registeredMonitors = new List<ObjectManager>();
    private Spawner canonicalTriggerSpawner;
    public IReadOnlyList<GameObject> ActiveBubbles => activeBubbles;

    public void RetainOnlyBubble(GameObject bubbleToKeep)
    {
        activeBubbles.RemoveAll(b => b != bubbleToKeep);
    }

    public void RefreshForScene(UnityEngine.SceneManagement.Scene scene)
    {
        // activeSceneChanged can fire before its serialized objects are available.
        // sceneLoaded calls this again once the existing managers and bubble are ready.
        if (!scene.isLoaded) return;
        if (hasScannedScene && scannedScene == scene) return;
        scannedScene = scene;
        hasScannedScene = true;
        RefreshDesktopObjects();
    }

    private void EnsureRules()
    {
        if (rules == null) rules = new List<DesktopMatchRule>();
        EnsureRule(DesktopObjectType.ThisPC);
        EnsureRule(DesktopObjectType.RecycleBin);
        EnsureRule(DesktopObjectType.FileExplorer);
        EnsureRule(DesktopObjectType.Shortcut);
        EnsureRule(DesktopObjectType.Folder);
        EnsureRule(DesktopObjectType.File);
    }

    private void EnsureRule(DesktopObjectType type)
    {
        foreach (var rule in rules)
            if (rule != null && rule.Type == type) return;
        rules.Add(new DesktopMatchRule { Type = type });
    }

    public void RefreshDesktopObjects()
    {
        EnsureRules();
        ClearGeneratedBubbles();
        TotalFound = TotalMatched = TotalSpawned = 0;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Main Fight Area") return;
        // Clean up old

        int totalFound = 0;
        int totalMatched = 0;
        int totalSpawned = 0;

#if UNITY_STANDALONE_WIN
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        GameObject template = Resources.Load<GameObject>("DesktopOverlay/CanonicalBlockingBubble");
        GameObject canonical = null;
        
        // 1. Try to find it in the BubbleManager first (original logic)
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var manager in root.GetComponentsInChildren<BubbleManager>(true))
            {
                if (manager.bigBubbles != null)
                {
                    foreach (var bubble in manager.bigBubbles)
                    {
                        if (bubble != null && bubble.name == CanonicalSceneObjectName)
                        { 
                            registeredManager = manager; 
                            canonical = bubble; 
                        }
                    }
                }
            }
        }
        
        // 2. If not found in manager, just search the entire scene for the hidden object they added back!
        if (canonical == null)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var bubbleScript in root.GetComponentsInChildren<BigBubbleDestroyOnCollision>(true))
                {
                    if (bubbleScript.gameObject.name == CanonicalSceneObjectName)
                    {
                        canonical = bubbleScript.gameObject;
                        break;
                    }
                }
                if (canonical != null) break;
            }
        }

        // 3. If they don't have a Resources prefab, use the scene object as the template!
        if (template == null && canonical != null)
        {
            template = canonical;
        }

        if (canonical == null || template == null)
        {
            Debug.LogError("[DesktopOverlay] Canonical prefab or scene template is missing. Make sure the manual bubble is in the scene! prefab=" 
                + (template != null) + " canonical=" + (canonical != null));
            return;
        }
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var spawner in root.GetComponentsInChildren<Spawner>(true))
            {
                if (spawner.bigBubbles != null && System.Array.IndexOf(spawner.bigBubbles, canonical) >= 0)
                    registeredSpawners.Add(spawner);
                if (spawner.trigger == canonical)
                {
                    if (canonicalTriggerSpawner != null)
                        Debug.LogWarning("[DesktopOverlay] Multiple Spawners use the canonical bubble as their trigger; runtime bubbles will use the first.");
                    else canonicalTriggerSpawner = spawner;
                }
            }
            foreach (var monitor in root.GetComponentsInChildren<ObjectManager>(true))
                if (monitor.ManagesObject(canonical)) registeredMonitors.Add(monitor);
        }
        if (registeredSpawners.Count == 0)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var spawner in root.GetComponentsInChildren<Spawner>(true))
                {
                    registeredSpawners.Add(spawner);
                }
                if (registeredSpawners.Count > 0) break;
            }
        }
        if (canonicalTriggerSpawner == null && registeredSpawners.Count > 0)
        {
            canonicalTriggerSpawner = registeredSpawners[0];
        }
        List<DesktopObject> desktopObjects = WindowsDesktopScanner.ScanDesktop();
        totalFound = desktopObjects.Count;

        // Apply anti-clutter prioritization: prioritize system icons/folders over loose files
        List<DesktopObject> objectsToProcess = FilterAndPrioritizeDesktopObjects(desktopObjects);

        foreach (var obj in objectsToProcess)
        {
            if (enableDiagnosticMode) Debug.Log("[DesktopScanner] " + obj + ", Identity: " + obj.shellIdentity + ", Source: " + obj.sourcePath);
            foreach (var rule in rules)
            {
                if (rule.Matches(obj))
                {
                    totalMatched++;
                    Vector3 unityWorldPos;
                    if (DesktopCoordinateConverter.TryWindowsToUnityWorld(obj.screenPosition,
                        Camera.main, canonical.transform.position.z, out unityWorldPos)
                        && CreateBubbleFor(obj, template, unityWorldPos)) totalSpawned++;
                    break; // Only apply first matching rule
                }
            }

        }
#endif

        TotalFound = totalFound;
        TotalMatched = totalMatched;
        TotalSpawned = totalSpawned;

        if (enableDiagnosticMode)
        {
            Debug.Log($"TOTAL DESKTOP OBJECTS FOUND = {totalFound}");
            Debug.Log($"MATCHED OBJECTS = {totalMatched}");
            Debug.Log($"SPAWNED OVERLAY OBJECTS = {totalSpawned}");
        }
    }

    private List<DesktopObject> FilterAndPrioritizeDesktopObjects(List<DesktopObject> rawObjects)
    {
        if (rawObjects == null || rawObjects.Count == 0) return new List<DesktopObject>();

        // If prioritization is disabled or total icons are within threshold, keep all (capped at maxBubblesToSpawn)
        if (!prioritizeKeyIcons || rawObjects.Count <= clutterThreshold)
        {
            if (maxBubblesToSpawn > 0 && rawObjects.Count > maxBubblesToSpawn)
            {
                return rawObjects.Take(maxBubblesToSpawn).ToList();
            }
            return rawObjects;
        }

        Debug.Log($"[DesktopOverlay] Clutter detected ({rawObjects.Count} icons > threshold {clutterThreshold}). Prioritizing key icons (System icons, Folders)...");

        // Priority ranking: System icons > Folders > Browsers > Shortcuts > Loose Files
        int GetPriority(DesktopObject obj)
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

        // Separate into key items (Priority <= 4) and loose files (Priority 5)
        var keyItems = rawObjects.Where(o => GetPriority(o) <= 4).OrderBy(GetPriority).ToList();

        List<DesktopObject> selected;

        // If we found enough key items (folders, shortcuts, system icons)
        if (keyItems.Count >= 5)
        {
            // Ignore loose files completely to eliminate screen clutter
            selected = keyItems;
        }
        else
        {
            // If there are very few key items on this desktop, include some loose files up to a reasonable count
            selected = keyItems;
            var remainingFiles = rawObjects.Where(o => GetPriority(o) > 4).ToList();
            int needed = Mathf.Min(clutterThreshold, maxBubblesToSpawn) - selected.Count;
            if (needed > 0 && remainingFiles.Count > 0)
            {
                selected.AddRange(remainingFiles.Take(needed));
            }
        }

        // Cap to maxBubblesToSpawn
        if (maxBubblesToSpawn > 0 && selected.Count > maxBubblesToSpawn)
        {
            selected = selected.Take(maxBubblesToSpawn).ToList();
        }

        Debug.Log($"[DesktopOverlay] Filtered down from {rawObjects.Count} to {selected.Count} prioritized icons to prevent screen clutter.");
        return selected;
    }

    private void ClearGeneratedBubbles()
    {
        foreach (var bubble in activeBubbles)
        {
            if (registeredManager != null && registeredManager.bigBubbles != null) 
            {
                registeredManager.bigBubbles.Remove(bubble);
            }
            foreach (var spawner in registeredSpawners)
                if (spawner != null && spawner.bigBubbles != null)
                {
                    var list = new List<GameObject>(spawner.bigBubbles);
                    list.Remove(bubble);
                    spawner.bigBubbles = list.ToArray();
                }
            if (canonicalTriggerSpawner != null) canonicalTriggerSpawner.UnregisterRuntimeTrigger(bubble);
            foreach (var monitor in registeredMonitors)
                if (monitor != null) monitor.UnregisterObject(bubble);
            if (bubble != null) Destroy(bubble);
        }
        activeBubbles.Clear();
        registeredManager = null;
        registeredSpawners.Clear();
        registeredMonitors.Clear();
        canonicalTriggerSpawner = null;
    }

    private bool CreateBubbleFor(DesktopObject obj, GameObject bubbleTemplate, Vector3 unityWorldPos)
    {
        if (bubbleTemplate != null)
        {
            // Enforce minimum distance spacing between bubbles to prevent overlapping
            if (minBubbleDistance > 0f)
            {
                foreach (var existing in activeBubbles)
                {
                    if (existing != null && Vector3.Distance(existing.transform.position, unityWorldPos) < minBubbleDistance)
                    {
                        if (enableDiagnosticMode)
                            Debug.Log($"[DesktopOverlay] Skipping bubble for '{obj.displayName}': too close to existing bubble '{existing.name}'");
                        return false;
                    }
                }
            }

            GameObject bubble = Instantiate(bubbleTemplate, unityWorldPos, bubbleTemplate.transform.rotation);
            bubble.name = "DesktopBubble_" + obj.displayName;
            
            // Ensure the local scale matches the original manually configured bubble
            bubble.transform.localScale = bubbleTemplate.transform.lossyScale;
            
            if (registeredManager != null && registeredManager.bigBubbles != null)
            {
                registeredManager.bigBubbles.Add(bubble);
            }
            
            foreach (var spawner in registeredSpawners)
            {
                if (spawner != null && spawner.bigBubbles != null)
                {
                    var list = new List<GameObject>(spawner.bigBubbles) { bubble };
                    spawner.bigBubbles = list.ToArray();
                    spawner.hasRegisteredAnyBubbles = true;
                }
            }
            foreach (var monitor in registeredMonitors) 
            {
                if (monitor != null) monitor.RegisterObject(bubble);
            }
            
            // The canonical scene bubble is also the serialized trigger for one
            // existing Spawner. Route generated bubbles through that same
            // Spawner action without replacing its manual trigger reference.
            if (canonicalTriggerSpawner != null) canonicalTriggerSpawner.RegisterRuntimeTrigger(bubble);
            bubble.SetActive(true);
            activeBubbles.Add(bubble);
            if (enableDiagnosticMode)
                Debug.Log("[DesktopOverlay] Spawned " + bubble.name + " world=" + unityWorldPos
                    + " spawners=" + registeredSpawners.Count + " monitors=" + registeredMonitors.Count);
            return true;
        }
        else
        {
            Debug.LogError("Could not find the existing png-transparent-soap-bubble in the scene to use as a template.");
        }
        return false;
    }
}
