using UnityEngine;
using System.Collections;

public class BossPhaseTwoController : MonoBehaviour
{
    public static BossPhaseTwoController Instance { get; private set; }

    private enum BossState { Dormant, Hovering, Telegraphing, Dashing, Returning }
    private BossState currentState = BossState.Dormant;

    [Header("Phase Setup")]
    [Tooltip("How many crosses the player needs to collect to defeat Phase 2.")]
    public int crossesToDefeat = 3;
    private int currentCrossesCollected = 0;
    private Collider2D bossCollider;
    private bool isPhaseActive = false;

    [Header("Hover & Roam Settings")]
    public float hoverSpeed = 2f;
    public float hoverAmplitude = 1.5f;
    public float roamSpeed = 1f;
    [Tooltip("The bottom-left bounds of the roaming area")]
    public Vector2 roamAreaMin = new Vector2(-6f, -3f);
    [Tooltip("The top-right bounds of the roaming area")]
    public Vector2 roamAreaMax = new Vector2(6f, 3f);

    private Vector2 targetRoamPos;
    private Vector2 baseRoamPos; // To track the actual position without the sine hover added
    private float hoverTime;

    [Header("Dash Attack Settings")]
    [Tooltip("Time between each dash attack")]
    public float timeBetweenAttacks = 4f;
    public float telegraphDuration = 1.5f;
    public float dashSpeed = 30f;
    public float returnSpeed = 12f;
    public AudioClip dashSound;

    [Header("Edge Telegraph Panels")]
    [Tooltip("Assign the red transparent UI panels or sprites here")]
    public GameObject topWarningPanel;
    public GameObject bottomWarningPanel;
    public GameObject leftWarningPanel;
    public GameObject rightWarningPanel;

    [Header("Prefabs")]
    public GameObject shockwavePrefab;
    public GameObject crossPrefab;

    [Header("Sans Attack Spawning")]
    public int numberOfBars = 7;
    public float barSpacing = 1.5f;

