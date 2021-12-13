using System.Collections.Concurrent;

namespace MikyM.Common.Utilities;

public class TaskConcurrentQueue
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<TaskCompletionSource<bool>> _queue = new();

    public TaskConcurrentQueue()
        => _semaphore = new SemaphoreSlim(1);

    public async Task<T> EnqueueAsync<T>(Func<Task<T>> taskGenerator)
    {
        var tcs = new TaskCompletionSource<bool>();
        _queue.Enqueue(tcs);
        await _semaphore.WaitAsync().ContinueWith(t =>
        {
            if (!_queue.TryDequeue(out var popped)) return;

            popped.SetResult(true);
        });
        try
        {
            return await taskGenerator();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    public async Task EnqueueAsync(Func<Task> taskGenerator)
    {
        var tcs = new TaskCompletionSource<bool>();
        _queue.Enqueue(tcs);
        await _semaphore.WaitAsync().ContinueWith(t =>
        {
            if (_queue.TryDequeue(out var popped))
                popped.SetResult(true);
        });
        try
        {
            await taskGenerator();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
