using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace LeanCode.AsyncTasks.Hangfire
{
    public class HangfireScheduler : ITaskScheduler
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<HangfireScheduler>();

        private readonly IBackgroundJobClient jobClient;
        private readonly IRecurringJobManager recurringJobs;
        private readonly string queue;

        public HangfireScheduler(
            IBackgroundJobClient backgroundJobClient,
            IRecurringJobManager recurringJobs,
            string queue)
        {
            this.jobClient = backgroundJobClient;
            this.recurringJobs = recurringJobs;
            this.queue = queue;
        }

        public Task ScheduleRecurring<TTask>(
            string cronExpression,
            TimeZoneInfo tz = null,
            string customId = null)
            where TTask : class, IAsyncTask
        {
            customId = customId ?? typeof(TTask).Name;
            tz = tz ?? TimeZoneInfo.Utc;
            var job = Job.FromExpression<AsyncTaskRunner<TTask>>(t => t.Run());

            recurringJobs.AddOrUpdate(
                customId, job, cronExpression,
                new RecurringJobOptions
                {
                    TimeZone = tz,
                    QueueName = queue
                });

            logger.Information(
                "Task {Task} registered as recurring task at '{CronExpr}' in {TimeZone} time zone",
                customId, cronExpression, tz.StandardName);
            return Task.CompletedTask;
        }

        public Task ScheduleNow<TTask, TParams>(TParams @params)
            where TTask : class, IAsyncTask<TParams>
        {
            var jobId = jobClient.Create<AsyncTaskRunner<TTask, TParams>>(
                t => t.Run(@params),
                new EnqueuedState(queue));
            logger.Information(
                "Task {Task} with params {@Params} enqueued as job {JobId}",
                typeof(TTask), @params, jobId);
            return Task.CompletedTask;
        }

        public Task ScheduleAfter<TTask, TParams>(TParams @params, TimeSpan delay)
            where TTask : class, IAsyncTask<TParams>
        {
            if (queue == BackgroundProcessingApp.DefaultQueue)
            {
                ScheduleAfterDefault<TTask, TParams>(@params, delay);
            }
            else
            {
                ScheduleAfterQueue<TTask, TParams>(@params, delay);
            }
            return Task.CompletedTask;
        }

        public Task ScheduleAt<TTask, TParams>(TParams @params, DateTimeOffset at)
            where TTask : class, IAsyncTask<TParams>
        {
            if (queue == BackgroundProcessingApp.DefaultQueue)
            {
                ScheduleAtDefault<TTask, TParams>(@params, at);
            }
            else
            {
                ScheduleAtQueue<TTask, TParams>(@params, at);
            }
            return Task.CompletedTask;
        }

        private void ScheduleAfterDefault<TTask, TParams>(
            TParams @params,
            TimeSpan delay)
            where TTask : class, IAsyncTask<TParams>
        {
            var jobId = jobClient.Schedule<AsyncTaskRunner<TTask, TParams>>(
                t => t.Run(@params),
                delay);
            logger.Information(
                "Task {Task} with params {@Params} scheduled to run after {Delay} as {JobId}",
                typeof(TTask), @params, delay, jobId);
        }

        private void ScheduleAfterQueue<TTask, TParams>(
            TParams @params,
            TimeSpan delay)
            where TTask : class, IAsyncTask<TParams>
        {
            var realJob = Job.FromExpression<AsyncTaskRunner<TTask, TParams>>(t => t.Run(@params));
            var jobId = jobClient.Create<IBackgroundJobClient>(
                c => c.Create(realJob, new EnqueuedState(queue)),
                new ScheduledState(delay));
            logger.Information(
                "Task {Task} with params {@Params} has been scheduled to run after {Delay} as {JobId}",
                typeof(TTask), @params, delay, jobId);
        }

        private void ScheduleAtDefault<TTask, TParams>(
            TParams @params,
            DateTimeOffset at)
            where TTask : class, IAsyncTask<TParams>
        {
            var jobId = jobClient.Schedule<AsyncTaskRunner<TTask, TParams>>(
                t => t.Run(@params),
                at);
            logger.Information(
                "Task {Task} with params {@Params} scheduled to run at {Offset} as {JobId}",
                typeof(TTask), @params, at, jobId);
        }

        private void ScheduleAtQueue<TTask, TParams>(
            TParams @params,
            DateTimeOffset at)
            where TTask : class, IAsyncTask<TParams>
        {
            var realJob = Job.FromExpression<AsyncTaskRunner<TTask, TParams>>(t => t.Run(@params));
            var jobId = jobClient.Create<IBackgroundJobClient>(
                c => c.Create(realJob, new EnqueuedState(queue)),
                new ScheduledState(at.UtcDateTime));
            logger.Information(
                "Task {Task} with params {@Params} scheduled to run at {Offset} as {JobId}",
                typeof(TTask), @params, at, jobId);
        }
    }
}
