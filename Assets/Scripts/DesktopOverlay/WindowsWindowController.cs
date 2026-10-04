using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WindowsWindowController
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    
    private const uint WS_EX_LAYERED = 0x00080000;

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    private static uint originalStyle;
    private static uint originalExStyle;
#endif

    public static void EnableTransparentWindow()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        IntPtr hWnd = GetActiveWindow();
        if (hWnd == IntPtr.Zero) return;

        // Store original styles
        originalStyle = GetWindowLong(hWnd, GWL_STYLE);
        originalExStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

        // Set to popup (borderless) and retain visibility
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        
        // Add layered style to exStyle (required for DWM alpha composition on non-Flip Model swapchains)
        SetWindowLong(hWnd, GWL_EXSTYLE, originalExStyle | WS_EX_LAYERED);

        // Extend DWM frame to the whole client area by using -1
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // Force window update
        SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
#endif
    }

    public static void DisableTransparentWindow()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        IntPtr hWnd = GetActiveWindow();
        if (hWnd == IntPtr.Zero) return;

        // Restore original styles
        if (originalStyle != 0)
        {
            SetWindowLong(hWnd, GWL_STYLE, originalStyle);
            SetWindowLong(hWnd, GWL_EXSTYLE, originalExStyle);
            
            // Remove extended DWM frame
            MARGINS margins = new MARGINS { cxLeftWidth = 0, cxRightWidth = 0, cyTopHeight = 0, cyBottomHeight = 0 };
            DwmExtendFrameIntoClientArea(hWnd, ref margins);
            
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
#endif
    }
}
