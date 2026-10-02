using UnityEngine;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.UI;

public class LoginManagerTMP : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInputField; // Reference to the TMP InputField
    [SerializeField] private Button loginButton; // Reference to the Login Button
    [SerializeField] private string correctPassword = "********"; // Set your correct password here
    [SerializeField] private ShakeObject objectToShake; // Reference to the object to shake
    [SerializeField] private TMP_Text hintText; // Reference to TextMeshPro for hint display

    private string hintMessage = "Hint: ********"; // Message to show as a hint

    void Start()
    {
        // Add a listener to the login button
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(CheckPassword);
        }
        else
        {
            Debug.LogError("Login Button is not assigned in the Inspector.");
        }

        // Ensure hintText is hidden initially
        if (hintText != null)
        {
            hintText.text = ""; // Clear hint text at the start
        }
        else
        {
            Debug.LogError("Hint Text (TMP_Text) reference is not assigned.");
        }
    }

    public void CheckPassword()
    {
        string enteredPassword = passwordInputField.text;

        if (enteredPassword == correctPassword)
        {
            Debug.Log("Login Successful!");
            // Add your logic for successful login here
        }
        else
        {
            Debug.Log("Incorrect Password! Try again.");

            // Shake the object when the password is incorrect
            if (objectToShake != null)
            {
                objectToShake.StartShake();
            }
            else
            {
                Debug.LogError("ShakeObject reference is not assigned.");
            }

            // Display the hint
            if (hintText != null)
            {
                hintText.text = hintMessage;
                hintText.color = Color.red; 
                hintText.fontSize = 29;
                hintText.fontStyle = FontStyles.Bold;
            }
        }
    }
}
