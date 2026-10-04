using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectPrefab; // The prefab to spawn (small bubbles or chips)
    public GameObject[] bigBubbles; // Array of big bubbles to manage
    public int maxSpawnCount = 3; // Maximum number of objects to spawn
    public GameObject trigger; // Object used to start spawning
    public GameObject targetSpawner; // The spawner this spawner will move to
    public float moveSpeed = 5f; // Speed of movement to the target point
    public float rotateSpeed = 180f; // Rotation speed during the animation
    public AudioClip moveSound; // Sound effect during movement
    public GameObject targetToDestroy; // Object to destroy when the last spawner reaches the point

    public bool hasRegisteredAnyBubbles = false;
    public static bool isCheatPopping = false;
    public static bool isBossFightStarted = false;
    private static int lastCheatExecutionFrame = -1;

    private bool hasStartedSpawning = false; // Flag to check if spawning has started
    private bool isMovingToTarget = false; // Flag to indicate movement to the target point
    private AudioSource audioSource; // AudioSource component
    private Rigidbody2D rb; // Rigidbody2D component (if available)
    private readonly List<GameObject> runtimeTriggers = new List<GameObject>();

    private void Start()
    {
        isBossFightStarted = false;

        // Check if any non-null bubbles were assigned in the inspector
        if (bigBubbles != null)
        {
            foreach (var b in bigBubbles)
            {
                if (b != null)
                {
                    hasRegisteredAnyBubbles = true;
                    break;
                }
            }
        }

        // Disable all big bubbles' colliders initially
        if (bigBubbles != null)
        {
            foreach (GameObject bigBubble in bigBubbles)
            {
                if (bigBubble != null)
                {
                    BoxCollider2D collider = bigBubble.GetComponent<BoxCollider2D>();
                    if (collider != null)
                    {
                        collider.enabled = false;
                    }
                }
            }
        }

        // Set up the audio source for the sound effect
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = moveSound;
        audioSource.loop = true; // Loop the sound while moving

        // Get the Rigidbody2D component if available
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Check for cheat code input
        CheckCheatCodeInput();

        // Start spawning initial chips only if big bubbles exist, boss fight has not started, and no chips exist yet
        if (!hasStartedSpawning && trigger == null && !isBossFightStarted && hasRegisteredAnyBubbles && AreAnyBigBubblesRemainingInScene())
        {
            hasStartedSpawning = true;
            if (!HasActiveChips())
            {
                SpawnAllObjects();
            }
        }

        // Clean up runtime triggers list as bubbles are destroyed
        for (int i = runtimeTriggers.Count - 1; i >= 0; i--)
        {
            if (runtimeTriggers[i] == null)
            {
                runtimeTriggers.RemoveAt(i);
            }
        }

        // Manage the colliders of big bubbles dynamically based on the presence of "Chip" objects
        ManageBigBubbleColliders();

        // Check if all big bubbles are destroyed, then start moving this spawner to the target
        CheckAndMoveIfAllBigBubblesDestroyed();

        // Handle movement if flagged
        if (isMovingToTarget)
        {
            MoveToTargetPoint();
        }
    }

    public static bool HasActiveChips()
    {
        GameObject[] chips = GameObject.FindGameObjectsWithTag("Chip");
        return chips != null && chips.Length > 0;
    }

    public static void DisableAllBigBubbleCollidersInScene()
    {
        BigBubbleDestroyOnCollision[] all = FindObjectsByType<BigBubbleDestroyOnCollision>(FindObjectsInactive.Exclude);
        if (all != null)
        {
            foreach (var b in all)
            {
                if (b != null)
                {
                    BoxCollider2D col = b.GetComponent<BoxCollider2D>();
                    if (col != null) col.enabled = false;
                }
            }
        }
    }

    public static void OnAnyBubblePopped(Vector3 popPosition)
    {
        if (isBossFightStarted || isCheatPopping) return;

        // Immediately turn off colliders on all remaining bubbles in the scene
        DisableAllBigBubbleCollidersInScene();

        // If this pop destroyed the last remaining bubble(s), initiate the boss sequence
        if (!AreAnyBigBubblesRemainingInScene())
        {
            InitiateBossSequence();
            return;
        }

        // Spawn chips from the most appropriate Spawner
        Spawner[] spawners = FindObjectsByType<Spawner>(FindObjectsInactive.Exclude);
        if (spawners != null && spawners.Length > 0)
        {
            Spawner chosen = spawners
                .Where(s => s != null && s.gameObject.activeInHierarchy && !s.isMovingToTarget)
                .OrderBy(s => Vector2.Distance(popPosition, s.transform.position))
                .FirstOrDefault();

            if (chosen == null)
            {
                chosen = spawners[0];
            }

            if (chosen != null)
            {
                chosen.SpawnAllObjects();
            }
        }
    }

    public static void InitiateBossSequence()
    {
        if (isBossFightStarted) return;

        // Switch to Boss Music immediately to allow buildup during the text box dialogue
        if (BossMusicManager.Instance != null)
        {
            BossMusicManager.Instance.PlayBossMusic();
        }

        var dialogue = BossIntroDialogue.Instance ?? FindAnyObjectByType<BossIntroDialogue>(FindObjectsInactive.Include);
        if (dialogue != null && !dialogue.hasPlayed)
        {
            if (!dialogue.gameObject.activeInHierarchy)
            {
                dialogue.gameObject.SetActive(true);
            }

            dialogue.StartDialogue(() => {
                TriggerBossFight();
            });
        }
        else
        {
            TriggerBossFight();
        }
    }

    public static void TriggerBossFight()
    {
        if (isBossFightStarted) return;
        isBossFightStarted = true;

        Debug.Log("<color=green>[Boss Fight] All bubbles cleared! Triggering Boss Fight...</color>");

        // 1. Destroy BossFightPause to trigger EnableOnTriggerDestroy.OnDestroy()
        GameObject pause = GameObject.Find("BossFightPause");
        if (pause != null)
        {
            Destroy(pause);
        }

        // 2. Failsafe: directly ensure the Boss root GameObject is enabled
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "Boss")
            {
                root.SetActive(true);
            }
        }

        // 3. Clear any leftover chips so they don't linger during the boss fight
        GameObject[] chips = GameObject.FindGameObjectsWithTag("Chip");
        foreach (var chip in chips)
        {
            if (chip != null) Destroy(chip);
        }

        // 4. Trigger spawner movement animation if targetSpawner exists
        Spawner[] allSpawners = FindObjectsByType<Spawner>(FindObjectsInactive.Exclude);
        foreach (var s in allSpawners)
        {
            if (s != null && s.targetSpawner != null && !s.isMovingToTarget)
            {
                s.MoveToPoint();
            }
        }
    }

    public static bool AreAnyBigBubblesRemainingInScene()
    {
        BigBubbleDestroyOnCollision[] bubbles = FindObjectsByType<BigBubbleDestroyOnCollision>(FindObjectsInactive.Exclude);
        if (bubbles == null) return false;
        foreach (var b in bubbles)
        {
            if (b != null && !b.isPopping) return true;
        }
        return false;
    }

    public bool AreAllBigBubblesDestroyed()
    {
        if (!hasRegisteredAnyBubbles || bigBubbles == null || bigBubbles.Length == 0) return false;

        bool foundAnyValidBubble = false;
        foreach (var b in bigBubbles)
        {
            if (b != null)
            {
                foundAnyValidBubble = true;
                var comp = b.GetComponent<BigBubbleDestroyOnCollision>();
                if (comp == null || !comp.isPopping) return false;
            }
        }
        return foundAnyValidBubble;
    }

    private void CheckCheatCodeInput()
    {
        bool cheatTriggered = Input.GetKeyDown(KeyCode.F9)
            || ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.B))
            || ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.B))
            || (Input.GetKey(KeyCode.B) && Input.GetKeyDown(KeyCode.P));

        if (cheatTriggered)
        {
            ExecuteInstantBossSequenceCheat();
        }
    }

    private static bool hasF9TriggeredPhase1 = false;

    public static void ExecuteInstantBossSequenceCheat()
    {
        if (Time.frameCount == lastCheatExecutionFrame) return;
        lastCheatExecutionFrame = Time.frameCount;

        if (!hasF9TriggeredPhase1)
        {
            hasF9TriggeredPhase1 = true;
            isCheatPopping = true;

            // 1. Clear all big bubbles in the scene
            BigBubbleDestroyOnCollision[] bubbles = FindObjectsByType<BigBubbleDestroyOnCollision>(FindObjectsInactive.Exclude);
            if (bubbles != null)
            {
                foreach (var b in bubbles)
                {
                    if (b != null)
                    {
                        b.isPopping = true;
                        Destroy(b.gameObject);
                    }
                }
            }

            // 2. Clear all chips
            GameObject[] chips = GameObject.FindGameObjectsWithTag("Chip");
            if (chips != null)
            {
                foreach (var chip in chips)
                {
                    if (chip != null) Destroy(chip);
                }
            }

            // 3. Reset all Spawners' triggers
            Spawner[] spawners = FindObjectsByType<Spawner>(FindObjectsInactive.Exclude);
            if (spawners != null)
            {
                foreach (var s in spawners)
                {
                    if (s != null)
                    {
                        s.bigBubbles = new GameObject[0];
                        s.runtimeTriggers.Clear();
                    }
                }
            }

            isCheatPopping = false;
            isBossFightStarted = false;

            Debug.Log("<color=cyan>[F9 Shortcut] Initiating Boss Sequence with Text Box Dialogue...</color>");

            // 4. Reset dialogue hasPlayed flag so F9 can be used repeatedly to test the dialogue
            var dialogue = BossIntroDialogue.Instance ?? FindAnyObjectByType<BossIntroDialogue>(FindObjectsInactive.Include);
            if (dialogue != null)
            {
                dialogue.hasPlayed = false;
                if (!dialogue.gameObject.activeInHierarchy)
                {
                    dialogue.gameObject.SetActive(true);
                }
            }

            // 5. Initiate the boss sequence (dialogue -> boss fight)
            InitiateBossSequence();
        }
        else
        {
            Debug.Log("<color=cyan>[F9 Shortcut] Initiating Boss Phase 2 Directly!</color>");
            
            // Hide the Phase 1 Boss if it's there
            var victoryScript = FindAnyObjectByType<CheckObjectsDestroyed>();
            if (victoryScript != null && victoryScript.logoManager != null)
            {
                foreach (var part in victoryScript.logoManager.logoParts)
                {
                    if (part != null) part.gameObject.SetActive(false);
                }
            }

            if (BossPhaseTwoController.Instance != null)
            {
                BossPhaseTwoController.Instance.StartPhaseTwo();
                
                // Immediately start the boss music
                var musicManager = FindAnyObjectByType<BossMusicManager>();
                if (musicManager != null)
                {
                    musicManager.PlayBossMusic();
                }
            }
        }
    }

    public static void ExecutePopAllLeavingOneCheat()
    {
        if (Time.frameCount == lastCheatExecutionFrame) return;
        lastCheatExecutionFrame = Time.frameCount;

        isCheatPopping = true;

        List<GameObject> allBubbles = new List<GameObject>();

        // 1. Gather all big bubbles from all Spawners
        Spawner[] spawners = FindObjectsByType<Spawner>(FindObjectsInactive.Exclude);
        foreach (var s in spawners)
        {
            if (s != null && s.bigBubbles != null)
            {
                foreach (var b in s.bigBubbles)
                {
                    if (b != null && !allBubbles.Contains(b))
                    {
                        allBubbles.Add(b);
                    }
                }
            }
        }

        // 2. Also check any BigBubbleDestroyOnCollision in scene
        BigBubbleDestroyOnCollision[] bubbleScripts = FindObjectsByType<BigBubbleDestroyOnCollision>(FindObjectsInactive.Exclude);
        foreach (var bs in bubbleScripts)
        {
            if (bs != null && bs.gameObject != null && !allBubbles.Contains(bs.gameObject))
            {
                allBubbles.Add(bs.gameObject);
            }
        }

        if (allBubbles.Count <= 1)
        {
            Debug.Log("[CHEAT] Only " + allBubbles.Count + " bubble(s) in scene. Nothing to pop!");
            isCheatPopping = false;
            return;
        }

        // 3. Find closest bubble to player to leave alive so user can immediately touch it
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject bubbleToKeep = null;
        if (player != null)
        {
            bubbleToKeep = allBubbles
                .OrderBy(b => Vector2.Distance(player.transform.position, b.transform.position))
                .FirstOrDefault();
        }
        if (bubbleToKeep == null)
        {
            bubbleToKeep = allBubbles[0];
        }

        // 4. Clear all existing active Chips so colliders won't be gated
        GameObject[] chips = GameObject.FindGameObjectsWithTag("Chip");
        foreach (var chip in chips)
        {
            if (chip != null) Destroy(chip);
        }

        // 5. Update all Spawners to prevent spawning chips and track only the remaining bubble
        foreach (var s in spawners)
        {
            if (s != null)
            {
                s.ResetTriggersForCheat(bubbleToKeep);
            }
        }

        // 6. Pop all other bubbles
        int popped = 0;
        foreach (var b in allBubbles)
        {
            if (b != null && b != bubbleToKeep)
            {
                var comp = b.GetComponent<BigBubbleDestroyOnCollision>();
                if (comp != null && comp.destroySound != null)
                {
                    AudioSource.PlayClipAtPoint(comp.destroySound, b.transform.position);
                }
                Destroy(b);
                popped++;
            }
        }

        // 7. Ensure remaining bubble is active with its collider enabled
        if (bubbleToKeep != null)
        {
            bubbleToKeep.SetActive(true);
            BoxCollider2D col = bubbleToKeep.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.enabled = true;
                col.isTrigger = true;
            }
        }

        // 8. Update DesktopOverlayObjectManager if present
        var overlay = FindAnyObjectByType<DesktopOverlayObjectManager>();
        if (overlay != null)
        {
            overlay.RetainOnlyBubble(bubbleToKeep);
        }

        // 9. Update ObjectManager if present
        ObjectManager[] oms = FindObjectsByType<ObjectManager>(FindObjectsInactive.Exclude);
        foreach (var om in oms)
        {
            if (om != null) om.ResetForCheat(bubbleToKeep);
        }

        // 10. Update BubbleManager if present
        BubbleManager bm = FindAnyObjectByType<BubbleManager>();
        if (bm != null && bm.bigBubbles != null)
        {
            bm.bigBubbles.Clear();
            bm.bigBubbles.Add(bubbleToKeep);
        }

        isCheatPopping = false;

        Debug.Log($"<color=green>[CHEAT ACTIVATED] Successfully popped {popped} bubbles! Exactly 1 bubble left ('{bubbleToKeep.name}' closest to player). Pop it to begin the boss fight!</color>");
    }

    public void ResetTriggersForCheat(GameObject singleBubble)
    {
        runtimeTriggers.Clear();
        if (singleBubble != null)
        {
            runtimeTriggers.Add(singleBubble);
        }
        bigBubbles = singleBubble != null ? new GameObject[] { singleBubble } : new GameObject[0];
        hasRegisteredAnyBubbles = singleBubble != null;
    }

    public void RegisterRuntimeTrigger(GameObject bubble)
    {
        if (bubble != null && !runtimeTriggers.Contains(bubble))
            runtimeTriggers.Add(bubble);
    }

    public void UnregisterRuntimeTrigger(GameObject bubble)
    {
        runtimeTriggers.Remove(bubble);
    }

    // Spawn all objects (chips) instantly
    private void SpawnAllObjects()
    {
        // Never spawn chips if boss fight started, cheating, or all bubbles are gone
        if (isBossFightStarted || isCheatPopping || !AreAnyBigBubblesRemainingInScene())
        {
            return;
        }

        for (int i = 0; i < maxSpawnCount; i++)
        {
            // Spawn the object at the spawner's position
            Vector2 spawnPosition = new Vector2(transform.position.x, transform.position.y);
            Camera cam = Camera.main;
            if (cam != null)
            {
                float randomX = Random.Range(cam.ScreenToWorldPoint(new Vector3(0, 0, 0)).x + 1f, cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - 1f);
                float randomY = Random.Range(cam.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 1f, cam.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y - 1f);
                spawnPosition = new Vector2(randomX, randomY);
            }
            GameObject spawnedObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

            // Set the tag for the spawned object to "Chip"
            spawnedObject.tag = "Chip";

            // Apply a random throw force to the spawned object
            float throwForce = Random.Range(5f, 10f);
            Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), -throwForce);
            }
        }
    }

    // Dynamically manage the colliders of big bubbles based on the presence of "Chip" objects
    private void ManageBigBubbleColliders()
    {
        bool hasActiveChips = HasActiveChips();

        // Whenever chips exist or boss fight has started, ALL big bubbles have colliders turned OFF
        // so the player cannot activate or pop other bubbles until chips are cleared!
        BigBubbleDestroyOnCollision[] allSceneBubbles = FindObjectsByType<BigBubbleDestroyOnCollision>(FindObjectsInactive.Exclude);
        if (allSceneBubbles != null)
        {
            foreach (var b in allSceneBubbles)
            {
                if (b != null && !b.isPopping)
                {
                    BoxCollider2D col = b.GetComponent<BoxCollider2D>();
                    if (col != null)
                    {
                        col.enabled = !hasActiveChips && !isBossFightStarted;
                    }
                }
            }
        }

        // Also update bigBubbles array references if assigned
        if (bigBubbles != null)
        {
            foreach (GameObject bigBubble in bigBubbles)
            {
                if (bigBubble != null)
                {
                    var comp = bigBubble.GetComponent<BigBubbleDestroyOnCollision>();
                    if (comp != null && comp.isPopping) continue;

                    BoxCollider2D collider = bigBubble.GetComponent<BoxCollider2D>();
                    if (collider != null)
                    {
                        collider.enabled = !hasActiveChips && !isBossFightStarted;
                    }
                }
            }
        }
    }

    // Trigger the spawner to move to the target point
    public void MoveToPoint()
    {
        isMovingToTarget = true;
        if (audioSource && moveSound)
        {
            audioSource.Play();
        }
    }

    // Move the spawner to the target spawner's position with a rotating animation
    private void MoveToTargetPoint()
    {
        // If targetSpawner is not assigned, directly trigger the boss fight and cleanup
        if (targetSpawner == null)
        {
            TriggerBossFight();
            isMovingToTarget = false;
            Destroy(gameObject);
            return;
        }

        // Get the target position (the position of the target spawner)
        Vector2 targetPosition = targetSpawner.transform.position;

        // Rotate the spawner for the animation
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        // Move the spawner toward the target position
        if (rb != null)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }

        // Check if the spawner has reached the target position
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMovingToTarget = false;
            if (audioSource)
            {
                audioSource.Stop();
            }

            // Trigger the boss fight upon arrival
            TriggerBossFight();

            // Destroy the spawner itself
            Destroy(gameObject);
        }
    }

    // Check if all big bubbles are destroyed and trigger movement
    private void CheckAndMoveIfAllBigBubblesDestroyed()
    {
        if (!hasRegisteredAnyBubbles) return;

        bool allDestroyed = AreAllBigBubblesDestroyed();

        // If all are destroyed and not already moving, initiate boss sequence
        if (allDestroyed && !isMovingToTarget && !isBossFightStarted)
        {
            InitiateBossSequence();
        }
    }
}
