using System;
using Serilog.Context;

namespace LeanCode.Correlation
{
    public static class Correlate
    {
        public static IDisposable Logs(Guid correlationId)
        {
            return LogContext.PushProperty("CorrelationId", correlationId);
        }

        public static IDisposable ExecutionLogs(Guid executionId)
        {
            return LogContext.PushProperty("ExecutionId", executionId);
        }

        public static IDisposable Logs(string correlationId)
        {
            return LogContext.PushProperty("CorrelationId", correlationId);
        }
    }
}
