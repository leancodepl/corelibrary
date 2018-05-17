using System;
using System.Threading.Tasks;

namespace LeanCode.AsyncTasks
{
    public interface ITaskScheduler
    {
        Task ScheduleRecurring<TTask>(
            string cronExpression,
            TimeZoneInfo tz = null,
            string customId = null)
            where TTask : class, IAsyncTask;

        Task ScheduleNow<TTask, TParams>(TParams @params)
            where TTask : class, IAsyncTask<TParams>;

        Task ScheduleAfter<TTask, TParams>(TParams @params, TimeSpan delay)
            where TTask : class, IAsyncTask<TParams>;

        Task ScheduleAt<TTask, TParams>(TParams @params, DateTimeOffset at)
            where TTask : class, IAsyncTask<TParams>;
    }
}
