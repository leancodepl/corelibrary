using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace LeanCode.DomainModels.RequestEventsExecutor
{
    sealed class EventExecutor
    {
        private static readonly TimeSpan[] EventRetryWaitTimes = new[]
        {
            TimeSpan.FromSeconds(0.2),
            TimeSpan.FromSeconds(0.4),
            TimeSpan.FromSeconds(0.8),
            TimeSpan.FromSeconds(1.6),
            TimeSpan.FromSeconds(3.2)
        };

        private const string HandlerTypeKey = "handlerType";
        private const string DomainEventKey = "domainEvent";

        private readonly Serilog.ILogger logger
            = Serilog.Log.ForContext<EventExecutor>();

        private readonly IServiceScopeFactory scopeFactory;
        private readonly PerRequestEventsStorage storage;

        private readonly Policy policy;

        public EventExecutor(IServiceScopeFactory scopeFactory,
            PerRequestEventsStorage storage)
            : this(scopeFactory, storage, false)
        { }

        public EventExecutor(IServiceScopeFactory scopeFactory,
            PerRequestEventsStorage storage, bool disableWait)
        {
            this.scopeFactory = scopeFactory;
            this.storage = storage;

            var builder = Policy
                .Handle<Exception>();
            if (disableWait)
            {
                policy = builder.RetryAsync(EventRetryWaitTimes.Length,
                    (e, _) => logger.Error(e, "Cannot execute handler for the event, retrying"));
            }
            else
            {
                policy = builder.WaitAndRetryAsync(EventRetryWaitTimes,
                    (e, _) => logger.Error(e, "Cannot execute handler for the event, retrying"));
            }
        }

        public async Task Execute(RequestDelegate next, HttpContext context)
        {
            var storedEvents = new Queue<IDomainEvent>();
            if (await ExecuteHttpRequest(next, context, storedEvents).ConfigureAwait(false))
            {
                await ExecuteEvents(storedEvents).ConfigureAwait(false);
            }
        }

        private async Task<bool> ExecuteHttpRequest(RequestDelegate next, HttpContext context, Queue<IDomainEvent> storedEvents)
        {
            logger.Debug("Executing request to {Path} and capturing its events", context.Request.Path);

            storage.UseNewQueue();
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occured during execution of request to {Path}, skipping event execution",
                    context.Request.Path);
                throw;
            }

            if (context.Response != null && !IsSuccessful(context.Response))
            {
                logger.Warning(
                    "Execution of request to {Path} resulted in {StatusCode} status code, skipping event execution",
                    context.Request.Path, context.Response.StatusCode);
                return false;
            }

            var captured = storage.CaptureQueue();
            CopyEvents(captured, storedEvents);
            return true;
        }

        private async Task ExecuteEvents(Queue<IDomainEvent> storedEvents)
        {
            using (var innerScope = scopeFactory.CreateScope())
            {
                var resolver = innerScope.ServiceProvider
                    .GetRequiredService<IDomainEventHandlerResolver>();
                while (storedEvents.Count > 0)
                {
                    var domainEvent = storedEvents.Dequeue();
                    var handlers = resolver.FindEventHandlers(domainEvent.GetType());
                    logger.Verbose("Executing event {Event} with {N} handlers", domainEvent, handlers.Count);

                    foreach (var handler in handlers)
                    {
                        logger.Verbose("Executing handler {Handler} with event {Event}", handler.GetType(), domainEvent);
                        await ExecuteEvent(handler, domainEvent, storedEvents).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task ExecuteEvent(IDomainEventHandlerWrapper handler, IDomainEvent domainEvent, Queue<IDomainEvent> output)
        {
            var handlerType = handler.GetType();
            var result = await policy
                .ExecuteAndCaptureAsync(async () =>
                {
                    storage.UseNewQueue();
                    await handler.HandleAsync(domainEvent).ConfigureAwait(false);
                    return storage.CaptureQueue();
                })
                .ConfigureAwait(false);

            if (result.Outcome == OutcomeType.Failure)
            {
                logger.Fatal(result.FinalException, "Cannot execute handler {Handler} with event {Event} after {N} retries",
                    handlerType, domainEvent, EventRetryWaitTimes.Length);
            }
            else
            {
                CopyEvents(result.Result, output);
                logger.Information("Handler {Handler} with event {Event} executed successfully", handlerType, domainEvent);
            }
        }

        private void CopyEvents(ConcurrentQueue<IDomainEvent> queue, Queue<IDomainEvent> output)
        {
            IDomainEvent domainEvent;
            while (queue.TryDequeue(out domainEvent))
            {
                output.Enqueue(domainEvent);
            }
        }

        private static bool IsSuccessful(HttpResponse response)
        {
            var code = response.StatusCode;
            return code >= 100 && code < 400;
        }
    }
}
