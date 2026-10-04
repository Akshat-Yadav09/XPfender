using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using UnityEngine;

public static class WindowsDesktopScanner
{
    private static IntPtr GetDesktopListViewHandle()
    {
        IntPtr hWnd = WindowsShellInterop.FindWindow("Progman", "Program Manager");
        Debug.Log($"[DesktopScanner] Progman HWND: {hWnd}");

        IntPtr hDefView = WindowsShellInterop.FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
        Debug.Log($"[DesktopScanner] SHELLDLL_DefView (under Progman) HWND: {hDefView}");
        
        if (hDefView == IntPtr.Zero)
        {
            Debug.Log("[DesktopScanner] Searching WorkerW windows for SHELLDLL_DefView...");
            IntPtr hWorkerW = IntPtr.Zero;
            do
            {
                hWorkerW = WindowsShellInterop.FindWindowEx(IntPtr.Zero, hWorkerW, "WorkerW", null);
                if (hWorkerW != IntPtr.Zero)
                {
                    hDefView = WindowsShellInterop.FindWindowEx(hWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (hDefView != IntPtr.Zero)
                    {
                        Debug.Log($"[DesktopScanner] Found SHELLDLL_DefView under WorkerW HWND: {hWorkerW}");
                    }
                }
            } while (hDefView == IntPtr.Zero && hWorkerW != IntPtr.Zero);
        }

        if (hDefView != IntPtr.Zero)
        {
            IntPtr hSysListView = WindowsShellInterop.FindWindowEx(hDefView, IntPtr.Zero, "SysListView32", "FolderView");
            Debug.Log($"[DesktopScanner] SysListView32 HWND: {hSysListView}");
            return hSysListView;
        }
        
        Debug.Log("[DesktopScanner] Failed to find desktop listview.");
        return IntPtr.Zero;
    }

    public static List<DesktopObject> ScanDesktop()
    {
        List<DesktopObject> results = new List<DesktopObject>();
        if (IntPtr.Size != 8)
        {
            Debug.LogError("[DesktopScanner] Desktop integration requires a 64-bit Windows player.");
            return results;
        }
        IntPtr previousDpi = WindowsShellInterop.SetThreadDpiAwarenessContext(new IntPtr(-4));
        try { return ScanDesktopCore(); }
        finally
        {
            if (previousDpi != IntPtr.Zero) WindowsShellInterop.SetThreadDpiAwarenessContext(previousDpi);
        }
    }

    private static List<DesktopObject> ScanDesktopCore()
    {
        List<DesktopObject> results = new List<DesktopObject>();
        
        IntPtr hListView = GetDesktopListViewHandle();
        if (hListView == IntPtr.Zero)
        {
            Debug.LogError("Could not find Desktop ListView handle.");
            return results;
        }

        uint processId;
        WindowsShellInterop.GetWindowThreadProcessId(hListView, out processId);

        IntPtr hProcess = WindowsShellInterop.OpenProcess(
            WindowsShellInterop.PROCESS_VM_OPERATION | WindowsShellInterop.PROCESS_VM_READ | WindowsShellInterop.PROCESS_VM_WRITE, 
            false, processId);

        if (hProcess == IntPtr.Zero)
        {
            Debug.LogError("Could not open explorer.exe process.");
            return results;
        }

        int itemCount = (int)WindowsShellInterop.SendMessage(hListView, WindowsShellInterop.LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
        Debug.Log($"[DesktopScanner] LVM_GETITEMCOUNT returned: {itemCount}");
        
        if (itemCount <= 0)
        {
            WindowsShellInterop.CloseHandle(hProcess);
            return results;
        }

        uint lvItemSize = (uint)Marshal.SizeOf(typeof(WindowsShellInterop.LVITEMW));
        uint pointSize = (uint)Marshal.SizeOf(typeof(WindowsShellInterop.RECT));
        uint stringBufSize = 512; // bytes, 256 wide chars

        IntPtr pSharedMem = WindowsShellInterop.VirtualAllocEx(hProcess, IntPtr.Zero, (UIntPtr)(lvItemSize + pointSize + stringBufSize), 
            WindowsShellInterop.MEM_COMMIT | WindowsShellInterop.MEM_RESERVE, WindowsShellInterop.PAGE_READWRITE);

        Debug.Log($"[DesktopScanner] VirtualAllocEx returned: {pSharedMem}");

        if (pSharedMem == IntPtr.Zero)
        {
            WindowsShellInterop.CloseHandle(hProcess);
            Debug.LogError("Failed to allocate memory in explorer.exe.");
            return results;
        }

        IntPtr pLvItem = pSharedMem;
        IntPtr pPoint = new IntPtr(pSharedMem.ToInt64() + lvItemSize);
        IntPtr pString = new IntPtr(pSharedMem.ToInt64() + lvItemSize + pointSize);

        string desktopFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string publicDesktopFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);

        try
        {
        Dictionary<string, List<string>> shellPaths = ReadShellPaths();
        for (int i = 0; i < itemCount; i++)
        {
            WindowsShellInterop.LVITEMW lvItem = new WindowsShellInterop.LVITEMW();
            lvItem.mask = 1; // LVIF_TEXT
            lvItem.iItem = i;
            lvItem.pszText = pString;
            lvItem.cchTextMax = 255;

            UIntPtr bytesWritten;
            if (!WindowsShellInterop.WriteProcessMemory(hProcess, pLvItem, ref lvItem, (UIntPtr)lvItemSize, out bytesWritten)
                || bytesWritten.ToUInt64() != lvItemSize) throw NativeFailure("Write LVITEM");

            // Get Text
            int textLength = (int)WindowsShellInterop.SendMessage(hListView, WindowsShellInterop.LVM_GETITEMTEXTW, (IntPtr)i, pLvItem);
            if (textLength <= 0) { Debug.LogWarning("[DesktopScanner] Empty item text at index " + i); continue; }
            
            byte[] stringBuf = new byte[stringBufSize];
            UIntPtr bytesRead;
            if (!WindowsShellInterop.ReadProcessMemory(hProcess, pString, stringBuf, (UIntPtr)stringBufSize, out bytesRead)
                || bytesRead.ToUInt64() != stringBufSize) throw NativeFailure("Read item text");
            string itemName = Encoding.Unicode.GetString(stringBuf, 0, Math.Min(textLength, 254) * 2);

            // Get Position
            // LVIR_ICON gives the icon rectangle, avoiding label and grid-origin offsets.
            WindowsShellInterop.RECT iconRect = new WindowsShellInterop.RECT { left = 1 };
            if (!WindowsShellInterop.WriteProcessMemory(hProcess, pPoint, ref iconRect, (UIntPtr)pointSize, out bytesWritten))
                throw NativeFailure("Write icon rectangle request");
            if (WindowsShellInterop.SendMessage(hListView, WindowsShellInterop.LVM_GETITEMRECT, (IntPtr)i, pPoint) == IntPtr.Zero)
            { Debug.LogWarning("[DesktopScanner] No icon rectangle for " + itemName); continue; }
            if (!WindowsShellInterop.ReadProcessMemory(hProcess, pPoint, out iconRect, (UIntPtr)pointSize, out bytesRead)
                || bytesRead.ToUInt64() != pointSize) throw NativeFailure("Read icon rectangle");
            WindowsShellInterop.POINT point = new WindowsShellInterop.POINT
            { x = (iconRect.left + iconRect.right) / 2, y = (iconRect.top + iconRect.bottom) / 2 };
            if (!WindowsShellInterop.ClientToScreen(hListView, ref point)) throw NativeFailure("ClientToScreen");

            DesktopObject obj = new DesktopObject();
            obj.displayName = itemName;
            obj.screenPosition = new Vector2(point.x, point.y);
            
            List<string> identities;
            if (shellPaths.TryGetValue(itemName, out identities) && identities.Count == 1)
                obj.shellIdentity = identities[0];
            else if (identities != null && identities.Count > 1)
                Debug.LogWarning("[DesktopScanner] Ambiguous Shell name: " + itemName + "; using filesystem fallback.");
            AnalyzeItem(obj, itemName, desktopFolderPath, publicDesktopFolderPath);
            results.Add(obj);
        }

        }
        finally
        {
            WindowsShellInterop.VirtualFreeEx(hProcess, pSharedMem, UIntPtr.Zero, WindowsShellInterop.MEM_RELEASE);
            WindowsShellInterop.CloseHandle(hProcess);
        }

        return results;
    }

    private static Exception NativeFailure(string operation)
    {
        return new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "[DesktopScanner] " + operation);
    }

