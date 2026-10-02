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
            Debug.LogWarning("No object assigned to enable on destruction.");
        }
    }
}
