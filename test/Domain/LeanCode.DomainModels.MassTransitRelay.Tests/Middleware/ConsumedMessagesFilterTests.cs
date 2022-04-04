using System.Data.Common;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.Time;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Middleware
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1849", Justification = "Allowed in tests.")]
    public sealed class ConsumedMessagesFilterTests : IAsyncLifetime, IDisposable
    {
        private static readonly Guid MessageId = Guid.NewGuid();

        private readonly IContainer container;
        private readonly InMemoryTestHarness harness;
        private readonly DbConnection dbConnection;

        public ConsumedMessagesFilterTests()
        {
            dbConnection = new SqliteConnection("Filename=:memory:");

            var collection = new ServiceCollection();
            collection.AddMassTransitInMemoryTestHarness();

            var factory = new AutofacServiceProviderFactory();
            var builder = factory.CreateBuilder(collection);
            builder.Register(s => TestDbContext.Create(dbConnection)).As<IConsumedMessagesContext>();

            container = builder.Build();
            harness = container.Resolve<InMemoryTestHarness>();
            harness.TestTimeout = TimeSpan.FromSeconds(1);

            harness.OnConfigureInMemoryBus += cfg =>
            {
                // we have to serialize access to database
                cfg.UseConcurrencyLimit(1);
                cfg.UseConsumedMessagesFiltering(container.Resolve<IServiceProvider>());
            };
        }

        [Fact]
        public async Task Consumes_unobserved_message_and_persists_information()
        {
            var consumer = harness.Consumer<FirstConsumer>();
            await harness.Start();

            await harness.Bus.Publish(new TestMsg(), ctx => ctx.MessageId = MessageId);

            Assert.Single(consumer.Consumed.Select<TestMsg>());
            var allConsumed = await FetchConsumedMessages();
            var consumed = Assert.Single(allConsumed);
            AssertConsumedMessage(consumed, typeof(FirstConsumer), typeof(TestMsg), MessageId);
        }

        [Fact]
        public async Task Two_different_messages_are_consumed()
        {
            var consumer = harness.Consumer<FirstConsumer>();
            await harness.Start();

            var msg1 = Guid.NewGuid();
            var msg2 = Guid.NewGuid();

            await harness.Bus.Publish(new TestMsg(), ctx => ctx.MessageId = msg1);
            await harness.Bus.Publish(new TestMsg(), ctx => ctx.MessageId = msg2);

            Assert.Single(consumer.Consumed.Select<TestMsg>(msg => msg.Context.MessageId == msg1));
            Assert.Single(consumer.Consumed.Select<TestMsg>(msg => msg.Context.MessageId == msg2));

            var consumedMessages = await FetchConsumedMessages();
            Assert.Contains(consumedMessages, msg => msg.MessageId == msg1 && msg.ConsumerType == typeof(FirstConsumer).FullName);
            Assert.Contains(consumedMessages, msg => msg.MessageId == msg2 && msg.ConsumerType == typeof(FirstConsumer).FullName);
        }

        [Fact]
        public async Task Does_not_consume_already_consumed_message()
        {
            using var dbContext = TestDbContext.Create(dbConnection);
            dbContext.Add(new ConsumedMessage(MessageId, TimeProvider.Now, typeof(ReportingConsumer).FullName, typeof(TestMsg).FullName));
            await dbContext.SaveChangesAsync();

            var consumer = new ReportingConsumer();
            var consumerHarness = harness.Consumer(() => consumer);
            await harness.Start();

            await harness.Bus.Publish(new TestMsg(), ctx => ctx.MessageId = MessageId);

            // The harness does not know we short circuit the pipeline
            // so we have to do it manually
            Assert.Single(consumerHarness.Consumed.Select<TestMsg>());
            Assert.False(consumer.Consumed);
        }

        [Fact]
        public async Task Passes_different_consumers_of_the_same_message_and_persists_information()
        {
            var first = harness.Consumer<FirstConsumer>();
            var second = harness.Consumer<SecondConsumer>();

            await harness.Start();

            await harness.Bus.Publish(new TestMsg(), cfg => cfg.MessageId = MessageId);

            Assert.Single(first.Consumed.Select<TestMsg>());
            Assert.Single(second.Consumed.Select<TestMsg>());

            var consumedMessages = await FetchConsumedMessages();
            Assert.Collection(
                consumedMessages,
                m => AssertConsumedMessage(m, typeof(FirstConsumer), typeof(TestMsg), MessageId),
                m => AssertConsumedMessage(m, typeof(SecondConsumer), typeof(TestMsg), MessageId));
        }

        private async Task<List<ConsumedMessage>> FetchConsumedMessages()
        {
            using var dbContext = TestDbContext.Create(dbConnection);
            return await dbContext.ConsumedMessages
                .OrderBy(msg => msg.ConsumerType)
                .ToListAsync();
        }

        private static void AssertConsumedMessage(ConsumedMessage msg, Type consumerType, Type messageType, Guid messageId)
        {
            Assert.Equal(consumerType.FullName, msg.ConsumerType);
            Assert.Equal(messageType.FullName, msg.MessageType);
            Assert.Equal(messageId, msg.MessageId);
        }

        public async Task InitializeAsync()
        {
            // harness started at test level because
            // we need to have different consumers in each test
            // await harness.Start();

            await dbConnection.OpenAsync();
            using var dbContext = TestDbContext.Create(dbConnection);
            await dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await harness.Stop();
            await dbConnection.CloseAsync();
            await container.DisposeAsync();
        }

        public void Dispose()
        {
            harness.Dispose();
            dbConnection.Dispose();
            container.Dispose();
        }

        private class TestMsg { }

        private class FirstConsumer : IConsumer<TestMsg>
        {
            public Task Consume(ConsumeContext<TestMsg> context)
            {
                return Task.CompletedTask;
            }
        }

        private class SecondConsumer : IConsumer<TestMsg>
        {
            public Task Consume(ConsumeContext<TestMsg> context)
            {
                return Task.CompletedTask;
            }
        }

        private class ReportingConsumer : IConsumer<TestMsg>
        {
            public bool Consumed { get; private set; }

            public Task Consume(ConsumeContext<TestMsg> context)
            {
                Consumed = true;
                return Task.CompletedTask;
            }
        }
    }
}
