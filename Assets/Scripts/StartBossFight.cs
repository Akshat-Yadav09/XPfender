using UnityEngine;

public class EnableOnTriggerDestroy : MonoBehaviour
{
    [Tooltip("The GameObject to enable when this trigger is destroyed.")]
    public GameObject objectToEnable;

    private void OnDestroy()
    {
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
        }
        else
        {
            // Fallback for when Inspector references are lost:
            // Find the inactive boss objects directly in the scene roots
            bool foundFallback = false;
            foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == "NON-Boss" || root.name == "Boss")
                {
                    root.SetActive(true);
                    foundFallback = true;
                }
            }
            
            if (!foundFallback)
            {
                Debug.LogWarning("No object assigned to enable on destruction, and fallback objects (NON-Boss/Boss) could not be found.");
            }
        }
    }
}
