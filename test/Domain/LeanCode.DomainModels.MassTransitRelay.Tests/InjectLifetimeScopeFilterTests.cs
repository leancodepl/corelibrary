using System.Diagnostics.CodeAnalysis;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using GreenPipes;
using MassTransit;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    [SuppressMessage("?", "SA1127", Justification = "Keeping the constraint on the same line works better here.")]
    public class InjectLifetimeScopeFilterTests
    {
        public InjectLifetimeScopeFilterTests()
        { }

        [Fact]
        public async Task Throws_when_cannot_find_autofac_service_provider()
        {
            var filter = new InjectLifetimeScopeFilter<string>();
            var context = new TestConsumeContext<string>();
            var pipe = new TestPipe<string>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => filter.Send(context, pipe));
        }

        [Fact]
        public async Task Lifetime_scope_gets_added_to_payload()
        {
            using var lifetimeScope = new TestLifeTimeScope();
            var filter = new InjectLifetimeScopeFilter<string>();
            var context = new TestConsumeContext<string>
            {
                AutofacServiceProvider = new Autofac.Extensions.DependencyInjection.AutofacServiceProvider(lifetimeScope),
            };
            var pipe = new TestPipe<string>();
            await filter.Send(context, pipe);
            Assert.Equal(lifetimeScope, context.LifetimeScope);
        }

        [Fact]
        public async Task Context_with_payload_gets_sent()
        {
            using var lifetimeScope = new TestLifeTimeScope();
            var filter = new InjectLifetimeScopeFilter<string>();
            var context = new TestConsumeContext<string>
            {
                AutofacServiceProvider = new Autofac.Extensions.DependencyInjection.AutofacServiceProvider(lifetimeScope),
            };
            var pipe = new TestPipe<string>();
            await filter.Send(context, pipe);
            Assert.Equal(lifetimeScope, pipe.LifetimeScope);
        }

#pragma warning disable CS0067
        private class TestLifeTimeScope : ILifetimeScope
        {
            public IDisposer Disposer => throw new NotImplementedException();
            public object Tag => throw new NotImplementedException();
            public IComponentRegistry ComponentRegistry => throw new NotImplementedException();
            public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;
            public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;
            public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;
            public ILifetimeScope BeginLifetimeScope() => throw new NotImplementedException();
            public ILifetimeScope BeginLifetimeScope(object tag) => throw new NotImplementedException();
            public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction) => throw new NotImplementedException();
            public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction) => throw new NotImplementedException();
            public void Dispose() { }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
            public object ResolveComponent(ResolveRequest request) => throw new NotImplementedException();
        }
