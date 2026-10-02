using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // Import Scene Management

public class PasswordVideoManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInputField;  // Reference to the password input field
    [SerializeField] private VideoPlayer videoPlayer;  // Reference to the VideoPlayer component
    [SerializeField] private string correctPassword = "password123";  // Correct password

    [SerializeField] private GameObject backgroundCanvas;  // Reference to the background canvas
    [SerializeField] private GameObject videoCanvas;  // Reference to the video player canvas
    [SerializeField] private Button passwordButton;  // Reference to the button that triggers the password check

    [SerializeField] private AudioSource audioSource;  // Reference to the AudioSource component
    [SerializeField] private AudioClip audioClip;  // Reference to the audio clip to play

    [SerializeField] private string nextSceneName;  // Name of the scene to load after the video finishes

    private void Start()
    {
        // Hide video canvas initially
        videoCanvas.SetActive(false);

        // Add listener to the password button
        if (passwordButton != null)
        {
            passwordButton.onClick.AddListener(CheckPasswordAndPlayVideo);
        }
        else
        {
            Debug.LogError("Password button is not assigned!");
        }

        // Attach an event listener to detect when the video finishes playing
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    public void CheckPasswordAndPlayVideo()
    {
        string enteredPassword = passwordInputField.text;  // Get the entered password

        if (enteredPassword == correctPassword)
        {
            Debug.Log("Password is correct! Playing video...");

            // Hide the background canvas and show the video canvas
            backgroundCanvas.SetActive(false);
            videoCanvas.SetActive(true);

            // Play the video if the VideoPlayer is assigned
            if (videoPlayer != null)
            {
                // Load the video clip from the Resources folder
                VideoClip videoClip = Resources.Load<VideoClip>("Videos/Videoplayback");  // Replace with your actual video name without extension

                if (videoClip != null)
                {
                    // Ensure VideoPlayer source is set to VideoClip
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = videoClip;  // Assign the loaded video clip to the VideoPlayer
                    videoPlayer.Play();  // Start playing the video
                    Debug.Log("Playing video...");

                    // Play audio if the AudioSource is assigned
                    if (audioSource != null && audioClip != null)
                    {
                        audioSource.clip = audioClip;  // Assign the audio clip
                        audioSource.Play();  // Start playing the audio
                        Debug.Log("Playing audio...");
                    }
                    else
                    {
                        Debug.LogError("AudioSource or AudioClip is not assigned!");
                    }
                }
                else
                {
                    Debug.LogError("Video file not found in Resources folder!");
                }
            }
            else
            {
                Debug.LogError("VideoPlayer component is not assigned!");
            }
        }
        else
        {
            Debug.Log("Incorrect password! Access denied.");
        }
    }

    // Callback for when the video finishes playing
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished playing. Loading next scene...");

        // Load the next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name is not specified!");
        }
    }
}
