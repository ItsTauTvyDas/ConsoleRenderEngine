using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace ConsoleRenderEngine.Writer;

public sealed class ConsolePixelWriter
{
    public ConsolePixelTable PixelTable { private set; get; } = new ConsolePixelTable();
    public int PixelLayer = 0;

    public void WritePixel(int x, int y, [AllowNull] RgbConsoleColor color) => PixelTable.WritePixel(Utilities.Combine(x, y, PixelLayer), color);

    public void WritePixel(int x, int y, int red, int green, int blue) => WritePixel(x, y, RgbConsoleColor.Color(red, green, blue));

    public void WritePixel(int x, int y, int rgb) => WritePixel(x, y, RgbConsoleColor.Color(rgb));

    public RgbConsoleColor GetSurfacePixelColor(int x, int y, out int layer) => PixelTable.GetSurfacePixelColor(x, y, out layer);

    public RgbConsoleColor GetPixelColor(int x, int y, int layer) => PixelTable.GetLayeredPixelColor(Utilities.Combine(x, y, layer));

    public void WritePixels(int x1, int y1, int x2, int y2, [AllowNull] RgbConsoleColor color)
    {
        //if (!Utilities.IsInBounds(x1, y1) || !Utilities.IsInBounds(x2, y2))
        //    return;

        var xMin = Math.Min(x1, x2);
        var yMin = Math.Min(y1, y2);

        var xMax = Math.Max(x1, x2);
        var yMax = Math.Max(y1, y2);

        var i = 0;
        int[] combined;
        RgbConsoleColor[] colors;

        if (x1 == x2 && y1 != y2) //Horizontal line
        {
            combined = new int[yMax - yMin];
            colors = new RgbConsoleColor[combined.Length];

            for (var y = yMin; y < yMax; y++)
            {
                combined[i] = Utilities.Combine(x1, y, PixelLayer);
                colors[i] = color;
                i++;
            }
        }
        else if (x1 != x2 && y1 == y2) //Vertical line
        {
            combined = new int[xMax - xMin];
            colors = new RgbConsoleColor[combined.Length];

            for (var x = xMin; x < xMax; x++)
            {
                combined[i] = Utilities.Combine(x, y1, PixelLayer);
                colors[i] = color;
                i++;
            }
        }
        else if (x1 == x2 && y1 == y2)
        {
            WritePixel(x1, y2, color);
            return;
        }
        else
        {
            combined = new int[yMax * xMax];
            colors = new RgbConsoleColor[combined.Length];

            for (var x = xMin; x < xMax; x++)
            for (var y = yMin; y < yMax; y++)
            {
                combined[i] = Utilities.Combine(x, y, PixelLayer);
                colors[i] = color;
                i++;
            }
        }

        PixelTable.WritePixels(combined, colors, false);
    }

    public void WriteImage(int x, int y, [DisallowNull] Bitmap imageData)
    {
        for (var x2 = 0; x2 < imageData.Width; x2++)
            for (var y2 = 0; y2 < imageData.Height; y2++)
                WritePixel(x2 + x, imageData.Height - y2 + y - 1, RgbConsoleColor.FromDrawingColor(imageData.GetPixel(x2, y2)));
    }

    public class RgbConsoleColor
    {
        public readonly int Rgb;

        private RgbConsoleColor(int red, int green, int blue)
        {
            Rgb = red;
            Rgb = (Rgb << 8) + green;
            Rgb = (Rgb << 8) + blue;
        }

        private RgbConsoleColor(int rgb) => Rgb = rgb;

        public static RgbConsoleColor Color(int red, int green, int blue) => new(red, green, blue);

        public static RgbConsoleColor Color(int rgb) => new(rgb);

        public static RgbConsoleColor FromDrawingColor(Color color) => new(color.R, color.G, color.B);

        public bool IsColor(int red, int green, int blue) => Rgb == Color(red, green, blue).Rgb;

        public void GetRgb(out int red, out int green, out int blue)
        {
            red = Rgb >> 16 & 0xff;
            green = Rgb >> 8 & 0xff;
            blue = Rgb & 0xff;
        }

        public void SetConsoleColor(bool foreground = false)
        {
            if (Rgb == -1)
                return;

            var type = foreground ? "38;2" : "48;2";
            GetRgb(out var red, out var green, out var blue);
            FastConsole.Write($"\x1b[{type};{red};{green};{blue}m");
        }

        public RgbConsoleColor Copy() => new(Rgb);
        
        public Color ToDrawingColor()
        {
            GetRgb(out var red, out var green, out var blue);
            return System.Drawing.Color.FromArgb(red, green, blue);
        }

        public override string ToString() => $"{Rgb}";

        public override int GetHashCode() => Rgb;

        public static RgbConsoleColor Random()
        {
            var rnd = new Random();
            return Color(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        }

        public static implicit operator int(RgbConsoleColor color) => color.Rgb;
        public static explicit operator RgbConsoleColor(int rgb) => new(rgb);

        public static RgbConsoleColor Default { get; } = new(0, 0, 0);
    }

    public class Point
    {
        public int X, Y;

        public Point(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        [return: NotNull]
        public Point Copy() => new(X, Y);

        public int Combine(int layer = -1) => Utilities.Combine(X, Y, layer);
        
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override bool Equals(object obj) => obj is Point p && X == p.X && Y == p.Y;

        public static bool operator ==(Point left, Point right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Point left, Point right) => !(left == right);
    }
}