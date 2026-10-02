using UnityEngine;
using System.Collections.Generic;

public class CountPlayers : MonoBehaviour
{
    public GameObject targetObject;  // Reference to the GameObject to enable when player count is 0
    private Dictionary<GameObject, float> chipTimers = new Dictionary<GameObject, float>(); // Track timer for each "Chip" tagged object
    private float chipTimerThreshold = 35f;  // Time threshold for chip objects (35 seconds)

    void Update()
    {
        // Find all game objects with the tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Log the count of players
        Debug.Log("Number of 'Player' objects in the scene: " + players.Length);

        // Check if the player count is 0 and enable the target object if there are no players
        if (players.Length == 0 && targetObject != null)
        {
            targetObject.SetActive(true);
        }

        // Find all game objects with the tag "Chip"
        GameObject[] chips = GameObject.FindGameObjectsWithTag("Chip");

        // Loop through each "Chip" tagged object
        foreach (GameObject chip in chips)
        {
            // If the chip is not already in the dictionary, add it with a timer
            if (!chipTimers.ContainsKey(chip))
            {
                chipTimers[chip] = 0f;  // Start the timer from 0 for new "Chip" tagged objects
            }

            // Increment the timer for the current "Chip" object
            chipTimers[chip] += Time.deltaTime;

            // Check if the timer exceeds the threshold
            if (chipTimers[chip] > chipTimerThreshold)
            {
                // Perform an action for the chip that exceeded the time threshold
                Debug.Log("Chip object " + chip.name + " has been in the scene for more than 35 seconds.");
                
                // You can add your action here, for example enabling a GameObject:
                if (targetObject != null)
                {
                    targetObject.SetActive(true);  // Example action: Enable target object
                }

                // You may want to reset or remove the chip from the dictionary after the action is performed
                chipTimers[chip] = 0f;  // Reset timer if necessary
            }
        }
    }
}