    private float attackTimer;
    private GameObject currentCross;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        bossCollider = GetComponent<Collider2D>();
        if (bossCollider != null)
        {
            bossCollider.enabled = false; // Disabled until Phase 2 starts
        }
        HideAllWarnings();
    }

    public void StartPhaseTwo()
    {
        if (isPhaseActive) return;
        isPhaseActive = true;

        if (bossCollider != null)
        {
            bossCollider.enabled = true; // Enable the collider for Phase 2 as requested
        }

        currentState = BossState.Hovering;
        baseRoamPos = transform.position;
        targetRoamPos = GetRandomRoamPosition();
        attackTimer = timeBetweenAttacks;

        Debug.Log("<color=green>[Boss Phase 2] Phase 2 Started!</color>");
        SpawnCross();
    }

    private void Update()
    {
        if (!isPhaseActive) return;

        switch (currentState)
        {
            case BossState.Hovering:
                HandleHoverAndRoaming();

                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    StartCoroutine(AttackRoutine());
                }
                break;

            case BossState.Telegraphing:
                // Still roam/hover in position during telegraph (as requested)
                HandleHoverAndRoaming();
                break;

            case BossState.Dashing:
                // Movement is handled entirely in the Coroutine
                break;

            case BossState.Returning:
                // Movement is handled entirely in the Coroutine
                break;
        }
    }

    private void HandleHoverAndRoaming()
    {
        hoverTime += Time.deltaTime;
        float hoverOffset = Mathf.Sin(hoverTime * hoverSpeed) * hoverAmplitude;

        // Move the base position towards the target
        baseRoamPos = Vector2.MoveTowards(baseRoamPos, targetRoamPos, roamSpeed * Time.deltaTime);

        // If we reached the roam pos, pick a new one
        if (Vector2.Distance(baseRoamPos, targetRoamPos) < 0.1f)
        {
            targetRoamPos = GetRandomRoamPosition();
        }

        // Apply base position + hover offset
        transform.position = new Vector3(baseRoamPos.x, baseRoamPos.y + hoverOffset, transform.position.z);
    }

    private Vector2 GetRandomRoamPosition()
    {
        Vector2 camPos = Camera.main != null ? (Vector2)Camera.main.transform.position : Vector2.zero;
        return new Vector2(
            camPos.x + Random.Range(roamAreaMin.x, roamAreaMax.x),
            camPos.y + Random.Range(roamAreaMin.y, roamAreaMax.y)
        );
    }

    private IEnumerator AttackRoutine()
    {
        currentState = BossState.Telegraphing;

        // Pick a random edge (0=Top, 1=Bottom, 2=Left, 3=Right)
        int edgeIndex = Random.Range(0, 4);
        GameObject warningPanel = null;
        Vector2 dashTargetPos = Vector2.zero;
        Vector2 shockwaveDir = Vector2.zero;
        Quaternion shockwaveRot = Quaternion.identity;

        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector2 camCenter = cam.transform.position;

        // Calculate positions dynamically based on screen size
        switch (edgeIndex)
        {
            case 0: // Top
                warningPanel = topWarningPanel;
                dashTargetPos = new Vector2(transform.position.x, camCenter.y + camHeight + 1.5f);
                shockwaveDir = Vector2.down;
                shockwaveRot = Quaternion.Euler(0, 0, 90);
                break;
            case 1: // Bottom
                warningPanel = bottomWarningPanel;
                dashTargetPos = new Vector2(transform.position.x, camCenter.y - camHeight - 1.5f);
                shockwaveDir = Vector2.up;
                shockwaveRot = Quaternion.Euler(0, 0, -90);
                break;
            case 2: // Left
                warningPanel = leftWarningPanel;
                dashTargetPos = new Vector2(camCenter.x - camWidth - 1.5f, transform.position.y);
                shockwaveDir = Vector2.right;
                shockwaveRot = Quaternion.Euler(0, 0, 0);
                break;
            case 3: // Right
                warningPanel = rightWarningPanel;
                dashTargetPos = new Vector2(camCenter.x + camWidth + 1.5f, transform.position.y);
                shockwaveDir = Vector2.left;
                shockwaveRot = Quaternion.Euler(0, 0, 180);
                break;
        }

        // Flashing logic for the warning panel
        float blinkTimer = 0f;
        bool isPanelVisible = true;
        float timePassed = 0f;

        if (warningPanel != null) warningPanel.SetActive(true);

        while (timePassed < telegraphDuration)
        {
            timePassed += Time.deltaTime;
            blinkTimer += Time.deltaTime;

            // Toggle panel every 0.15 seconds to create a flashing effect
            if (blinkTimer >= 0.15f)
            {
                blinkTimer = 0f;
                isPanelVisible = !isPanelVisible;
                if (warningPanel != null) warningPanel.SetActive(isPanelVisible);
            }
            yield return null;
        }

        if (warningPanel != null) warningPanel.SetActive(false);

        // Transition to Dash State
        currentState = BossState.Dashing;

        if (dashSound != null) AudioSource.PlayClipAtPoint(dashSound, transform.position);

        // Dash rapidly to the edge
        while (Vector2.Distance(transform.position, dashTargetPos) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, dashTargetPos, dashSpeed * Time.deltaTime);
            yield return null;
        }

        // Boss has impacted the edge! Generate the Shockwave (Sans Attack)
        if (shockwavePrefab != null)
        {
            // Determine the perpendicular direction to spawn the wall of bars
            Vector2 perpDir = (shockwaveDir.x == 0) ? Vector2.right : Vector2.up;

            // Calculate edge length and dynamically spawn enough bars to cover it completely!
            float edgeLength = (shockwaveDir.x == 0) ? (camWidth * 2f) : (camHeight * 2f);
            edgeLength += 4f; // extra padding to ensure it covers the extreme corners
            int dynamicBars = Mathf.Max(numberOfBars, Mathf.CeilToInt(edgeLength / barSpacing));

            for (int i = 0; i < dynamicBars; i++)
            {
                // Calculate offset from the center impact point
                float offset = (i - (dynamicBars - 1) / 2f) * barSpacing;
                Vector2 spawnPos = dashTargetPos + (perpDir * offset);

                GameObject wave = Instantiate(shockwavePrefab, spawnPos, shockwaveRot);

                BossShockwave waveScript = wave.GetComponent<BossShockwave>();
                if (waveScript != null)
                {
                    // Pass direction so it knows which way to thrust inward
                    waveScript.Initialize(shockwaveDir);
                }
            }
        }

        // Return gracefully to a new random roam position instead of dead center
        currentState = BossState.Returning;
        targetRoamPos = GetRandomRoamPosition();

        // Eased return using Lerp for a smoother deceleration
        while (Vector2.Distance(transform.position, targetRoamPos) > 0.5f)
        {
            transform.position = Vector2.Lerp(transform.position, targetRoamPos, returnSpeed * 0.5f * Time.deltaTime);
            yield return null;
        }

        // IMPORTANT: Sync the base position so it doesn't snap when hovering resumes!
        baseRoamPos = transform.position;

        // Reset hover time so the sine wave starts at 0 (no sudden vertical jumping)
        hoverTime = 0f;

        // Reset variables for the next cycle
        attackTimer = timeBetweenAttacks;
        currentState = BossState.Hovering;
    }

    private void HideAllWarnings()
    {
        if (topWarningPanel != null) topWarningPanel.SetActive(false);
        if (bottomWarningPanel != null) bottomWarningPanel.SetActive(false);
        if (leftWarningPanel != null) leftWarningPanel.SetActive(false);
        if (rightWarningPanel != null) rightWarningPanel.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlaneController2D player = collision.gameObject.GetComponent<PlaneController2D>();
            if (player != null)
            {
                player.TriggerDestruction();
                Debug.Log("<color=red>Player killed by Boss collision!</color>");
            }
        }
    }

    private void SpawnCross()
    {
        if (crossPrefab != null)
        {
            Vector2 randomSpawnPos = GetRandomRoamPosition();
            currentCross = Instantiate(crossPrefab, randomSpawnPos, Quaternion.identity);

            BossTargetCross crossScript = currentCross.GetComponent<BossTargetCross>();
            if (crossScript != null)
            {
                crossScript.Initialize(this);
            }
        }
    }

    public void OnCrossCollected()
    {
        currentCrossesCollected++;
        Debug.Log($"<color=cyan>[Boss Phase 2] Cross collected! Progress: {currentCrossesCollected} / {crossesToDefeat}</color>");

        if (currentCrossesCollected >= crossesToDefeat)
        {
            EndPhaseTwo();
        }
        else
        {
            // Spawn the next cross after a short 1 second delay
            Invoke(nameof(SpawnCross), 1f);
        }
    }

    private void EndPhaseTwo()
    {
        Debug.Log("<color=green>[Boss Phase 2] Phase 2 Defeated!</color>");

        // Stop all attacks and clear warnings
        StopAllCoroutines();
        HideAllWarnings();
        isPhaseActive = false;
        currentState = BossState.Dormant;

        // Clean up the remaining cross
        if (currentCross != null) Destroy(currentCross);

        if (bossCollider != null) bossCollider.enabled = false;

        StartCoroutine(DeathSequenceRoutine());
    }

    private System.Collections.IEnumerator DeathSequenceRoutine()
    {
        // Instantly hide the boss graphics so they don't overlap the dialogue box
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // Just yield one frame to be safe
        yield return null;

        string[] outroLines = new string[]
        {
            "<link=\"shake\">NO! THIS IS IMPOSSIBLE!</link>",
            "My beautiful virus...",
            "Deleting system32... just kidding.",
            "<link=\"shake\">I'm fading... aaaaaah!</link>"
        };

        if (BossIntroDialogue.Instance != null)
        {
            BossIntroDialogue.Instance.PlayCustomDialogue(outroLines, () =>
            {
                FinishDeathSequence();
            });
        }
        else
        {
            FinishDeathSequence();
        }
    }

    private void FinishDeathSequence()
    {
        // Hide all planes so they disappear
        PlaneController2D[] allPlanes = FindObjectsByType<PlaneController2D>(FindObjectsInactive.Include);
        foreach (var plane in allPlanes)
        {
            if (plane != null) plane.gameObject.SetActive(false);
        }

        // Show Victory Screen
        var victory = FindAnyObjectByType<CheckObjectsDestroyed>();
        if (victory != null)
        {
            victory.EnableUI();
        }

        // Destroy the boss
        Destroy(gameObject);
    }
}