    private static void AnalyzeItem(DesktopObject obj, string name, string userDesktop, string publicDesktop)
    {
        // Shell parsing identities survive localized or renamed display names.
        if ((!string.IsNullOrEmpty(obj.shellIdentity) && obj.shellIdentity.IndexOf("645FF040-5081-101B-9F08-00AA002F954E", StringComparison.OrdinalIgnoreCase) >= 0)
            || name.Equals("Recycle Bin", StringComparison.OrdinalIgnoreCase))
        {
            obj.type = DesktopObjectType.RecycleBin;
            return;
        }
        if ((!string.IsNullOrEmpty(obj.shellIdentity) && obj.shellIdentity.IndexOf("20D04FE0-3AEA-1069-A2D8-08002B30309D", StringComparison.OrdinalIgnoreCase) >= 0)
            || name.Equals("This PC", StringComparison.OrdinalIgnoreCase) || name.Equals("Computer", StringComparison.OrdinalIgnoreCase))
        {
            obj.type = DesktopObjectType.ThisPC;
            return;
        }

        // Try to find the file in actual desktop folders
        string targetPath = obj.shellIdentity;
        if (string.IsNullOrEmpty(targetPath) || (!File.Exists(targetPath) && !Directory.Exists(targetPath)))
            targetPath = Path.Combine(userDesktop, name);
        if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
        {
            targetPath = Path.Combine(publicDesktop, name);
        }
        // Often shortcuts don't have .lnk in the display name in the shell
        if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
        {
            string lnkPath = Path.Combine(userDesktop, name + ".lnk");
            if (File.Exists(lnkPath)) targetPath = lnkPath;
            else
            {
                lnkPath = Path.Combine(publicDesktop, name + ".lnk");
                if (File.Exists(lnkPath)) targetPath = lnkPath;
            }
        }

        if (File.Exists(targetPath))
        {
            obj.sourcePath = targetPath;
            obj.targetPath = targetPath;
            if (targetPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                obj.type = DesktopObjectType.Shortcut;
                obj.isShortcut = true;
                ResolveShortcut(obj, targetPath);
            }
            else
            {
                obj.type = DesktopObjectType.File;
            }
        }
        else if (Directory.Exists(targetPath))
        {
            obj.sourcePath = targetPath;
            obj.targetPath = targetPath;
            obj.type = DesktopObjectType.Folder;
        }
        else
        {
            obj.type = DesktopObjectType.Unknown;
        }
    }

