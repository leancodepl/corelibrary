using System;
using System.Collections.Generic;
using Polly;

namespace LeanCode.DomainModels.EventsExecution
{
    public sealed class RetryPolicies
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<RetryPolicies>();

        private static IEnumerable<TimeSpan> GetEventRetryWaitTimes()
        {
            yield return TimeSpan.FromSeconds(0.2);
            yield return TimeSpan.FromSeconds(0.4);
            yield return TimeSpan.FromSeconds(0.8);
            yield return TimeSpan.FromSeconds(1.6);
            yield return TimeSpan.FromSeconds(3.2);
        }

        public Polly.Retry.AsyncRetryPolicy EventHandlerPolicy { get; }

        public RetryPolicies()
        {
            EventHandlerPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    GetEventRetryWaitTimes(),
                    (e, _) => logger.Error(e, "Cannot execute handler for the event, retrying"));
        }
    }
}
