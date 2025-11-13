using System.Runtime.InteropServices;

namespace SpawnDev.BlazorJS.Photino.Native;

internal static class NativeMethodsWindows
{
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(nint hWnd, int nCmdShow);

    // ShowWindow commands
    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;
    public const int SW_MINIMIZE = 6;
    public const int SW_RESTORE = 9;
}