    // Enrich the visible ListView entries from the Shell namespace, not a directory listing.
    // Duplicate display names are deliberately not assigned an arbitrary identity.
    private static Dictionary<string, List<string>> ReadShellPaths()
    {
        var paths = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        object shell = null, folder = null, items = null;
        try
        {
            shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
            folder = Invoke(shell, "NameSpace", BindingFlags.InvokeMethod, 0);
            items = Invoke(folder, "Items", BindingFlags.InvokeMethod);
            int count = Convert.ToInt32(Invoke(items, "Count", BindingFlags.GetProperty));
            for (int i = 0; i < count; i++)
            {
                object item = Invoke(items, "Item", BindingFlags.InvokeMethod, i);
                try
                {
                    string name = Convert.ToString(Invoke(item, "Name", BindingFlags.GetProperty));
                    string path = Convert.ToString(Invoke(item, "Path", BindingFlags.GetProperty));
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path)) continue;
                    List<string> values;
                    if (!paths.TryGetValue(name, out values)) paths[name] = values = new List<string>();
                    values.Add(path);
                }
                finally { ReleaseCom(item); }
            }
        }
        catch (Exception ex) { Debug.LogWarning("[DesktopScanner] Shell identity lookup failed: " + ex.Message); }
        finally { ReleaseCom(items); ReleaseCom(folder); ReleaseCom(shell); }
        return paths;
    }

    private static object Invoke(object value, string member, BindingFlags flags, params object[] args)
    {
        return value.GetType().InvokeMember(member, flags, null, value, args);
    }

    private static void ReleaseCom(object value)
    {
        if (value != null && Marshal.IsComObject(value)) Marshal.ReleaseComObject(value);
    }

    private static void ResolveShortcut(DesktopObject obj, string lnkPath)
    {
        object shellLinkInstance = null;
        try
        {
            Type shellLinkType = Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"));
            shellLinkInstance = Activator.CreateInstance(shellLinkType);
            
            WindowsShellInterop.IPersistFile persistFile = (WindowsShellInterop.IPersistFile)shellLinkInstance;
            persistFile.Load(lnkPath, 0);
            
            WindowsShellInterop.IShellLinkW shellLink = (WindowsShellInterop.IShellLinkW)shellLinkInstance;
            
            StringBuilder sb = new StringBuilder(512);
            shellLink.GetPath(sb, sb.Capacity, IntPtr.Zero, 0);
            
            obj.targetPath = sb.ToString();
            if (!string.IsNullOrEmpty(obj.targetPath))
            {
                obj.targetExecutable = Path.GetFileName(obj.targetPath);
                
                // Categorize
                string exeName = obj.targetExecutable.ToLower();
                if (exeName == "explorer.exe") obj.type = DesktopObjectType.FileExplorer;
                else if (exeName == "chrome.exe" || exeName == "firefox.exe" || exeName == "msedge.exe" || exeName == "brave.exe")
                    obj.type = DesktopObjectType.Browser;
            }
            
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to resolve shortcut {lnkPath}: {ex.Message}");
        }
        finally
        {
            if (shellLinkInstance != null) Marshal.ReleaseComObject(shellLinkInstance);
        }
    }
}
