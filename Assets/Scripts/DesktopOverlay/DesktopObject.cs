using UnityEngine;

public enum DesktopObjectType
{
    File,
    Folder,
    Shortcut,
    FileExplorer,
    Browser,
    RecycleBin,
    ThisPC,
    Unknown
}

public class DesktopObject
{
    public string displayName;
    public DesktopObjectType type;
    public string targetPath; // For shortcuts or files
    public string targetExecutable; // Just the exe name if applicable
    public string sourcePath; // The desktop item itself; remains .lnk for shortcuts.
    public string shellIdentity; // Canonical Shell parsing identity, including virtual objects.
    public bool isShortcut;
    public Vector2 screenPosition; // Physical virtual-screen pixels at the icon center, top-left origin.
    
    public override string ToString()
    {
        return $"Name: {displayName}, Type: {type}, Target: {targetPath}, Executable: {targetExecutable}, Pos: {screenPosition}";
    }
}
