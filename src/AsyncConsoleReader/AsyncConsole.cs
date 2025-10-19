using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AsyncConsoleReader.Interop;

namespace AsyncConsoleReader;

public static class AsyncConsole
{
    public static ValueTask<ConsoleKeyInfo> ReadKeyAsync(bool intercept = false, CancellationToken cancellationToken = default)
    {
        var source = Worker<ConsoleKeyInfo, bool>.Rent(
            static (intercept, ct) => ReadKey(intercept, ct),
            intercept,
            cancellationToken);
        ThreadPool.UnsafeQueueUserWorkItem(source, false);
        return source.AsValueTask();
    }

    public static ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        var source = Worker<string?, object?>.Rent(
            static (_, ct) => ReadLine(ct),
            null,
            cancellationToken);
        ThreadPool.UnsafeQueueUserWorkItem(source, false);
        return source.AsValueTask();
    }

    public static ValueTask<int> ReadAsync(CancellationToken cancellationToken = default)
    {
        var source = Worker<int, object?>.Rent(
            static (_, ct) => Read(ct),
            null,
            cancellationToken);
        ThreadPool.UnsafeQueueUserWorkItem(source, false);
        return source.AsValueTask();
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            CancellationTokenRegistration registration = default;
            if (cancellationToken.CanBeCanceled)
            {
                registration = cancellationToken.UnsafeRegister(_ =>
                {
                    var handle = GetStdHandle(STD_INPUT_HANDLE);
                    CancelIoEx(handle, IntPtr.Zero);
                }, null);
            }

            try
            {
                return Console.ReadLine();
            }
            finally
            {
                registration.Dispose();
            }
        }
        else
        {
            return ReadLineUnix(cancellationToken);
        }
    }

    unsafe static string? ReadLineUnix(CancellationToken cancellationToken)
    {
        int* pipefds = stackalloc int[2];
        pipe(pipefds);
        int readPipe = pipefds[0];
        int writePipe = pipefds[1];

        CancellationTokenRegistration registration = default;
        if (cancellationToken.CanBeCanceled)
        {
            registration = cancellationToken.UnsafeRegister(_ =>
            {
                unsafe
                {
                    byte* buf = stackalloc byte[1];
                    write(pipefds[1], buf, 1);
                }
            }, null);
        }

        try
        {
            // Set stdin to non-blocking
            int flags = fcntl(0, F_GETFL, 0);
            fcntl(0, F_SETFL, flags | O_NONBLOCK);

            var builder = new DefaultInterpolatedStringHandler();
            byte* buf = stackalloc byte[1];

            Span<uint> set = stackalloc uint[32];

            while (true)
            {
                FD_ZERO(set);
                FD_SET(0, set);
                FD_SET(readPipe, set);

                var nfds = Math.Max(0, readPipe) + 1;
                var setPtr = NativeMemory.Alloc(128);
                var setPtrSpan = new Span<byte>(setPtr, 128);
                MemoryMarshal.AsBytes(set).CopyTo(setPtrSpan);

                var result = select(nfds, (nint)setPtr, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                setPtrSpan.CopyTo(MemoryMarshal.AsBytes(set));
                NativeMemory.Free(setPtr);

                if (FD_ISSET(0, set))
                {
                    int n = read(0, buf, 1);
                    if (n > 0)
                    {
                        var c = (char)buf[0];
                        if (c == '\n')
                        {
                            return builder.ToStringAndClear();
                        }
                        builder.AppendFormatted(c);
                    }
                    else if (n == 0)
                    {
                        return builder.ToStringAndClear();
                    }
                }
                else if (FD_ISSET(readPipe, set))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
        }
        finally
        {
            // Restore blocking mode
            var flags = fcntl(0, F_GETFL, 0);
            fcntl(0, F_SETFL, flags & ~O_NONBLOCK);
        }
    }

    public static int Read(CancellationToken cancellationToken = default)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            CancellationTokenRegistration registration = default;
            if (cancellationToken.CanBeCanceled)
            {
                registration = cancellationToken.UnsafeRegister(_ =>
                {
                    var handle = GetStdHandle(STD_INPUT_HANDLE);
                    CancelIoEx(handle, IntPtr.Zero);
                }, null);
            }

            try
            {
                return Console.Read();
            }
            finally
            {
                registration.Dispose();
            }
        }
        else
        {
            return ReadUnix(cancellationToken);
        }
    }

    unsafe static int ReadUnix(CancellationToken cancellationToken)
    {
        int* pipefds = stackalloc int[2];
        pipe(pipefds);
        int readPipe = pipefds[0];
        int writePipe = pipefds[1];

        CancellationTokenRegistration registration = default;
        if (cancellationToken.CanBeCanceled)
        {
            registration = cancellationToken.UnsafeRegister(_ =>
            {
                unsafe
                {
                    byte* buf = stackalloc byte[1];
                    write(pipefds[1], buf, 1);
                }
            }, null);
        }

        try
        {
            // Set stdin to non-blocking
            int flags = fcntl(0, F_GETFL, 0);
            fcntl(0, F_SETFL, flags | O_NONBLOCK);

            byte* buf = stackalloc byte[1];
            Span<uint> set = stackalloc uint[32];

            FD_ZERO(set);
            FD_SET(0, set);
            FD_SET(readPipe, set);

            var nfds = Math.Max(0, readPipe) + 1;
            var setPtr = NativeMemory.Alloc(128);
            var setPtrSpan = new Span<byte>(setPtr, 128);
            MemoryMarshal.AsBytes(set).CopyTo(setPtrSpan);

            var result = select(nfds, (nint)setPtr, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            setPtrSpan.CopyTo(MemoryMarshal.AsBytes(set));
            NativeMemory.Free(setPtr);

            if (FD_ISSET(0, set))
            {
                int n = read(0, buf, 1);
                if (n > 0)
                {
                    return buf[0];
                }
                else if (n == 0)
                {
                    return -1; // EOF
                }
            }
            else if (FD_ISSET(readPipe, set))
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return -1; // Should not reach here
        }
        finally
        {
            // Restore blocking mode
            var flags = fcntl(0, F_GETFL, 0);
            fcntl(0, F_SETFL, flags & ~O_NONBLOCK);
        }
    }
}
