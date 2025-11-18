using System;
using System.Drawing;
using System.Threading;
using ConsoleRenderEngine;
using static ConsoleRenderEngine.Writer.ConsolePixelWriter;

namespace Examples;

internal class TetrisGame : ConsoleRenderer
{
    class Shape
    {
        public int X, Y, Width, Height, Layer;
        public string[] Pattern;
        public RgbConsoleColor Color;

        public Shape(string[] pattern, int x, int y, int width, int height, int layer, RgbConsoleColor color)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Layer = layer;
            Color = color;
            Pattern = pattern;
        }
    }

    static RgbConsoleColor WallColor = RgbConsoleColor.Color(255, 255, 255);
    static Random Random = new Random();
    Shape CurrentShape;
    bool Move = true, MoveFaster, MoveLeft, MoveRight;
    int ShapesCount;

    string[][] Patterns = new string[][]
    {
        new string[] {
            "####",
            "####",
            "####",
            "####"
        },
        new string[] {
            "########",
            "########"
        },
        new string[] {
            "####",
            "####",
            "##  ",
            "##  ",
            "##  ",
            "##  "
        },
        new string[] {
            "####",
            "####",
            "  ##",
            "  ##",
            "  ##",
            "  ##"
        },
        new string[] {
            "##  ",
            "##  ",
            "####",
            "####",
            "  ##",
            "  ##"
        },
        new string[] {
            "  ##",
            "  ##",
            "####",
            "####",
            "##  ",
            "##  "
        },
        new string[] {
            "  ##  ",
            "  ##  ",
            "######",
            "######"
        }
    };

    RgbConsoleColor[] Colors = new RgbConsoleColor[]
    {
        RgbConsoleColor.FromDrawingColor(Color.Pink),
        RgbConsoleColor.FromDrawingColor(Color.DarkBlue),
        RgbConsoleColor.FromDrawingColor(Color.Yellow),
        RgbConsoleColor.FromDrawingColor(Color.Aqua),
        RgbConsoleColor.FromDrawingColor(Color.Green),
        RgbConsoleColor.FromDrawingColor(Color.IndianRed),
        RgbConsoleColor.FromDrawingColor(Color.YellowGreen)
    };

    public TetrisGame() : base(30, 42)
    {
        PixelPhysics.IgnoreNulls = true;

        LoopSleep = 10;
        RenderMode = Mode.ManualRender;
        PaintBasics();

        Thread moveThread = new Thread(() =>
        {
            while (true)
            {
                ConsoleKeyInfo key = FastConsole.ReadKey();
                if (key.Key == ConsoleKey.DownArrow)
                {
                    MoveFaster = true;
                }
            }
        });
        moveThread.Start();

        CurrentShape = SpawnNextShape();

        ActionEveryLoop += () => {
            if (Move || MoveFaster)
            {
                if (!MoveFaster)
                {
                    Thread.Sleep(150); //Sleep 50ms more
                }

                if (!CheckShape())
                {
                    CurrentShape = SpawnNextShape();
                    ShapesCount++;
                    MoveFaster = false;
                    return;
                }

                MoveShapeDown();
                Render();
            }
        };

        StartRenderLoop();
    }

    private bool CheckShape()
    {
        Shape shape = CurrentShape;
        //bool isTouchgingSmth = true;

        //TODO

        return shape.Y > 1;// || isTouchgingSmth;
    }

    private Shape SpawnNextShape(int x_ = 11, int y_ = 41)
    {
        Shape shape = NextShape(x_, y_);
        PixelWriter.PixelLayer = shape.Layer + ShapesCount;
        for (int y = 0; y < shape.Pattern.Length; y++)
        {
            for (int x = 0; x < shape.Pattern[y].Length; x++)
            {
                char i = shape.Pattern[shape.Pattern.Length - y - 1][x];
                if (i == ' ')
                    continue;

                PixelWriter.WritePixel(shape.X + x, shape.Y + y, shape.Color);
            }
        }
        Render();
        return shape;
    }

    private Shape NextShape(int x, int y)
    {
        int i = Random.Next(Patterns.Length);
        string[] pattern = Patterns[i];

        return new Shape(pattern, x - pattern[0].Length / 2, y - pattern.Length, pattern[0].Length - 1, pattern.Length - 1, 3, Colors[i]);
    }

    private void MoveShapeDown()
    {
        Shape shape = CurrentShape;
        PixelPhysics.MovePixels(shape.X, shape.Y, shape.X + shape.Width, shape.Y + shape.Height, shape.Layer, 0, -1);
        shape.Y--;
    }

    public void PaintBasics()
    {
        int w = 29, h = 41;

        PixelWriter.PixelLayer = 4;

        //Walls
        PixelWriter.WritePixels(0, 0, w, 0, WallColor);
        PixelWriter.WritePixels(w, 0, w, h, WallColor);
        PixelWriter.WritePixels(0, h, w + 1, h, WallColor);
        PixelWriter.WritePixels(0, h, 0, 0, WallColor);

        //Panel
        PixelWriter.WritePixels(21, 1, w, h, WallColor);

        //Next shape
        TextWriter.WriteText(22, h - 1, " NEXT  SHAPES", RgbConsoleColor.Default);
        PixelWriter.WritePixels(22, h - 20 - 2, w, h - 2, RgbConsoleColor.Default);

        Render();
    }
}