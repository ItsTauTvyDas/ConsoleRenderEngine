using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using ConsoleRenderEngine;
using static ConsoleRenderEngine.Writer.ConsolePixelWriter;
// ReSharper disable FunctionNeverReturns

namespace Examples;

internal class SnakeGame : ConsoleRenderer
{
    private HashSet<int> Apples = new();
    private List<int> Tail = new();

    private Random Random = new();

    public int HighestScore, 
               DirectionX, //1 = Right, -1 = Left
               DirectionY; //1 = Up, -1 = Down

    public readonly int MaxX = Console.WindowWidth / 2 + 2,
                        MaxY = Console.WindowHeight - 1,
                        
                        MinX = 1,
                        MinY = 1,
                        
                        AppleCount = 2;

    public Point Location = new(), OldLocation = new();

    private bool TickSnake, Init, Moved;

    private RgbConsoleColor SnakeColor = RgbConsoleColor.Color(255, 255, 0), 
                            AppleColor = RgbConsoleColor.Color(255, 0, 0),
                            WallColor = RgbConsoleColor.Color(255, 255, 255);
    
    [DllImport("user32.dll")]
    public static extern bool MessageBeep(uint uType);

    public SnakeGame()
    {
        TextWriter.WriteText(49, MaxY, "Made by ItsTautvydas", RgbConsoleColor.Color(100, 10, 200));

        //Snake controls
        var thread = new Thread(() =>
        {
            while (true)
            {
                var keyInfo = Console.ReadKey(true);

                if (!Moved)
                    continue;

                var key = keyInfo.Key;
                Moved = false;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (DirectionY == -1 && Tail.Count > 0)
                            break;
                        DirectionX = 0;
                        DirectionY = 1;
                        break;
                    case ConsoleKey.DownArrow:
                        if (DirectionY == 1 && Tail.Count > 0)
                            break;
                        DirectionX = 0;
                        DirectionY = -1;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (DirectionX == 1 && Tail.Count > 0)
                            break;
                        DirectionY = 0;
                        DirectionX = -1;
                        break;
                    case ConsoleKey.RightArrow:
                        if (DirectionX == -1 && Tail.Count > 0)
                            break;
                        DirectionY = 0;
                        DirectionX = 1;
                        break;
                    case ConsoleKey.NumPad0:
                        DirectionX = 0;
                        DirectionY = 0;
                        break;
                }
            }
        });

        //Move snake every 80ms
        var movementThread = new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(80);
                TickSnake = true;
                Moved = true;
            }
        });

        //Do stuff after rendering walls
        PixelsDoneRendering += (_, _) =>
        {
            if (Init)
                return;
            
            thread.Start();
            movementThread.Start();
            Init = true;
        };

        RenderMode = Mode.ManualRender;

        PaintBackground();
        Reset();

        ActionEveryLoop += () =>
        {
            //Do stuff BEFORE rendering
            if (!TickSnake)
                return;
            //This could be updated only when snake collects an apple or dies
            TextWriter.WriteText(1, MaxY, "Score: " + Tail.Count + "      ", RgbConsoleColor.Default);
            TextWriter.WriteText(8, MaxY, "Highest Score: " + HighestScore, RgbConsoleColor.Default);
            MoveSnake();
            CheckForApple();
            TickSnake = false;
            Render();
        };

        //Main loop
        StartRenderLoop();
    }

    public void Reset()
    {
        if (HighestScore < Tail.Count)
            HighestScore = Tail.Count;

        TickSnake = false;
        DirectionX = DirectionY = 0;

        PixelWriter.PixelLayer = 1;

        PixelWriter.WritePixel(Location.X, Location.Y, null);
        PixelWriter.WritePixel(OldLocation.X, OldLocation.Y, null);

        foreach (var combined in Tail)
            PixelWriter.PixelTable.WritePixel(combined, null);

        Tail.Clear();

        Location.X = Console.WindowWidth / 4 - 3;
        Location.Y = Console.WindowHeight / 2;

        OldLocation = Location.Copy();
        Location.X++;

        foreach (var combined in Apples)
            PixelWriter.PixelTable.WritePixel(combined, null);

        Apples.Clear();

        GenerateApples();

        RenderSnake();
        Render();
    }

    //Not the best way, because it might be a problem if there's not much space left
    private void GenerateApple()
    {
        int x = Random.Next(MinX, MaxX), y = Random.Next(MinY, MaxY);
        var point = new Point(x, y);
        var combined = point.Combine(1);

        var color = PixelWriter.GetSurfacePixelColor(x, y, out _);
        if (Apples.Contains(combined) || color != null && (color.Rgb == SnakeColor.Rgb || color.Rgb == AppleColor.Rgb))
        {
            GenerateApple();
            return;
        }

        Apples.Add(combined);
        PixelWriter.PixelTable.WritePixel(combined, AppleColor);
    }

    public void GenerateApples()
    {
        for (var i = 0; i < AppleCount; i++)
            GenerateApple();
    }

    public bool CheckForApple()
    {
        var combined = Location.Combine(1);

        if (!Apples.Remove(combined))
            return false;
        
        Tail.Add(combined);
        new Thread(Console.Beep).Start(); //Beep is blocking method
        GenerateApple();
        return true;
    }

    public void MoveSnake()
    {
        OldLocation = Location.Copy();
        Location.X += DirectionX;
        Location.Y += DirectionY;

        var color = PixelWriter.PixelTable.GetSurfacePixelColor(Location.X, Location.Y, out _);
        if (Location != OldLocation && color != null && (color.Rgb == SnakeColor.Rgb || color.Rgb == WallColor.Rgb))
            //if (Location.X >= MaxX || Location.X < MinX || Location.Y < MinY || Location.Y >= MaxY)
        {
            MessageBeep(0x00000030); // MB_ICONEXCLAMATION
            Reset();
            return;
        }

        RenderSnake();
    }

    private void RenderSnake()
    {
        PixelWriter.PixelLayer = 1;
        if (Tail.Count > 0)
        {
            //Set last location as tail's first element
            Tail.Insert(0, OldLocation.Combine(1));

            var index = 0;
            foreach (var tail in Tail) //Render tail
            {
                PixelWriter.PixelTable.WritePixel(tail, index + 1 == Tail.Count ? null : SnakeColor);
                index++;
            }

            //then remove last one
            Tail.RemoveAt(Tail.Count - 1);
        }
        else if (OldLocation != Location)
            PixelWriter.WritePixel(OldLocation.X, OldLocation.Y, null);

        PixelWriter.WritePixel(Location.X, Location.Y, SnakeColor);
    }

    public void PaintBackground()
    {
        int w = 59, h = 29;

        PixelWriter.PixelLayer = 0;

        PixelWriter.PixelLayer = 2;
        PixelWriter.WritePixels(0, 0, w, 0, WallColor);
        PixelWriter.WritePixels(w, 0, w, h + 1, WallColor);
        PixelWriter.WritePixels(w, h, 0, h, WallColor);
        PixelWriter.WritePixels(0, h, 0, 0, WallColor);
    }
}