using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BossIntroDialogue : MonoBehaviour
{
    public static BossIntroDialogue Instance { get; private set; }

    [Header("UI & Graphic References")]
    [Tooltip("The TextMeshPro text component to display dialogue. If left empty, auto-detects from this object or its children.")]
    public TMP_Text dialogueText;

    [Tooltip("UI Image background box (recommended for Canvas). If left empty, auto-detects from this object or its children.")]
    public UnityEngine.UI.Image backgroundImage;

    [Tooltip("UI RawImage background box. If left empty, auto-detects from this object or its children.")]
    public UnityEngine.UI.RawImage backgroundRawImage;

    [Tooltip("The SpriteRenderer background box (for world space). If left empty, auto-detects from this object or its children.")]
    public SpriteRenderer backgroundSprite;

    [Tooltip("The parent/root GameObject of the dialogue UI to show/hide. Defaults to this GameObject.")]
    public GameObject dialogueContainer;

    [Tooltip("Optional text to show when waiting for player to advance, e.g. '[Click / Space to continue]'")]
    public TMP_Text continuePromptText;

    [Header("Dialogue Content (Option A)")]
    [TextArea(2, 5)]
    public string[] dialogueLines = new string[]
    {
        "Oh my god... how could you?!",
        "Do you have any idea how many hours it took to infect this registry?!",
        "Those bubbles were my emotional support firewalls!",
        "Fine. Task Manager won't save you now. Prepare for the Blue Screen of Death!"
    };

    [Header("Typewriter Settings")]
    [Tooltip("Time in seconds between each character typing out.")]
    public float typingSpeed = 0.035f;

    [Tooltip("Use unscaled time so typing works even if the game is paused.")]
    public bool useUnscaledTime = true;

    [Tooltip("Pause the game timescale during dialogue.")]
    public bool pauseGameDuringDialogue = false;

    [Header("Audio (Optional)")]
    public AudioClip popupSound;
    public AudioClip typeSound;
    public AudioClip finishLineSound;
    private AudioSource audioSource;

    [Header("Typing Audio Variation")]
    [Tooltip("Minimum pitch for typing click variation (e.g. 0.85).")]
    [Range(0.5f, 2.0f)]
    public float minPitch = 0.85f;

    [Tooltip("Maximum pitch for typing click variation (e.g. 1.25).")]
    [Range(0.5f, 2.0f)]
    public float maxPitch = 1.25f;

    [Tooltip("Minimum volume factor for typing click variation.")]
    [Range(0.1f, 1.0f)]
    public float minVolume = 0.55f;

    [Tooltip("Maximum volume factor for typing click variation.")]
    [Range(0.1f, 1.0f)]
    public float maxVolume = 0.85f;

    [Tooltip("Skip playing click sound on whitespace characters (spaces, tabs, newlines).")]
    public bool skipSoundOnWhitespace = true;

    [Tooltip("Play typing sound every N typed characters (1 = every character).")]
    [Range(1, 5)]
    public int playSoundEveryNCharacters = 1;

    [Tooltip("Extra pause delay multiplier when encountering sentence-ending punctuation (., ?, !).")]
    public float punctuationPauseMultiplier = 3.5f;

    [Tooltip("Subtle voice/sentence pitch inflection (e.g. rising pitch for questions, energetic for exclamations).")]
    public bool useSentenceInflection = true;

    [HideInInspector]
    public bool isDialogueActive = false;
    [HideInInspector]
    public bool hasPlayed = false;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Action onDialogueCompleteCallback;
    private float previousTimeScale = 1f;

    [Header("Shake Settings")]
    [Tooltip("How much the text shakes when wrapped in <link=\"shake\">.</link>")]
    public float shakeIntensity = 2f;
    [Tooltip("How fast the text shakes.")]
    public float shakeSpeed = 20f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Auto-detect references if not explicitly dragged into the Inspector
        if (dialogueText == null)
        {
            dialogueText = GetComponent<TMP_Text>() ?? GetComponentInChildren<TMP_Text>(true);
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<UnityEngine.UI.Image>() ?? GetComponentInChildren<UnityEngine.UI.Image>(true);
        }

        if (backgroundRawImage == null)
        {
            backgroundRawImage = GetComponent<UnityEngine.UI.RawImage>() ?? GetComponentInChildren<UnityEngine.UI.RawImage>(true);
        }

        if (backgroundSprite == null)
        {
            backgroundSprite = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true);
        }

        if (dialogueContainer == null)
        {
            dialogueContainer = gameObject;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        ResetAudioPitch();

        // Hide dialogue box initially at the start of the game
        SetDialogueVisible(false);
    }

    private void LateUpdate()
    {
        if (dialogueText == null || !isDialogueActive) return;

        dialogueText.ForceMeshUpdate();
        var textInfo = dialogueText.textInfo;
        
        bool needsUpdate = false;

        for (int i = 0; i < textInfo.linkCount; i++)
        {
            TMP_LinkInfo linkInfo = textInfo.linkInfo[i];
            
            if (linkInfo.GetLinkID() == "shake")
            {
                for (int j = 0; j < linkInfo.linkTextLength; j++)
                {
                    int charIndex = linkInfo.linkTextfirstCharacterIndex + j;
                    
                    if (!textInfo.characterInfo[charIndex].isVisible) continue;

                    int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
                    int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;

                    Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

                    // Calculate offset using Perlin Noise for a smooth but chaotic shake
                    Vector2 offset = new Vector2(
                        Mathf.PerlinNoise(Time.unscaledTime * shakeSpeed, charIndex * 0.1f) - 0.5f,
                        Mathf.PerlinNoise(charIndex * 0.1f, Time.unscaledTime * shakeSpeed) - 0.5f
                    ) * shakeIntensity;

                    // Apply offset to all 4 vertices of the character
                    sourceVertices[vertexIndex + 0] += (Vector3)offset;
                    sourceVertices[vertexIndex + 1] += (Vector3)offset;
                    sourceVertices[vertexIndex + 2] += (Vector3)offset;
                    sourceVertices[vertexIndex + 3] += (Vector3)offset;

                    needsUpdate = true;
                }
            }
        }

        if (needsUpdate)
        {
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                dialogueText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }

    private void Update()
    {
        // F9 shortcut to trigger or test the intro dialogue at any time
        if (Input.GetKeyDown(KeyCode.F9) && !isDialogueActive)
        {
            Spawner.ExecuteInstantBossSequenceCheat();
            return;
        }

        if (!isDialogueActive) return;

        // Advance or speed up on Space, Enter, E, or Left Mouse Click
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E) ||
            Input.GetMouseButtonDown(0))
        {
            OnPlayerAdvanceInput();
        }

        // Allow skipping entire cutscene with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipEntireDialogue();
        }
    }

    private Action<int> onLineChangedCallback;

    /// <summary>
    /// Starts the dialogue with custom lines. Useful for mid-fight or outro dialogue.
    /// </summary>
    public void PlayCustomDialogue(string[] customLines, Action onComplete = null, Action<int> onLineChanged = null)
    {
        if (customLines != null && customLines.Length > 0)
        {
            dialogueLines = customLines;
        }
        hasPlayed = false; // Reset so it can play again
        onLineChangedCallback = onLineChanged;
        StartDialogue(onComplete);
    }

    /// <summary>
    /// Starts the boss intro dialogue sequence. When finished, invokes the onComplete callback.
    /// </summary>
    public void StartDialogue(Action onComplete = null)
    {
        if (hasPlayed)
        {
            onComplete?.Invoke();
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        onDialogueCompleteCallback = onComplete;
        isDialogueActive = true;
        hasPlayed = true;
        currentLineIndex = 0;

        if (pauseGameDuringDialogue)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        // Make dialogue UI visible
        SetDialogueVisible(true);

        ResetAudioPitch();

        if (popupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(popupSound);
        }

        StartNextLine();
    }

    private void StartNextLine()
    {
        if (currentLineIndex < dialogueLines.Length)
        {
            onLineChangedCallback?.Invoke(currentLineIndex);

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeLineCoroutine(dialogueLines[currentLineIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLineCoroutine(string line)
    {
        isTyping = true;
        
        if (continuePromptText != null)
        {
            continuePromptText.gameObject.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.text = line;
            dialogueText.ForceMeshUpdate();
            dialogueText.maxVisibleCharacters = 0;
        }
        else
        {
            yield break;
        }

        int totalChars = dialogueText.textInfo.characterCount;
        int soundCharCounter = 0;

        for (int i = 0; i < totalChars; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;
            char c = dialogueText.textInfo.characterInfo[i].character;

            bool isWhitespace = char.IsWhiteSpace(c);

            // Procedurally play randomized click sound
            if (typeSound != null && audioSource != null)
            {
                if (!isWhitespace || !skipSoundOnWhitespace)
                {
                    soundCharCounter++;
                    if (soundCharCounter % playSoundEveryNCharacters == 0)
                    {
                        // Pass false for question/exclamation logic for simplicity, or detect it
                        PlayDynamicTypeSound(i, totalChars, c == '?', c == '!');
                    }
                }
            }

            // Calculate timing pause with intelligent punctuation cadence
            float charDelay = typingSpeed;

            if (c == '.' || c == '?' || c == '!')
            {
                charDelay = typingSpeed * punctuationPauseMultiplier;
            }
            else if (c == ',' || c == ';' || c == ':' || c == '-')
            {
                charDelay = typingSpeed * (punctuationPauseMultiplier * 0.5f);
            }

            if (useUnscaledTime)
            {
                yield return new WaitForSecondsRealtime(charDelay);
            }
            else
            {
                yield return new WaitForSeconds(charDelay);
            }
        }

        isTyping = false;
        typingCoroutine = null;

        // Reset pitch to 1.0f before playing finish line sound
        ResetAudioPitch();

        if (finishLineSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(finishLineSound, 0.6f);
        }

        if (continuePromptText != null)
        {
            continuePromptText.gameObject.SetActive(true);
        }
    }

    private void PlayDynamicTypeSound(int charIndex, int totalChars, bool isQuestion, bool isExclamation)
    {
        if (audioSource == null || typeSound == null) return;

        // Base randomized pitch between minPitch and maxPitch
        float pitch = UnityEngine.Random.Range(minPitch, maxPitch);

        // Optional inflection near the end of the sentence
        if (useSentenceInflection && totalChars > 5)
        {
            float progress = (float)charIndex / totalChars;
            if (progress > 0.65f)
            {
                if (isQuestion)
                {
                    // Subtle upward inflection for question mark (?): mimics questioning tone
                    pitch += (progress - 0.65f) * 0.4f;
                }
                else if (isExclamation)
                {
                    // Slight punchy lift for exclamation (!)
                    pitch += (progress - 0.65f) * 0.25f;
                }
            }
        }

        // Clamp pitch to a safe audible range
        audioSource.pitch = Mathf.Clamp(pitch, 0.4f, 2.5f);

        // Slight randomized volume variation for physical mechanical feel
        float volume = UnityEngine.Random.Range(minVolume, maxVolume);

        audioSource.PlayOneShot(typeSound, volume);
    }

    private void ResetAudioPitch()
    {
        if (audioSource != null)
        {
            audioSource.pitch = 1.0f;
        }
    }

    private void OnPlayerAdvanceInput()
    {
        if (isTyping)
        {
            // If still typing out, instantly reveal the full line
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            ResetAudioPitch();

            if (dialogueText != null)
            {
                dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
            }

            isTyping = false;
            if (continuePromptText != null)
            {
                continuePromptText.gameObject.SetActive(true);
            }
        }
        else
        {
            // Move to next line
            currentLineIndex++;
            StartNextLine();
        }
    }

    public void SkipEntireDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        ResetAudioPitch();

        EndDialogue();
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        isTyping = false;

        ResetAudioPitch();

        if (pauseGameDuringDialogue)
        {
            Time.timeScale = previousTimeScale;
        }

        SetDialogueVisible(false);

        // Hand off to start the boss fight
        onDialogueCompleteCallback?.Invoke();
        onDialogueCompleteCallback = null;
    }

    public void SetDialogueVisible(bool visible)
    {
        if (dialogueContainer != null && dialogueContainer != gameObject)
        {
            dialogueContainer.SetActive(visible);
        }

        if (dialogueText != null)
        {
            if (dialogueText.gameObject != gameObject)
                dialogueText.gameObject.SetActive(visible);
            else
                dialogueText.enabled = visible;
        }

        if (backgroundImage != null)
        {
            if (backgroundImage.gameObject != gameObject)
                backgroundImage.gameObject.SetActive(visible);
            else
                backgroundImage.enabled = visible;
        }

        if (backgroundRawImage != null)
        {
            if (backgroundRawImage.gameObject != gameObject)
                backgroundRawImage.gameObject.SetActive(visible);
            else
                backgroundRawImage.enabled = visible;
        }

        if (backgroundSprite != null)
        {
            if (backgroundSprite.gameObject != gameObject)
                backgroundSprite.gameObject.SetActive(visible);
            else
                backgroundSprite.enabled = visible;
        }

        if (continuePromptText != null)
        {
            if (continuePromptText.gameObject != gameObject)
                continuePromptText.gameObject.SetActive(visible && !isTyping);
            else
                continuePromptText.enabled = visible && !isTyping;
        }
    }
}
