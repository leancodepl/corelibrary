using System;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes.Internals.Extensions;

namespace LeanCode.DomainModels.MassTransitRelay.Testing
{
    // Based on https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Coordination/AsyncManualResetEvent.cs
    public sealed class AsyncManualResetEvent
    {
        private readonly object mutex;
        private TaskCompletionSource<object?> tcs;

        public AsyncManualResetEvent(bool set)
        {
            mutex = new object();
            tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (set)
            {
                tcs.TrySetResult(null);
            }
        }

        public AsyncManualResetEvent()
            : this(false)
        { }

        public bool IsSet
        {
            get
            {
                lock (mutex)
                {
                    return tcs.Task.IsCompleted;
                }
            }
        }

        public Task WaitAsync()
        {
            lock (mutex)
            {
                return tcs.Task;
            }
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            var waitTask = WaitAsync();
            if (waitTask.IsCompleted)
            {
                return waitTask;
            }
            else
            {
                return waitTask.OrCanceled(cancellationToken);
            }
        }

        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            try
            {
                await WaitAsync().OrTimeout(timeout);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        public void Set()
        {
            lock (mutex)
            {
                tcs.TrySetResult(null);
            }
        }

        public void Reset()
        {
            lock (mutex)
            {
                if (tcs.Task.IsCompleted)
                {
                    tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                }
            }
        }
    }
}
