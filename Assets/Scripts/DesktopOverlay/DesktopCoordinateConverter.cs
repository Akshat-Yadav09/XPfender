using UnityEngine;
using System;

public static class DesktopCoordinateConverter
{
    public static bool TryWindowsToUnityWorld(Vector2 physicalPosition, Camera camera,
        float worldZ, out Vector3 worldPosition)
    {
        worldPosition = default;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (camera == null || camera.targetTexture != null) return false;
        IntPtr window = WindowsShellInterop.FindPlayerWindow();
        if (window == IntPtr.Zero) return false;
        IntPtr previous = WindowsShellInterop.SetThreadDpiAwarenessContext(new IntPtr(-4));
        try
        {
            WindowsShellInterop.RECT client;
            var origin = new WindowsShellInterop.POINT();
            if (!WindowsShellInterop.GetClientRect(window, out client)
                || !WindowsShellInterop.ClientToScreen(window, ref origin)) return false;
            int width = client.right - client.left, height = client.bottom - client.top;
            if (width <= 0 || height <= 0) return false;
            Vector2 screen = PhysicalToUnityScreen(physicalPosition,
                new Rect(origin.x, origin.y, width, height), new Vector2(Screen.width, Screen.height));
            if (!camera.pixelRect.Contains(screen)) return false;
            float depth = camera.WorldToScreenPoint(new Vector3(0, 0, worldZ)).z;
            if (depth <= 0) return false;
            worldPosition = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
            return true;
        }
        finally
        {
            if (previous != IntPtr.Zero) WindowsShellInterop.SetThreadDpiAwarenessContext(previous);
        }
#else
        return false;
#endif
    }

    public static Vector2 PhysicalToUnityScreen(Vector2 point, Rect physicalClient, Vector2 renderSize)
    {
        return new Vector2((point.x - physicalClient.x) * renderSize.x / physicalClient.width,
            (physicalClient.y + physicalClient.height - point.y) * renderSize.y / physicalClient.height);
    }
}
