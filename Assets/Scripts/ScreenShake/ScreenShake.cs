using UnityEngine;

public class ShakeObject : MonoBehaviour
{
    public float shakeAmount = 0.1f;  // How much the object shakes
    public float shakeDuration = 1f;  // How long the shake lasts
    private Vector3 originalPosition;
    private float shakeTimer;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // Shake effect
            Vector3 randomShake = originalPosition + new Vector3(
                Random.Range(-shakeAmount, shakeAmount), 
                Random.Range(-shakeAmount, shakeAmount), 
                Random.Range(-shakeAmount, shakeAmount)  // Optional to shake on Z-axis
            );
            transform.position = randomShake;

            // Decrease shake timer
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            // Reset to original position once shaking is done
            transform.position = originalPosition;
        }
    }

    // Method to start the shake
    public void StartShake()
    {
        shakeTimer = shakeDuration;
    }
}
