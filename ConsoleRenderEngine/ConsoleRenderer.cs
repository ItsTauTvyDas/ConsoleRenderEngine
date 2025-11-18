using ConsoleRenderEngine.Writer;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using static ConsoleRenderEngine.WindowsApi;

namespace ConsoleRenderEngine;

public class ConsoleRenderer
{
    public ConsolePixelWriter PixelWriter = new ConsolePixelWriter();
    public ConsoleTextWriter TextWriter = new ConsoleTextWriter();
    public ConsolePixelPhysics PixelPhysics;

    public static bool EnableDebug;
    public static string DebugTitleMessage = "";
    
    public static Mode RenderMode = Mode.AutoRender;

    public event EventHandler PixelsDoneRendering;
        
    // public bool IsReRendering { private set; get; }

    public int FramesPerSecond { private set; get; }

    private int LastTick, LastFrameRate, FrameRate;

    public int LoopSleep;
    public readonly int Width, Height;

    public event Action ActionEveryLoop, ActionBeforeRender, ActionAfterRender;

    public ConsoleRenderer() : this(Console.WindowWidth / 2, Console.WindowHeight) {}

    public ConsoleRenderer(int width, int height)
    {
        width *= 2;

        Width = width;
        Height = height;

        PixelPhysics = new ConsolePixelPhysics(PixelWriter);

        var handle = GetStdHandle(-11);
        GetConsoleMode(handle, out var mode);
        SetConsoleMode(handle, mode | 0x4); //Set mode to flag ENABLE_VIRTUAL_TERMINAL_PROCESSING(0x4)

        //Disable scrollbars and make it so you can write in scrollbar places
        Console.SetWindowSize(width - 2, height);
        Console.SetBufferSize(width, height);

        var stdHandle = GetStdHandle(-11);
        var bufferInfo = new ConsoleScreenBufferInfoEx();
        bufferInfo.cbSize = (uint)Marshal.SizeOf(bufferInfo);
        GetConsoleScreenBufferInfoEx(stdHandle, ref bufferInfo);
        ++bufferInfo.srWindow.Right;
        ++bufferInfo.srWindow.Bottom;
        SetConsoleScreenBufferInfoEx(stdHandle, ref bufferInfo);

        DisableQuickEdit();
    }

    public void StartRenderLoop()
    {
        while (true)
        {
            if (LoopSleep > 0)
                Thread.Sleep(LoopSleep);

            ActionEveryLoop?.Invoke();
            
            if (RenderMode != Mode.AutoRender || (PixelWriter.PixelTable.StorageQueue.Count == 0 && TextWriter.TextStorageQueue.Count == 0))
                continue;
            
            Render();
        }
    }

    public void CalculateFrameRate()
    {
        if (Environment.TickCount - LastTick >= 1000)
        {
            LastFrameRate = FrameRate;
            FrameRate = 0;
            LastTick = Environment.TickCount;
        }

        FrameRate++;
        FramesPerSecond = LastFrameRate;
    }

    public void Render()
    {
        ActionBeforeRender?.Invoke();

        Console.CursorVisible = false;

        FastConsole.WriteAll(PixelWriter, TextWriter);
        FastConsole.Flush();

        CalculateFrameRate();
        if (EnableDebug)
            Console.Title = $"FPS: {FramesPerSecond}, SP: {PixelWriter.PixelTable.Storage.Count}, QP: {PixelWriter.PixelTable.StorageQueue.Count} Debug: {DebugTitleMessage}";

        PixelsDoneRendering?.Invoke(null, new EventArgs());
        ActionAfterRender?.Invoke();
    }

    public enum Mode
    {
        ManualRender, AutoRender
    }

    /*public void Render(bool reRender = false)
    {
        if (IsReRendering)
            return;

        Console.CursorVisible = false;

        CalculateFrameRate();
        if (Debug)
            Console.Title = $"Frame Rate: {FramesPerSecond}, Static Pixels: {PixelWriter.PixelTable.Storage.Count}, Queued Pixels: {PixelWriter.PixelTable.StorageQueue.Count}, Debug: {DebugText}";

        if (reRender && !IsReRendering)
        {
            IsReRendering = true;

            for (int x_ = 0; x_ < Console.WindowWidth; x_++)
                for (int y_ = 0; y_ < Console.WindowHeight; y_++)
                    PixelWriter.WritePixel(x_, y_, null);

            foreach (var pixel in PixelWriter.PixelTable.Storage)
                PixelWriter.PixelTable.StorageQueue.Enqueue(pixel);
            PixelWriter.PixelTable.Storage.Clear();

            IsReRendering = false;
            return;
        }

        var queue = PixelWriter.PixelTable.StorageQueue;
        var textQueue = TextWriter.TextStorageQueue;

        RgbConsoleColor color = null;
        int x = 0, y = 0;

        Tuple<int, int, char, int, char> textTuple = null;

        object last = null;

        if (textQueue.TryDequeue(out var text))
        {
            if (textQueue.Count == 0)
                last = text;

            int combined = text.Item1;
            Utilities.Separate(combined, out x, out y);
            if (!Utilities.IsInBounds(x, y))
                return;

            color = PixelWriter.GetPixelColor(x, y, out _);

            TextWriter.WriteToStorage(text);
            textTuple = text;
            Write(x, y, color, textTuple);
        }
        else if (queue.TryDequeue(out var next))
        {
            if (queue.Count == 0)
                last = next;

            int combined = next.Key;
            color = RgbConsoleColor.Color(next.Value);

            Utilities.Separate(combined, out x, out y, out int layer);

            if (!Utilities.IsInBounds(x, y))
                return;

            PixelWriter.PixelTable.GetPixelColor(x, y, out int layer_);
            var upperColor = PixelWriter.PixelTable.GetPixelColor(x, y, out _, layer);

            if (layer < layer_)
                return;

            PixelWriter.PixelTable.WritePixel(combined, color, true);

            if (color.Rgb == -1)
                color = upperColor;

            TextWriter.TryGetRawValue(x, y, out textTuple);
            Write(x, y, color, textTuple);
        }

        if (last != null)
            _PixelsDoneRendering.Invoke(last, new EventArgs());
    }

    private void Write(int x, int y, RgbConsoleColor color, Tuple<int, int, char, int, char> textTuple)
    {
        if (color == null || color.Rgb < 0)
            color = RgbConsoleColor.Default;

        Utilities.SetWriteLocation(x, y);
        color.SetConsoleColor();

        if (textTuple != null)
        {
            int color1 = textTuple?.Item2 ?? -1;
            int color2 = textTuple?.Item4 ?? -1;

            if (color1 > -1)
                RgbConsoleColor.Color(color1).SetConsoleColor(true);
            FastConsole.Write(textTuple.Item3);

            if (color2 > -1)
                RgbConsoleColor.Color(color2).SetConsoleColor(true);
            FastConsole.Write(textTuple.Item5);
        }
        else
            FastConsole.Write("  ");

        FastConsole.Flush();
    }*/
}