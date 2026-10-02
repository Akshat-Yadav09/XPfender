using UnityEngine;
using System.Collections;

public class NotificationPopup : MonoBehaviour
{
    [SerializeField] private GameObject notificationPopup; // The notification popup UI
    [SerializeField] private AudioSource audioSource; // The AudioSource for playing sound
    [SerializeField] private AudioClip notificationSound; // The sound to play when the notification appears

    [Header("Settings")]
    [SerializeField] private float notificationDuration = 3f; // How long the notification stays on screen

    public void ShowNotification()
    {
        // Show the notification popup
        notificationPopup.SetActive(true);

        // Play the sound immediately
        PlaySound();

        // Hide the notification after the specified duration
        StartCoroutine(HideNotificationAfterDelay());
    }

    private void PlaySound()
    {
        if (audioSource != null && notificationSound != null)
        {
            audioSource.clip = notificationSound; // Set the sound clip
            audioSource.Play(); // Play the sound immediately
        }
        else
        {
            Debug.LogError("AudioSource or Notification Sound is not assigned!");
        }
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration); // Wait for the specified notification duration
        notificationPopup.SetActive(false); // Hide the notification popup
    }
}
