using UnityEngine;

public class ShakeObject : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeAmount = 15f;  // Larger amount for UI/Screen shake
    public float shakeDuration = 0.5f; // Faster duration for error bump
    public float shakeSpeed = 50f;   // Speed of the shake

    [Header("Audio")]
    public AudioClip errorSound;

    private Vector3 originalPosition;
    private float shakeTimer;
    private AudioSource audioSource;

    void Start()
    {
        originalPosition = transform.localPosition; // localPosition is better for UI
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // Improved head-shake (left/right) effect using sine wave, dampening over time
            float dampen = shakeTimer / shakeDuration;
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount * dampen;
            
            transform.localPosition = originalPosition + new Vector3(offsetX, 0, 0);

            // Decrease shake timer
            shakeTimer -= Time.deltaTime;
        }
        else if (transform.localPosition != originalPosition)
        {
            // Reset to original position once shaking is done
            transform.localPosition = originalPosition;
        }
    }

    // Method to start the shake
    public void StartShake()
    {
        shakeTimer = shakeDuration;
        
        if (errorSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(errorSound);
        }
    }
}
