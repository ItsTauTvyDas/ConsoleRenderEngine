using ConsoleRenderEngine.Writer;
using static ConsoleRenderEngine.Writer.ConsolePixelWriter;

namespace ConsoleRenderEngine;

public class ConsolePixelPhysics
{
    public ConsolePixelWriter PixelWriter;
    public bool IgnoreNulls;

    public ConsolePixelPhysics(ConsolePixelWriter pixelWriter) => PixelWriter = pixelWriter;

    public void MovePixelTo(int x1, int y1, int layer1, int x2, int y2, int layer2) =>
        MovePixelTo(Utilities.Combine(x1, y1, layer1), Utilities.Combine(x2, y2, layer2));

    public void MovePixelTo(int combined1, int combined2)
    {
        if (combined1 == combined2)
            return;

        RgbConsoleColor color = PixelWriter.PixelTable.GetLayeredPixelColor(combined1);

        if (IgnoreNulls && (color == null || color.Rgb == 0))
            return;

        PixelWriter.PixelTable.WritePixel(combined1, null, true);
        PixelWriter.PixelTable.WritePixel(combined2, color, true);
    }

    public void MovePixel(int x, int y, int layer, int movementAmountX, int movementAmountY) =>
        MovePixels(Utilities.Combine(x, y, layer), -1, movementAmountX, movementAmountY);

    public void MovePixels(int x1, int y1, int x2, int y2, int layer, int movementAmountX, int movementAmountY) =>
        MovePixels(
            Utilities.Combine(x1, y1, layer),
            Utilities.Combine(x2, y2, layer),
            movementAmountX, movementAmountY
        );

    public void MovePixels(int combinedA, int combinedB, int movementAmountX, int movementAmountY, int newLayer = -1)
    {
        Utilities.Separate(combinedA, out var x1, out var y1, out var layer1);

        if (combinedA == combinedB || combinedB == -1)
        {
            MovePixelTo(x1, y1, layer1, x1 + movementAmountX, y1 + movementAmountY, layer1);
            return;
        }

        Utilities.Separate(combinedB, out var x2, out var y2, out var layer2);

        if (layer1 != layer2)
            return;

        var topX = x1 < x2 ? x2 : x1;
        var bottomX = x1 > x2 ? x2 : x1;

        var topY = y1 < y2 ? y2 : y1;
        var bottomY = y1 > y2 ? y2 : y1;

        for (var x = bottomX; x <= topX; x++)
            for (var y = bottomY; y <= topY; y++)
                MovePixelTo(x, y, layer1, x + movementAmountX, y + movementAmountY, newLayer >= 0 ? newLayer : layer1);
    }

    public void SwapPixels(int x1, int y1, int layer1, int x2, int y2, int layer2) =>
        SwapPixels(Utilities.Combine(x1, y1, layer1), Utilities.Combine(x2, y2, layer2));

    public void SwapPixels(int combined1, int combined2)
    {
        var color1 = PixelWriter.PixelTable.GetLayeredPixelColor(combined1);
        var color2 = PixelWriter.PixelTable.GetLayeredPixelColor(combined2);
        PixelWriter.PixelTable.WritePixel(combined1, color2);
        PixelWriter.PixelTable.WritePixel(combined2, color1);
    }
}