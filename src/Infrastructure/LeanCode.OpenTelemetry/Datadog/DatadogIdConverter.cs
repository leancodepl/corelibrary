using System.Diagnostics;

namespace LeanCode.OpenTelemetry.Datadog;

[Obsolete("This class is deprecated. Datadog now supports proper W3C trace context.")]
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
