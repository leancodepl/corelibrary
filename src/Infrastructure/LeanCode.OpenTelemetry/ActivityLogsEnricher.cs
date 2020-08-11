using System.Diagnostics;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace LeanCode.OpenTelemetry
{
    public class ActivityLogsEnricher : ILogEventEnricher
    {
        private readonly string traceIdKey;
        private readonly string spanIdKey;

        public ActivityLogsEnricher(string traceIdKey, string spanIdKey)
        {
            this.spanIdKey = spanIdKey;
            this.traceIdKey = traceIdKey;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;
            if (activity != null)
            {
                var spanId = propertyFactory.CreateProperty(spanIdKey, activity.SpanId.ToString());
                var traceId = propertyFactory.CreateProperty(traceIdKey, activity.TraceId.ToString());
                logEvent.AddOrUpdateProperty(spanId);
                logEvent.AddOrUpdateProperty(traceId);
            }
        }
    }

    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Enrich log events with <c>SpanId</c> and <c>TraceId</c> from current activity (<see cref="Activity.Current" />)
        /// </summary>
        public static LoggerConfiguration FromCurrentActivity(
            this LoggerEnrichmentConfiguration config,
            string spanIdKey = "dd.spanId",
            string traceIdKey = "dd.traceId")
        {
            return config.With(new ActivityLogsEnricher(spanIdKey, traceIdKey));
        }
    }
}
