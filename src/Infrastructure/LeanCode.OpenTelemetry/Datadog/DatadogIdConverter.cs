using System;
using System.Diagnostics;

namespace LeanCode.OpenTelemetry.Datadog;

public static class DatadogIdConverter
{
    public static ulong ToDatadogFormat(this ActivitySpanId spanId)
    {
        Span<byte> buffer = stackalloc byte[8];
        spanId.CopyTo(buffer);

        if (BitConverter.IsLittleEndian)
        {
            // the ids are in big endian
            buffer.Reverse();
        }

        return BitConverter.ToUInt64(buffer);
    }

    /// <remarks>The conversion truncates the trace id to the first 8 bytes</remarks>
    public static ulong ToDatadogFormat(this ActivityTraceId traceId)
    {
        Span<byte> buffer = stackalloc byte[16];
        traceId.CopyTo(buffer);

        if (BitConverter.IsLittleEndian)
        {
            // the ids are in big endian
            buffer.Reverse();
        }

        return BitConverter.ToUInt64(buffer);
    }
}
