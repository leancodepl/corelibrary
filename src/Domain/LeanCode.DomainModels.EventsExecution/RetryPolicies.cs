using System;
using Polly;

namespace LeanCode.DomainModels.EventsExecution
{
    public sealed class RetryPolicies
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<RetryPolicies>();

        private static readonly TimeSpan[] EventRetryWaitTimes = new[]
        {
            TimeSpan.FromSeconds(0.2),
            TimeSpan.FromSeconds(0.4),
            TimeSpan.FromSeconds(0.8),
            TimeSpan.FromSeconds(1.6),
            TimeSpan.FromSeconds(3.2)
        };

        public Polly.Retry.AsyncRetryPolicy EventHandlerPolicy { get; }

        public RetryPolicies()
        {
            EventHandlerPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(EventRetryWaitTimes,
                    (e, _) => logger.Error(e, "Cannot execute handler for the event, retrying"));
        }
    }
}
