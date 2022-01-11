// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;

namespace MikyM.Common.Utilities;

public class ConcurrentTaskQueue
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<TaskCompletionSource<bool>> _queue = new();

    public ConcurrentTaskQueue()
        => _semaphore = new SemaphoreSlim(1);

    public async Task<T> EnqueueAsync<T>(Func<Task<T>> task)
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
            return await task();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    public async Task EnqueueAsync(Func<Task> task)
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
            await task();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
