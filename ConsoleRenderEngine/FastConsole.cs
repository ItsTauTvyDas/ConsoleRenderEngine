using ConsoleRenderEngine.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using static ConsoleRenderEngine.Writer.ConsolePixelWriter;

namespace ConsoleRenderEngine;

public static class FastConsole
{
    private static readonly BufferedStream Stream;
    public static bool CanReadAsyncKey;
    public static bool AlwaysReadKeyAsync;
    public static ConsoleKeyInfo LastAsyncPressedKey { private set; get; }

    static FastConsole()
    {
        Console.OutputEncoding = Encoding.Unicode;  // crucial

        // avoid special "ShadowBuffer" for hard-coded size 0x14000 in 'BufferedStream' 
        Stream = new BufferedStream(Console.OpenStandardOutput(), 0x15000);

        var readKeyAsyncThread = new Thread(() =>
        {
            while (true)
            {
                if (!AlwaysReadKeyAsync && !CanReadAsyncKey)
                    continue;
                LastAsyncPressedKey = ReadKey();
                CanReadAsyncKey = false;
            }
            // No idea why IDE complains about this...?
            // ReSharper disable once FunctionNeverReturns
        });
        readKeyAsyncThread.Start();
    }

    public static void WriteLine() => WriteLine("");

    public static void WriteLine(object obj) => Write(obj + Environment.NewLine);

    public static ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

    public static void AsyncReadKey() => CanReadAsyncKey = true;

    public static void WriteAll(ConsolePixelWriter pwriter, ConsoleTextWriter twriter)
    {
        Console.SetCursorPosition(0, 0);

        //The reason I add to a list, is because to avoid flickering (?) bug.
        //E.g. in snake game, when collecting apples, second (if I recall) tail pixel flickers
        //Sure there could be mistake in my snake game code... But whatever, if it works, it works :)
        var plist = new List<KeyValuePair<int, int>>();

        var pqueue = pwriter.PixelTable.StorageQueue;
        var pstorage = pwriter.PixelTable.Storage;

        var tqueue = twriter.TextStorageQueue;
        var tstorage = twriter.TextStorage;

        while (pqueue.Count != 0)
            plist.Add(pqueue.Dequeue());

        while (tqueue.Count != 0)
            twriter.WriteToStorage(tqueue.Dequeue());

        int h = Console.WindowHeight - 1, w = Console.WindowWidth / 2 - 1;
        for (var y = h; y >= 0; y--)
        {
            for (var x = 0; x <= w; x++)
            {
                Tuple<int, int> pixel = null;
                var layer = -1;

                int color1 = -1, color2 = -1;
                char char1 = ' ', char2 = ' ';

                //Render pixels
                foreach (var pix in plist)
                {
                    Utilities.Separate(pix.Key, out int x2, out int y2, out int layer2);
                    if (x != x2 || y != y2 || layer2 <= layer)
                        continue;
                    pixel = new Tuple<int, int>(pix.Key, pix.Value);
                    layer = layer2;
                }

                lock (pwriter.PixelTable.Storage)
                    foreach (var pix in pstorage) {
                        Utilities.Separate(pix.Key, out _, out _, out int layer2);
                        var combined = Utilities.Combine(x, y, layer2);
                        if (combined != pix.Key || layer2 <= layer)
                            continue;
                        layer = layer2;
                        pixel = new Tuple<int, int>(combined, pstorage[combined]);
                    }

                var combined2 = Utilities.Combine(x, y); //How can I not use "combined" as variable name???? previous one is in another scope tho...

                //Render text
                if (tstorage.ContainsKey(combined2))
                    lock (twriter.TextStorage)
                    {
                        tstorage.TryGetValue(combined2, out var value);
                        if (value == null)
                            continue;
                        char1 = value.Item2;
                        char2 = value.Item4;

                        color1 = value.Item1;
                        color2 = value.Item3;
                    }

                RgbConsoleColor.Color(pixel?.Item2 ?? 0).SetConsoleColor();

                RgbConsoleColor.Color(color1).SetConsoleColor(true);
                Write(char1);
                RgbConsoleColor.Color(color2).SetConsoleColor(true);
                Write(char2);

                RgbConsoleColor.Color(0).SetConsoleColor();
            }

            RgbConsoleColor.Color(0).SetConsoleColor();
            if (y - 1 > 0) //Skip last new line
                WriteLine(); //After writing whole line, skip to next line
        }

        foreach (var elem in plist)
            lock (pwriter.PixelTable.Storage)
                pwriter.PixelTable.WritePixel(elem.Key, RgbConsoleColor.Color(elem.Value), true);
    }

    public static void Write(object obj)
    {
        var s = $"{obj}";
        // avoid endless 'GetByteCount' dithering in 'Encoding.Unicode.GetBytes(s)'
        var rgb = new byte[s.Length << 1];
        Encoding.Unicode.GetBytes(s, 0, s.Length, rgb, 0);

        lock (Stream)
            Stream.Write(rgb, 0, rgb.Length);
    }

    public static void Flush()
    {
        lock (Stream)
            Stream.Flush();
    }
}