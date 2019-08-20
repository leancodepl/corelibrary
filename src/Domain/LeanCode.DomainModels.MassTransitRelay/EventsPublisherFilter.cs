using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using LeanCode.DomainModels.EventsExecution;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class EventsPublisherFilter : IFilter<ConsumeContext>
    {
        private readonly AsyncEventsInterceptor interceptor;

        public EventsPublisherFilter(AsyncEventsInterceptor interceptor)
        {
            this.interceptor = interceptor;
        }

        public void Probe(ProbeContext context)
        {
        }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            interceptor.Configure();
            await next.Send(context);
            var queue = interceptor.CaptureQueue();

            var publishTasks = queue.Select(evt => context.Publish(evt));

            await Task.WhenAll(publishTasks);
        }
    }
}
