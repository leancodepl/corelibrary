using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace LeanCode.DomainModels.EventsExecutor
{
    using ExecutionStorage = Queue<ConcurrentQueue<IDomainEvent>>;

    sealed class EventsExecutor : IEventsExecutor
    {
        private readonly Serilog.ILogger logger
            = Serilog.Log.ForContext<EventsExecutor>();

        private readonly RetryPolicies policies;
        private readonly AsyncEventsStorage storage;
        private readonly IDomainEventHandlerResolver resolver;

        public EventsExecutor(RetryPolicies policies,
            AsyncEventsStorage storage,
            IDomainEventHandlerResolver resolver)
        {
            this.policies = policies;
            this.storage = storage;
            this.resolver = resolver;
        }

        public async Task<TResult> HandleEventsOf<TResult>(Func<Task<ExecutionResult<TResult>>> action)
        {
            logger.Debug("Starting executing action {Action}", action);

            storage.Prepare();
            ExecutionResult<TResult> result;
            try
            {
                result = await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                storage.CaptureQueue();
                logger.Error(ex, "Exception occured during execution od {Action}, skipping event execution",
                    action);
                throw;
            }

            var queue = storage.CaptureQueue();

            if (result.SkipExecution)
            {
                logger.Warning("Action {Action} completed successfully, but requested to skip event execution", action);
                return result.Result;
            }

            if (!queue.IsEmpty)
            {
                ExecutionStorage execStorage = new ExecutionStorage();
                execStorage.Enqueue(queue);
                await ExecuteEvents(execStorage).ConfigureAwait(false);
            }
            return result.Result;
        }

        private async Task ExecuteEvents(ExecutionStorage execStorage)
        {
            while (execStorage.Count > 0)
            {
                var queue = execStorage.Dequeue();
                while (queue.TryDequeue(out var domainEvent))
                {
                    var handlers = resolver.FindEventHandlers(domainEvent.GetType());
                    logger.Debug("Executing event {Event} with {N} handlers",
                        domainEvent, handlers.Count);
                    foreach (var h in handlers)
                    {
                        logger.Verbose("Executing handler {Handler} with event {Event}",
                            h.UnderlyingHandler, domainEvent);
                        await ExecuteEvent(h, domainEvent, execStorage).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task ExecuteEvent(
            IDomainEventHandlerWrapper handler,
            IDomainEvent domainEvent,
            ExecutionStorage execStorage)
        {
            var result = await policies.EventHandlerPolicy
                .ExecuteAndCaptureAsync(async () =>
                {
                    storage.Prepare();
                    await handler.HandleAsync(domainEvent).ConfigureAwait(false);
                    return storage.CaptureQueue();
                })
                .ConfigureAwait(false);

            if (result.Outcome == OutcomeType.Failure)
            {
                logger.Fatal(result.FinalException, "Cannot execute handler {Handler} with event {Event}",
                    handler.UnderlyingHandler, domainEvent);
            }
            else
            {
                if (!result.Result.IsEmpty)
                {
                    execStorage.Enqueue(result.Result);
                }
                logger.Information("Handler {Handler} with event {Event} executed successfully",
                    handler.UnderlyingHandler, domainEvent);
            }
        }
    }
}
