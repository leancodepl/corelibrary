using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenTelemetry.Context.Propagation;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    public class ActivityContextEFConverter : ValueConverter<ActivityContext, string>
    {
        private static readonly TraceContextPropagator TraceFormat = new TraceContextPropagator();

        public ActivityContextEFConverter()
            : base(
                ctx => From(ctx),
                s => To(s))
        { }

        private static string From(ActivityContext ctx)
        {
            var carrier = new Carrier();
            var propagationCtx = new PropagationContext(ctx, default);

            TraceFormat.Inject(propagationCtx, carrier, (c, key, value) =>
            {
                if (key == "traceparent")
                {
                    c.Value = value;
                }
            });
            return carrier.Value;
        }

        private static ActivityContext To(string value)
        {
            var carrier = new Carrier { Value = value };
            return TraceFormat.Extract(default, carrier, (c, key) =>
            {
                if (key == "traceparent")
                {
                    return new[] { c.Value };
                }
                else
                {
                    return Array.Empty<string>();
                }
            }).ActivityContext;
        }

        // OpenTelemetry implementation for .NET does not have separate ActivityContext serializer.
        // Instead serialization is very tightly coupled with the process of enriching other payloads with 'traceparent' and 'tracestate' headers,
        // so conversion is done in such clumsy way
        private class Carrier
        {
            public string Value { get; set; } = null!;
        }
    }
}
