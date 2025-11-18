using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static ConsoleRenderEngine.Utilities;
using static ConsoleRenderEngine.Writer.ConsolePixelWriter;

namespace ConsoleRenderEngine.Writer;

public class ConsolePixelTable
{
    public readonly Dictionary<int, int> Storage = new();
    public readonly Queue<KeyValuePair<int, int>> StorageQueue = new();

    public RgbConsoleColor GetLayeredPixelColor(int combined, bool fromQueue = false)
    {
        if (!fromQueue)
            return Storage.TryGetValue(combined, out var output) ? RgbConsoleColor.Color(output) : null;
        
        var en = StorageQueue.GetEnumerator();
        while (en.MoveNext())
        {
            var pair = en.Current;
            if (pair.Key == combined)
                return RgbConsoleColor.Color(pair.Value);
        }

        return null;
    }

    public List<int> SelectPixels(int combined1, int combined2)
    {
        var list = new List<int>();

        Separate(combined1, out var x1, out var y1, out var layer1);
        Separate(combined2, out var x2, out var y2, out var layer2);

        if (layer1 != layer2)
            return null;

        var  topX = (x1 < x2 ? x2 : x1);
        var  bottomX = (x1 > x2 ? x2 : x1);

        var  topY = (y1 < y2 ? y2 : y1);
        var  bottomY = (y1 > y2 ? y2 : y1);

        for (var x = bottomX; x <= topX; x++)
            for (var y = bottomY; y <= topY; y++)
                list.Add(Combine(x, y, layer1));

        return list;
    }

    public RgbConsoleColor GetSurfacePixelColor(int x, int y, out int layer, int upUntilLayer = -1)
    {
        layer = -1;
        RgbConsoleColor color = null;

        foreach (var pair in Storage)
        {
            Separate(pair.Key, out var x2, out var y2, out var layer2);

            if (upUntilLayer >= 0 && layer2 >= upUntilLayer)
                return color;

            if (x != x2 || y != y2)
                continue;
            
            layer = Math.Max(layer, layer2);
            color = RgbConsoleColor.Color(pair.Value);
        }
        return color;
    }

    public void WritePixel(int combined, [AllowNull] RgbConsoleColor color, bool direct = false)
    {
        Separate(combined, out var x, out var y, out _);

        if (!IsInBounds(x, y))
            return;

        color ??= RgbConsoleColor.Color(-1);

        if (direct)
        {
            Storage.Remove(combined);
            if (color.Rgb != -1)
                Storage.Add(combined, color);
        }
        else
            StorageQueue.Enqueue(new KeyValuePair<int, int>(combined, color.Rgb));
    }

    public void WritePixels([DisallowNull] int[] combinedxylayer, RgbConsoleColor[] colors, bool direct)
    {
        for (var i = 0; i < combinedxylayer.Length; i++)
        {
            var color = colors[i];

            WritePixel(combinedxylayer[i], color, direct);
        }
    }

    public bool IsPixelVisible(int combined)
    {
        var hlayer = -1;
        foreach (var combined2 in Storage.Keys)
        {
            if (combined != combined2)
                continue;
            Separate(combined, out _, out _, out var layer);
            hlayer = Math.Max(hlayer, layer);

            if (hlayer > layer)
                return false;
        }

        return true;
    }
}