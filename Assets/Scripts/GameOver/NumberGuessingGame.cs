using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NumberGuessingGame : MonoBehaviour
{
    public TMP_InputField inputField;  // Reference to the TMP Input Field
    public TMP_Text resultText;        // Reference to the TMP Text Field
    public Button registerButton;      // Reference to the Button

    private int randomNumber;

    void Start()
    {
        randomNumber = Random.Range(1, 11);  // Generate random number between 1 and 10

        // Add listener to button to call RegisterInput when clicked
        registerButton.onClick.AddListener(RegisterInput);
    }

    public void RegisterInput()
    {
        // Register the input when the button is pressed
        CheckGuess();
    }

    private void CheckGuess()
    {
        if (int.TryParse(inputField.text, out int guess))
        {
            if (guess == randomNumber)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // Restart current scene
            }
            else
            {
                SceneManager.LoadScene("ApoorvaKaScene");  // Change to "ApoorvaKaScene"
            }
        }
        else
        {
            resultText.text = "Please enter a valid number!";
        }
    }
}
