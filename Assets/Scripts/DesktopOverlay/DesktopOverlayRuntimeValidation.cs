#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

// Opt-in development-player checks. No validation objects exist in normal launches.
public class DesktopOverlayRuntimeValidation : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (!Debug.isDebugBuild || Array.IndexOf(Environment.GetCommandLineArgs(), "--xp-desktop-probe") < 0) return;
        var go = new GameObject("DesktopOverlayRuntimeValidation");
        DontDestroyOnLoad(go);
        go.AddComponent<DesktopOverlayRuntimeValidation>();
    }

    private IEnumerator Start()
    {
        Time.timeScale = 1;
        yield return null;
        yield return null;
        yield return null;
        if (SceneManager.GetActiveScene().name != "Main Fight Area")
            yield return SceneManager.LoadSceneAsync("Main Fight Area");
        yield return null;
        var overlay = FindAnyObjectByType<DesktopOverlayObjectManager>();
        var automatic = GameObject.Find("DesktopBubble_This PC");
        var prefab = Resources.Load<GameObject>("DesktopOverlay/CanonicalBlockingBubble");
        GameObject manual = null;
        var sceneManager = FindAnyObjectByType<BubbleManager>();
        foreach (var bubble in sceneManager.bigBubbles)
            if (bubble != null && bubble.name == DesktopOverlayObjectManager.CanonicalSceneObjectName) manual = bubble;
        int expectedSpawnCount = overlay.TotalMatched;
        Check(overlay.TotalSpawned == expectedSpawnCount,
            "one automatic bubble for every matched shell object, shortcut, folder, and file");
        Check(GameObject.Find("DesktopBubble_Recycle Bin") != null, "Recycle Bin automatic bubble exists");
        Check(automatic != null, "This PC automatic bubble exists");
        foreach (var bubble in overlay.ActiveBubbles)
        {
            Check(sceneManager.bigBubbles.Contains(bubble), bubble.name + " BubbleManager registration");
            foreach (var spawner in FindObjectsByType<Spawner>(FindObjectsInactive.Exclude))
                if (Array.IndexOf(spawner.bigBubbles, manual) >= 0)
                    Check(Array.IndexOf(spawner.bigBubbles, bubble) >= 0, bubble.name + " existing Spawner collider control");
            foreach (var monitor in FindObjectsByType<ObjectManager>(FindObjectsInactive.Exclude))
                if (monitor.ManagesObject(manual)) Check(monitor.ManagesObject(bubble), bubble.name + " destruction monitor registration");
        }
        if (automatic != null)
        {
            Check(automatic.GetComponent<SpriteRenderer>().sprite == manual.GetComponent<SpriteRenderer>().sprite,
                "sprite parity");
            Check(automatic.GetComponent<SpriteRenderer>().sharedMaterial == manual.GetComponent<SpriteRenderer>().sharedMaterial,
                "material parity");
            Check(automatic.GetComponent<BoxCollider2D>().isTrigger == manual.GetComponent<BoxCollider2D>().isTrigger,
                "trigger parity");
            Check(automatic.GetComponent<BoxCollider2D>().size == manual.GetComponent<BoxCollider2D>().size,
                "collider size parity");
            Check(Vector3.Distance(automatic.transform.lossyScale, manual.transform.lossyScale) < 0.00001f,
                "world scale parity");
            Check(automatic.GetComponent<BigBubbleDestroyOnCollision>().destroySound
                == manual.GetComponent<BigBubbleDestroyOnCollision>().destroySound, "sound parity");
            yield return VerifyChipGating(manual, automatic);
            Spawner triggerSpawner = null;
            foreach (var spawner in FindObjectsByType<Spawner>(FindObjectsInactive.Exclude))
                if (spawner.trigger == manual) triggerSpawner = spawner;
            int chipsBeforePop = GameObject.FindGameObjectsWithTag("Chip").Length;
            Destroy(automatic);
            yield return null;
            yield return null;
            int chipsAfterPop = GameObject.FindGameObjectsWithTag("Chip").Length;
            Check(triggerSpawner != null && chipsAfterPop - chipsBeforePop == triggerSpawner.maxSpawnCount,
                "automatic bubble destruction invokes the canonical Spawner chip-spawn action");
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--xp-desktop-hold") >= 0)
                yield return new WaitForSeconds(20); // Allows observing the generated desktop placements.
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) root.SetActive(false);
            yield return VerifyCollision(manual, "manual");
            yield return VerifyCollision(prefab, "automatic");
        }
        Debug.Log("[DesktopValidation] FINISHED");
        Application.Quit(failures == 0 ? 0 : 1);
    }

    private IEnumerator VerifyChipGating(GameObject manual, GameObject automatic)
    {
        var chip = new GameObject("Validation Chip") { tag = "Chip" };
        yield return null;
        yield return null;
        Check(!manual.GetComponent<BoxCollider2D>().enabled && !automatic.GetComponent<BoxCollider2D>().enabled,
            "manual and automatic colliders blocked by existing Spawners while chips exist");
        Destroy(chip);
        // Existing scene chips may remain; compare state instead of forcing gameplay state.
        yield return null;
        yield return null;
        Check(manual.GetComponent<BoxCollider2D>().enabled == automatic.GetComponent<BoxCollider2D>().enabled,
            "manual and automatic collider gating parity after validation chip removal");
    }

    private IEnumerator VerifyCollision(GameObject source, string label)
    {
        Scene test = SceneManager.CreateScene("DesktopCollision" + label,
            new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        GameObject bubble = Instantiate(source, new Vector3(0, 0, 0), Quaternion.identity);
        bubble.transform.localScale = source.transform.lossyScale;
        SceneManager.MoveGameObjectToScene(bubble, test);
        bubble.SetActive(true);
        bubble.GetComponent<BoxCollider2D>().enabled = true; // Isolated control trial, without scene chip gating.
        var managerObject = new GameObject("Validation BubbleManager");
        SceneManager.MoveGameObjectToScene(managerObject, test);
        var manager = managerObject.AddComponent<BubbleManager>();
        manager.bigBubbles = new System.Collections.Generic.List<GameObject> { bubble };
        yield return null; // Run the existing bubble and manager Start methods.
        GameObject player = new GameObject("Validation Player");
        SceneManager.MoveGameObjectToScene(player, test);
        player.tag = "Player";
        player.AddComponent<Rigidbody2D>().gravityScale = 0;
        player.AddComponent<BoxCollider2D>();
        PhysicsScene2D physics = test.GetPhysicsScene2D();
        physics.Simulate(0.02f);
        var count = typeof(BubbleManager).GetField("smallBubblesDestroyed", BindingFlags.Instance | BindingFlags.NonPublic);
        Check((int)count.GetValue(manager) == 1, label + " real Player trigger notified existing BubbleManager");
        AudioClip sound = source.GetComponent<BigBubbleDestroyOnCollision>().destroySound;
        var audio = bubble != null ? bubble.GetComponent<AudioSource>() : null;
        Check(sound == null || (audio != null && audio.isPlaying), label + " existing AudioSource started destruction sound");
        yield return new WaitForSeconds((sound != null ? sound.length : 0) + 0.1f);
        Check(bubble == null, label + " existing destruction path completed");
        yield return SceneManager.UnloadSceneAsync(test);
    }

    private static int failures;
    private static void Check(bool pass, string label)
    {
        if (!pass) failures++;
        Debug.Log("[DesktopValidation] " + (pass ? "PASS " : "FAIL ") + label);
    }
}
#endif
