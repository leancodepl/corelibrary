using System;
using System.Threading.Tasks;

namespace LeanCode.AsyncTasks
{
    public interface ITaskScheduler
    {
        Task ScheduleRecurringAsync<TTask>(string cronExpression, TimeZoneInfo? tz = null, string? customId = null)
            where TTask : class, IAsyncTask;

        Task ScheduleNowAsync<TTask, TParams>(TParams @params)
            where TTask : class, IAsyncTask<TParams>;

        Task ScheduleAfterAsync<TTask, TParams>(TParams @params, TimeSpan delay)
            where TTask : class, IAsyncTask<TParams>;

        Task ScheduleAtAsync<TTask, TParams>(TParams @params, DateTimeOffset at)
            where TTask : class, IAsyncTask<TParams>;
    }
}
