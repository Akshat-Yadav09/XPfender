using System;
using UnityEngine;

[Serializable]
public class DesktopMatchRule
{
    public DesktopObjectType Type;
    
    [Tooltip("List of exact executable names to match for shortcuts (e.g., chrome.exe)")]
    public string[] executableNames;
    
    [Tooltip("Special shell identifiers or names to match (e.g., 'Recycle Bin')")]
    public string[] shellIdentifiers;
    
    [Tooltip("The Unity prefab to spawn over this object")]
    public GameObject bubblePrefab;

    public bool Matches(DesktopObject obj)
    {
        if ((obj.type == Type || (Type == DesktopObjectType.Shortcut && obj.isShortcut)) && Type != DesktopObjectType.Unknown)
        {
            return true;
        }

        if (executableNames != null && !string.IsNullOrEmpty(obj.targetExecutable))
        {
            foreach (var exe in executableNames)
            {
                if (string.Equals(exe, obj.targetExecutable, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        if (shellIdentifiers != null && !string.IsNullOrEmpty(obj.displayName))
        {
            foreach (var id in shellIdentifiers)
            {
                if (!string.IsNullOrEmpty(id) && (string.Equals(id, obj.displayName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(id, obj.shellIdentity, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