#pragma warning restore CS0067

        private class TestPipe<T> : IPipe<ConsumeContext<T>> where T : class
        {
            public ILifetimeScope LifetimeScope { get; set; }
            public void Probe(ProbeContext context) => throw new NotImplementedException();
            public Task Send(ConsumeContext<T> context)
            {
                LifetimeScope = (context as TestConsumeContext<T>).LifetimeScope;
                return Task.CompletedTask;
            }
        }

        private class TestConsumeContext<T> : ConsumeContext<T> where T : class
        {
            public bool TryGetPayload<T1>(out T1 payload) where T1 : class
            {
                if (AutofacServiceProvider is null)
                {
                    payload = null;
                    return false;
                }
                else
                {
                    payload = AutofacServiceProvider as T1;
                    return true;
                }
            }

            public T1 GetOrAddPayload<T1>(PayloadFactory<T1> payloadFactory) where T1 : class
            {
                LifetimeScope = payloadFactory.Invoke() as ILifetimeScope;
                return null;
            }

            public Autofac.Extensions.DependencyInjection.AutofacServiceProvider AutofacServiceProvider { get; set; }
            public ILifetimeScope LifetimeScope { get; set; }
            public T Message => throw new NotImplementedException();
            public ReceiveContext ReceiveContext => throw new NotImplementedException();
            public Task ConsumeCompleted => throw new NotImplementedException();
            public IEnumerable<string> SupportedMessageTypes => throw new NotImplementedException();
            public CancellationToken CancellationToken => throw new NotImplementedException();
            public Guid? MessageId => throw new NotImplementedException();
            public Guid? RequestId => throw new NotImplementedException();
            public Guid? CorrelationId => throw new NotImplementedException();
            public Guid? ConversationId => throw new NotImplementedException();
            public Guid? InitiatorId => throw new NotImplementedException();
            public DateTime? ExpirationTime => throw new NotImplementedException();
            public Uri SourceAddress => throw new NotImplementedException();
            public Uri DestinationAddress => throw new NotImplementedException();
            public Uri ResponseAddress => throw new NotImplementedException();
            public Uri FaultAddress => throw new NotImplementedException();
            public DateTime? SentTime => throw new NotImplementedException();
            public Headers Headers => throw new NotImplementedException();
            public HostInfo Host => throw new NotImplementedException();
            public void AddConsumeTask(Task task) => throw new NotImplementedException();
            public T1 AddOrUpdatePayload<T1>(PayloadFactory<T1> addFactory, UpdatePayloadFactory<T1> updateFactory) where T1 : class => throw new NotImplementedException();
            public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => throw new NotImplementedException();
            public ConnectHandle ConnectSendObserver(ISendObserver observer) => throw new NotImplementedException();
            public Task<ISendEndpoint> GetSendEndpoint(Uri address) => throw new NotImplementedException();
            public bool HasMessageType(Type messageType) => throw new NotImplementedException();
            public bool HasPayloadType(Type payloadType) => throw new NotImplementedException();
            public Task NotifyConsumed(TimeSpan duration, string consumerType) => throw new NotImplementedException();
            public Task NotifyConsumed<T1>(ConsumeContext<T1> context, TimeSpan duration, string consumerType) where T1 : class => throw new NotImplementedException();
            public Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception) => throw new NotImplementedException();
            public Task NotifyFaulted<T1>(ConsumeContext<T1> context, TimeSpan duration, string consumerType, Exception exception) where T1 : class => throw new NotImplementedException();
            public Task Publish<T1>(T1 message, CancellationToken cancellationToken = default) where T1 : class => throw new NotImplementedException();
            public Task Publish<T1>(T1 message, IPipe<PublishContext<T1>> publishPipe, CancellationToken cancellationToken = default) where T1 : class => throw new NotImplementedException();
            public Task Publish<T1>(T1 message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T1 : class => throw new NotImplementedException();
            public Task Publish(object message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task Publish<T1>(object values, CancellationToken cancellationToken = default) where T1 : class => throw new NotImplementedException();
            public Task Publish<T1>(object values, IPipe<PublishContext<T1>> publishPipe, CancellationToken cancellationToken = default) where T1 : class => throw new NotImplementedException();
            public Task Publish<T1>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T1 : class => throw new NotImplementedException();
            public void Respond<T1>(T1 message) where T1 : class => throw new NotImplementedException();
            public Task RespondAsync<T1>(T1 message) where T1 : class => throw new NotImplementedException();
            public Task RespondAsync<T1>(T1 message, IPipe<SendContext<T1>> sendPipe) where T1 : class => throw new NotImplementedException();
            public Task RespondAsync<T1>(T1 message, IPipe<SendContext> sendPipe) where T1 : class => throw new NotImplementedException();
            public Task RespondAsync(object message) => throw new NotImplementedException();
            public Task RespondAsync(object message, Type messageType) => throw new NotImplementedException();
            public Task RespondAsync(object message, IPipe<SendContext> sendPipe) => throw new NotImplementedException();
            public Task RespondAsync(object message, Type messageType, IPipe<SendContext> sendPipe) => throw new NotImplementedException();
            public Task RespondAsync<T1>(object values) where T1 : class => throw new NotImplementedException();
            public Task RespondAsync<T1>(object values, IPipe<SendContext<T1>> sendPipe) where T1 : class => throw new NotImplementedException();
            public Task RespondAsync<T1>(object values, IPipe<SendContext> sendPipe) where T1 : class => throw new NotImplementedException();
            public bool TryGetMessage<T1>(out ConsumeContext<T1> consumeContext) where T1 : class => throw new NotImplementedException();
        }
    }
}
