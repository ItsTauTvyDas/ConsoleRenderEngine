using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static ConsoleRenderEngine.Writer.ConsolePixelWriter;

namespace ConsoleRenderEngine.Writer;

public class ConsoleTextWriter
{
    public const char NullCharacter = '\u0000';

    public readonly Dictionary<int, Tuple<int, char, int, char>> TextStorage = new();
    public readonly Queue<Tuple<int, int, char, int, char>> TextStorageQueue = new();

    public void WriteText(int x, int y, [AllowNull] string text, [AllowNull] RgbConsoleColor color)
    {
        var list = new List<Tuple<int, char, int, char>>();
        var builder = new StringBuilder();

        if (text == null)
        {
            TextStorageQueue.Enqueue(new Tuple<int, int, char, int, char>(Utilities.Combine(x, y), -1, NullCharacter, -1, NullCharacter));
            return;
        }

        if (text.Length % 2 != 0)
            text += " ";

        for (var i = 1; i <= text.Length; i++)
        {
            builder.Append(text[i - 1]);
            if (i % 2 != 0)
                continue;
            list.Add(new Tuple<int, char, int, char>(color, builder[0], -1, builder[1]));
            builder.Clear();
        }

        var index = 0;
        foreach (var tuple in list)
        {
            var combined = Utilities.Combine(x + index, y);
            TextStorageQueue.Enqueue(new Tuple<int, int, char, int, char>(combined, tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
            index++;
        }
    }

    internal void WriteToStorage(Tuple<int, int, char, int, char> tuple)
    {
        var combined = tuple.Item1;
        var tuple2 = new Tuple<int, char, int, char>(tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);

        TextStorage.Remove(combined);
        TextStorage.Add(combined, tuple2);
    }

    public void TryGetColor(int x, int y, [MaybeNull] out Tuple<RgbConsoleColor, RgbConsoleColor> tuple)
    {
        TextStorage.TryGetValue(Utilities.Combine(x, y), out var tuple2);
        tuple = tuple2 != null ? new Tuple<RgbConsoleColor, RgbConsoleColor>(
            RgbConsoleColor.Color(tuple2.Item1),
            RgbConsoleColor.Color(tuple2.Item3)
        ) : null;
    }

    public void TryGetText(int x, int y, [MaybeNull] out Tuple<RgbConsoleColor, char, RgbConsoleColor, char> tuple)
    {
        TextStorage.TryGetValue(Utilities.Combine(x, y), out var tuple2);
        tuple = tuple2 != null ? new Tuple<RgbConsoleColor, char, RgbConsoleColor, char>(
            RgbConsoleColor.Color(tuple2.Item1),
            tuple2.Item2,
            RgbConsoleColor.Color(tuple2.Item3), tuple2.Item4
        ) : null;
    }

    public void TryGetRawValue(int x, int y, [MaybeNull] out Tuple<int, int, char, int, char> tuple)
    {
        var combined = Utilities.Combine(x, y);
        TextStorage.TryGetValue(combined, out var tuple2);
        tuple = tuple2 != null ? new Tuple<int, int, char, int, char>(combined, tuple2.Item1, tuple2.Item2, tuple2.Item3, tuple2.Item4) : null;
    }

    public void ClearText(int x, int y) => WriteText(x, y, null, null);
}