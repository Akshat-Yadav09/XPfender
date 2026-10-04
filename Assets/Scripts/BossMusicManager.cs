using UnityEngine;
using System.Collections;

public class BossMusicManager : MonoBehaviour
{
    public static BossMusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("The AudioSource that plays the normal background music. If empty, the script will look for one on this GameObject.")]
    public AudioSource bgmAudioSource;

    [Header("Normal Music")]
    [Tooltip("The normal music clip to play before the boss fight starts. (Plays automatically on start)")]
    public AudioClip normalMusic;

    [Tooltip("Whether to automatically play the normal music when the scene loads.")]
    public bool playNormalMusicOnStart = true;

    [Header("Boss Fight Music")]
    [Tooltip("The music clip to play when the boss fight starts.")]
    public AudioClip bossFightMusic;

    [Tooltip("Should the normal music smoothly crossfade into the boss music?")]
    public bool crossfade = true;

    [Tooltip("Duration of the crossfade in seconds.")]
    public float crossfadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (bgmAudioSource == null)
        {
            bgmAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (playNormalMusicOnStart && normalMusic != null && bgmAudioSource != null)
        {
            bgmAudioSource.clip = normalMusic;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
    }

    private bool hasBossMusicStarted = false;

    /// <summary>
    /// Switches the background music to the boss fight music.
    /// </summary>
    public void PlayBossMusic()
    {
        if (hasBossMusicStarted) return;
        hasBossMusicStarted = true;

        if (bgmAudioSource == null || bossFightMusic == null)
        {
            Debug.LogWarning("[BossMusicManager] Missing BGM AudioSource or Boss Fight Music Clip! Cannot play boss music.");
            return;
        }

        if (crossfade)
        {
            StartCoroutine(CrossfadeMusic(bossFightMusic, crossfadeDuration));
        }
        else
        {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = bossFightMusic;
            bgmAudioSource.Play();
        }
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        float startVolume = bgmAudioSource.volume;
        float halfDuration = duration / 2f;

        // Fade out current music
        for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
        {
            bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDuration);
            yield return null;
        }

        bgmAudioSource.volume = 0f;
        bgmAudioSource.Stop();
        
        // Swap clip and play
        bgmAudioSource.clip = newClip;
        bgmAudioSource.Play();

        // Fade in new music
        for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
        {
            bgmAudioSource.volume = Mathf.Lerp(0f, startVolume, t / halfDuration);
            yield return null;
        }

        bgmAudioSource.volume = startVolume;
    }

    /// <summary>
    /// Instantly stops the music and optionally plays a sound effect.
    /// </summary>
    public void StopMusic(AudioClip endSfx = null)
    {
        StopAllCoroutines();
        
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }

        if (endSfx != null)
        {
            // Play it on a temporary AudioSource or PlayClipAtPoint so it isn't affected by the music AudioSource's settings
            AudioSource.PlayClipAtPoint(endSfx, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 1.0f);
        }
    }
}
