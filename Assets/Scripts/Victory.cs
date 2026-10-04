using UnityEngine;

public class CheckObjectsDestroyed : MonoBehaviour
{
    [Header("Manager Reference")]
    public LogoManager logoManager; // Reference to LogoManager

    [Header("UI to Enable")]
    public GameObject ui;

    private bool phaseOneCompleted = false;

    void Update()
    {
        if (phaseOneCompleted) return;

        // Check if all logo parts are destroyed
        if (AreAllPartsDestroyed())
        {
            phaseOneCompleted = true;

            if (BossPhaseTwoController.Instance != null)
            {
                string[] transitionLines = new string[]
                {
                    "System Report: Threat Neutralized.",
                    "...",
                    "Wait...",
                    "You thought it was over?",
                    "Your computer is <link=\"shake\"><color=red>MINE.</color></link>",
                    "Your files will be <link=\"shake\">temporary...</link>",
                    "But I am <link=\"shake\"><color=red>FOREVER!</color></link>"
                };

                if (BossIntroDialogue.Instance != null)
                {
                    BossIntroDialogue.Instance.PlayCustomDialogue(transitionLines, 
                    () => 
                    {
                        BossPhaseTwoController.Instance.StartPhaseTwo();
                    }, 
                    (lineIndex) => 
                    {
                        // lineIndex 2 corresponds to "Wait..."
                        if (lineIndex == 2)
                        {
                            if (BossMusicManager.Instance != null)
                            {
                                BossMusicManager.Instance.PlayBossMusic();
                            }
                        }
                    });
                }
                else
                {
                    // Fallback if no dialogue manager is found
                    BossPhaseTwoController.Instance.StartPhaseTwo();
                }
            }
            else
            {
                // No Phase 2 found, just end the game
                EnableUI();
            }
        }
    }

    private bool AreAllPartsDestroyed()
    {
        if (logoManager == null || logoManager.logoParts == null)
        {
            Debug.LogError("LogoManager or its logoParts array is not set!");
            return false;
        }

        // Check if all logo parts are destroyed
        foreach (LogoPart part in logoManager.logoParts)
        {
            if (part != null && !part.IsDestroyed) // If any part is not destroyed, return false
            {
                return false;
            }
        }

        return true; // All parts are destroyed
    }

    public void EnableUI()
    {
        if (ui != null)
        {
            ui.SetActive(true);
        }
        else
        {
            Debug.LogError("UI GameObject is not set in the inspector!");
        }
    }
}
