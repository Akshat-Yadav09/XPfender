using UnityEngine;

public class CheckObjectsDestroyed : MonoBehaviour
{
    [Header("Manager Reference")]
    public LogoManager logoManager; // Reference to LogoManager

    [Header("UI to Enable")]
    public GameObject ui;

    void Update()
    {
        // Check if all logo parts are destroyed
        if (AreAllPartsDestroyed())
        {
            EnableUI();
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

    private void EnableUI()
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
