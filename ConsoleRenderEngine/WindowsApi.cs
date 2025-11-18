using System;
using System.Runtime.InteropServices;

namespace ConsoleRenderEngine;

public static class WindowsApi
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Coord
    {
        public short X, Y;

        public Coord(short x, short y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SmallRect
    {
        public short Left, Top, Right, Bottom;

        public SmallRect(short width, short height)
        {
            Left = Top = 0;
            Right = width;
            Bottom = height;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ConsoleScreenBufferInfoEx
    {
        public uint cbSize;
        public Coord dwSize;
        public Coord dwCursorPosition;
        public short wAttributes;
        public SmallRect srWindow;
        public Coord dwMaximumWindowSize;
        public ushort wPopupAttributes;
        public bool bFullscreenSupported;

        public ColorRef black, darkBlue, darkGreen, darkCyan, darkRed, darkMagenta, darkYellow, gray, darkGray, blue, green, cyan, red, magenta, yellow, white;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorRef
    {
        public uint ColorDWORD;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref ConsoleScreenBufferInfoEx consoleScreenBufferInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref ConsoleScreenBufferInfoEx consoleScreenBufferInfoEx);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetConsoleMode(IntPtr handle, out uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int handle);

    private const uint EnableQuickEdit = 0x0040;
    private const int StdInputHandle = -10;

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    public static bool DisableQuickEdit()
    {
        var consoleHandle = GetStdHandle(StdInputHandle);
        if (!GetConsoleMode(consoleHandle, out var consoleMode))
            return false;
        consoleMode &= ~EnableQuickEdit;
        return SetConsoleMode(consoleHandle, consoleMode);
    }
}