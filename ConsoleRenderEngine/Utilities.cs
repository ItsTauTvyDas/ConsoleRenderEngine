using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ConsoleRenderEngine;

public static class Utilities
{
    public static int ConvertToConsoleY(int y) => Console.WindowHeight - y - 1;

    public static int ConvertToGameY(int y) => Console.WindowHeight + y;

    public static bool IsInBounds(int x, int y) => x >= 0 && y >= 0 && x <= Console.WindowWidth - 2 && y <= Console.WindowHeight;

    public static void SetWriteLocation(int x, int y)
    {
        x *= 2;
        y = ConvertToConsoleY(y);

        if (!IsInBounds(x, y))
            return;

        Console.SetCursorPosition(x, y);
    }

    public static int SetLayer(int combined, int newLayer)
    {
        Separate(combined, out var x, out var y, out _);
        return Combine(x, y, newLayer);
    }

    public static int SetX(int combined, int newX)
    {
        Separate(combined, out _, out var y, out var layer);
        return Combine(newX, y, layer);
    }

    public static int SetY(int combined, int newY)
    {
        Separate(combined, out var x, out _, out var layer);
        return Combine(x, newY, layer);
    }

    public static int Combine(int x, int y, int layer = -1)
    {
        int i = x;
        i = (i << 8) + y;
        if (layer >= 0)
            i = (i << 8) + layer;
        return i;
    }

    public static void Separate(int combined, out int x, out int y, out int layer)
    {
        x = (combined >> 16) & 0xff;
        y = (combined >> 8) & 0xff;
        layer = combined & 0xff;
    }

    public static void Separate(int combined, out int x, out int y)
    {
        x = (combined >> 8) & 0xff;
        y = combined & 0xff;
    }

    public static void RemoveLayer(int combined, out int newCombined)
    {
        Separate(combined, out var x, out var y, out _);
        newCombined = Combine(x, y);
    }

    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using var graphics = Graphics.FromImage(destImage);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using var wrapMode = new ImageAttributes();
        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

        return destImage;
    }
}