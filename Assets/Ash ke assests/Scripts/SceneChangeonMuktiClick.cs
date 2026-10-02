using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeonMuktiClick : MonoBehaviour
{
    [SerializeField] private GameObject TeleCanvas;
    [SerializeField] private AudioSource audioSource; // AudioSource to play sound
    [SerializeField] private AudioClip buttonClickSound; // Sound to play on button click

    [Header("Sound Delay")]
    [SerializeField] public float soundDelay = 0f; // Delay before sound plays (public to adjust in Inspector)

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MuktiClick()
    {
        StartCoroutine(PlayButtonClickSound()); // Play sound with delay and load scene
        SceneManager.LoadScene("BuggedWindows");
    }

    public void BubbleTon()
    {
        StartCoroutine(PlayButtonClickSound()); // Play sound with delay and load scene
        StartCoroutine(LoadingWithDelay());
        SceneManager.LoadScene("Antivirus");
    }

    private IEnumerator LoadingWithDelay()
    {
        TeleCanvas.SetActive(true);
        yield return new WaitForSeconds(5f);
        TeleCanvas.SetActive(false);
    }

    private IEnumerator PlayButtonClickSound()
    {
        if (soundDelay > 0f)
        {
            yield return new WaitForSeconds(soundDelay); // Wait for the sound delay
        }

        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.clip = buttonClickSound; // Set the sound clip
            audioSource.Play(); // Play the sound
        }
        else
        {
            Debug.LogError("AudioSource or ButtonClickSound is not assigned!");
        }
    }
}
