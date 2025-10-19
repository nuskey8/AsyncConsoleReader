using System.Collections.Concurrent;
using System.Threading.Tasks.Sources;

namespace AsyncConsoleReader;

sealed class Worker<T, TState> : IThreadPoolWorkItem, IValueTaskSource<T>
{
    static readonly ConcurrentStack<Worker<T, TState>> pool = new();

    TState? state;
    Func<TState, CancellationToken, T>? func;
    CancellationToken cancellationToken;
    ManualResetValueTaskSourceCore<T> core;

    public static Worker<T, TState> Rent(
        Func<TState, CancellationToken, T> func,
        TState state,
        CancellationToken cancellationToken)
    {
        if (!pool.TryPop(out var command))
        {
            command = new();
        }

        command.state = state;
        command.func = func;
        command.cancellationToken = cancellationToken;

        return command;
    }

    public static void Return(Worker<T, TState> command)
    {
        command.state = default;
        command.func = default;
        command.cancellationToken = default;
        pool.Push(command);
    }

    public void Execute()
    {
        try
        {
            var result = func!(state!, cancellationToken);
            core.SetResult(result);
        }
        catch (Exception ex)
        {
            core.SetException(ex);
        }
    }

    public ValueTask<T> AsValueTask()
    {
        return new ValueTask<T>(this, core.Version);
    }

    public T GetResult(short token)
    {
        try
        {
            return core.GetResult(token);
        }
        finally
        {
            core.Reset();
        }
    }
    public ValueTaskSourceStatus GetStatus(short token)
        => core.GetStatus(token);
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        => core.OnCompleted(continuation, state, token, flags);
}
