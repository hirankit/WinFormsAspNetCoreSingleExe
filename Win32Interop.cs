using System.Runtime.InteropServices;

namespace WinFormsAspNetCoreSingleExe;

public static partial class Win32Interop
{
    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;
    public const int SW_RESTORE = 9;
    [LibraryImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = false)]
    public static partial int ShowWindow(IntPtr hWnd, int nCmdShow);

    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOZORDER = 0x0004;
    [LibraryImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [LibraryImport("kernel32.dll", EntryPoint = "GetConsoleWindow", SetLastError = false)]
    public static partial IntPtr GetConsoleWindow();

    [LibraryImport("user32.dll", EntryPoint = "IsWindowVisible", SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsWindowVisible(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "IsIconic", SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsIconic(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "GetSystemMenu", SetLastError = false)]
    public static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

    public const uint SC_CLOSE = 0xF060;
    public const uint MF_BYCOMMAND = 0x00000000;
    public const int MF_GRAYED = 0x00000001;
    public const int MF_ENABLED = 0x00000000;
    [LibraryImport("user32.dll", EntryPoint = "EnableMenuItem", SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    public static bool EnableCloseButtonMenuItem(IntPtr hMenu)
    {
        return EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);
    }

    public static bool DisableCloseButtonMenuItem(IntPtr hMenu)
    {
        return EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
    }

    public static bool EnableWindowCloseButton(IntPtr hWnd)
    {
        var hMenu = GetSystemMenu(hWnd, false);
        return EnableCloseButtonMenuItem(hMenu);
    }

    public static bool DisableWindowCloseButton(IntPtr hWnd)
    {
        var hMenu = GetSystemMenu(hWnd, false);
        return DisableCloseButtonMenuItem(hMenu);
    }

    public static void RestoreWindowIfMinimized(IntPtr hWnd)
    {
        if (IsIconic(hWnd))
        {
            ShowWindow(hWnd, SW_RESTORE);
        }
    }

    public static void ShowWindow(IntPtr hWnd)
    {
        ShowWindow(hWnd, SW_SHOW);
    }

    public static void HideWindow(IntPtr hWnd)
    {
        ShowWindow(hWnd, SW_HIDE);
    }

    public static void SetWindowPos(IntPtr hWnd, int posX, int posY)
    {
        SetWindowPos(hWnd, IntPtr.Zero, posX, posY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
    }
}