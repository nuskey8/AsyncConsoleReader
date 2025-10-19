using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AsyncConsoleReader;

internal static partial class Interop
{
    public const int STD_INPUT_HANDLE = -10;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static unsafe partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static unsafe partial bool CancelIoEx(IntPtr handle, IntPtr lpOverlapped);

    [LibraryImport("libSystem.dylib", SetLastError = true)]
    public static unsafe partial int read(int fd, byte* buffer, int count);

    [LibraryImport("libSystem.dylib", SetLastError = true)]
    public static unsafe partial int write(int fd, byte* buffer, int count);

    [LibraryImport("libSystem.dylib", SetLastError = true)]
    public static unsafe partial int pipe(int* fds);

    [LibraryImport("libSystem.dylib", SetLastError = true)]
    public static unsafe partial int fcntl(int fd, int cmd, int arg);

    [LibraryImport("libSystem.dylib", SetLastError = true)]
    public static unsafe partial int select(int nfds, IntPtr readfds, IntPtr writefds, IntPtr exceptfds, IntPtr timeout);

    public const int F_GETFL = 3;
    public const int F_SETFL = 4;
    public const int O_NONBLOCK = 0x0004;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FD_SET(int fd, Span<uint> set)
    {
        int index = fd / 32;
        int bit = fd % 32;
        set[index] |= 1u << bit;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FD_ISSET(int fd, Span<uint> set)
    {
        int index = fd / 32;
        int bit = fd % 32;
        return (set[index] & (1u << bit)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FD_ZERO(Span<uint> set)
    {
        set.Clear();
    }
}