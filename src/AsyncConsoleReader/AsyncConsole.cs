using System.Buffers;

namespace AsyncConsoleReader;

public static class AsyncConsole
{
    public static async ValueTask<ConsoleKeyInfo> ReadKeyAsync(bool intercept = false, CancellationToken cancellationToken = default)
    {
        var source = Worker<ConsoleKeyInfo, bool>.Rent(
            static (intercept, ct) => ReadKey(intercept, ct),
            intercept,
            cancellationToken);

        try
        {
            ThreadPool.UnsafeQueueUserWorkItem(source, false);
            return await source.AsValueTask();
        }
        finally
        {
            Worker<ConsoleKeyInfo, bool>.Return(source);
        }
    }

    public static async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        var source = Worker<string?, object?>.Rent(
            static (_, ct) => ReadLine(ct),
            null,
            cancellationToken);

        try
        {
            ThreadPool.UnsafeQueueUserWorkItem(source, false);
            return await source.AsValueTask();
        }
        finally
        {
            Worker<string?, object?>.Return(source);
        }
    }

    public static async ValueTask<int> ReadAsync(CancellationToken cancellationToken = default)
    {
        var source = Worker<int, object?>.Rent(
            static (_, ct) => Read(ct),
            null,
            cancellationToken);

        try
        {
            ThreadPool.UnsafeQueueUserWorkItem(source, false);
            return await source.AsValueTask();
        }
        finally
        {
            Worker<int, object?>.Return(source);
        }
    }

    public static ConsoleKeyInfo ReadKey(bool intercept = false, CancellationToken cancellationToken = default)
    {
        while (!Console.KeyAvailable)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return Console.ReadKey(intercept);
    }

    public static string? ReadLine(CancellationToken cancellationToken = default)
    {
        var buffer = ArrayPool<char>.Shared.Rent(1024);
        var tail = 0;
        var pos = 0;

        try
        {
            while (true)
            {
                var key = ReadKey(true, cancellationToken);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return buffer.AsSpan(0, tail).ToString();
                    case ConsoleKey.Backspace:
                        if (pos > 0)
                        {
                            pos--;
                            tail--;

                            buffer.AsSpan(pos + 1, tail - pos).CopyTo(buffer.AsSpan(pos));

                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                            Console.Write(buffer.AsSpan(pos, tail - pos).ToString() + " ");
                            Console.SetCursorPosition(Console.CursorLeft - (tail - pos + 1), Console.CursorTop);
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (pos > 0)
                        {
                            pos--;
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (pos < tail)
                        {
                            pos++;
                            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                        }
                        break;
                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            if (tail >= buffer.Length)
                            {
                                var newBuffer = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                                buffer.AsSpan(0, tail).CopyTo(newBuffer);
                                ArrayPool<char>.Shared.Return(buffer);
                                buffer = newBuffer;
                            }

                            buffer[tail++] = key.KeyChar;
                            Console.Write(key.KeyChar);
                            pos++;
                        }
                        break;
                }
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    public static int Read(CancellationToken cancellationToken = default)
    {
        var key = ReadKey(true, cancellationToken);
        Console.Write(key.KeyChar);
        return key.KeyChar;
    }
}
