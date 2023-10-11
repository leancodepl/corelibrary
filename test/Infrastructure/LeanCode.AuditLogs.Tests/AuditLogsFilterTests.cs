using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using LeanCode.CQRS.MassTransitRelay;
using LeanCode.OpenTelemetry;
using LeanCode.TimeProvider;
using LeanCode.TimeProvider.TestHelpers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Xunit;

namespace LeanCode.AuditLogs.Tests;

public sealed class AuditLogsFilterTests
{
    private const string SomeId = "some_id";
    private const string ActorId = "actor_id";

    private static readonly JsonSerializerOptions Options =
        new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
        };

    private readonly IServiceProvider serviceProvider;
    private readonly ITestHarness harness;
    private static readonly TestEntity TestEntity = new() { Id = SomeId };

    public AuditLogsFilterTests()
    {
        TestTimeProvider.ActivateFake(new DateTimeOffset(2023, 10, 6, 11, 0, 3, 0, TimeSpan.Zero));
        var collection = new ServiceCollection();
        collection
            .AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("AuditLogsFilterTests", serviceInstanceId: Environment.MachineName))
            .WithTracing(builder =>
            {
                builder
                    .AddProcessor<IdentityTraceAttributesFromBaggageProcessor>()
                    .AddSource("MassTransit")
                    .AddLeanCodeTelemetry();
            });
        collection.AddDbContext<TestDbContext>();
        collection.AddTransient<IAuditLogStorage, StubAuditLogStorage>();
        collection.AddMassTransitTestHarness(ConfigureMassTransit);

        serviceProvider = collection.BuildServiceProvider();
        harness = serviceProvider.GetRequiredService<ITestHarness>();
    }

    private static void ConfigureMassTransit(IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumersWithDefaultConfiguration(
            new[] { typeof(Consumer).Assembly, typeof(AuditLogsConsumer).Assembly },
            typeof(DefaultConsumerDefinition<>)
        );

        cfg.UsingInMemory(
            (ctx, busCfg) =>
            {
                busCfg.ConfigureEndpoints(ctx, new DefaultEndpointNameFormatter("InMemory"));
                busCfg.ConnectBusObservers(ctx);
            }
        );
    }

    [Fact]
    public async Task Publishes_entity_changes_to_bus()
    {
        await harness.Start();
        var consumerHarness = harness.GetConsumerHarness<Consumer>();

        await harness.Bus.Publish(new TestMsg());
        (await consumerHarness.Consumed.Any<TestMsg>()).Should().BeTrue();

        harness.Published
            .Select<AuditLogMessage>()
            .Should()
            .ContainSingle()
            .Which.Context.Message.Should()
            .BeEquivalentTo(
                new
                {
                    EntityChanged = new
                    {
                        Ids = new string[] { SomeId },
                        Type = typeof(TestEntity).FullName,
                        EntityState = "Added",
                        Changes = JsonSerializer.SerializeToDocument(TestEntity, Options),
                    },
                    ActionName = typeof(Consumer).FullName,
                    ActorId = null as string,
                    DateOccurred = Time.NowWithOffset,
                },
                opt => opt.ComparingByMembers<JsonElement>()
            )
            .And.Subject.Should()
            .Match((s) => s.As<AuditLogMessage>().SpanId != null && s.As<AuditLogMessage>().TraceId != null);
    }

    [Fact]
    public async Task Publishes_actor_id_when_provided_to_bus()
    {
        await harness.Start();
        var consumerHarness = harness.GetConsumerHarness<Consumer>();

        await harness.Bus.Publish(new TestMsg { ActorId = ActorId, });
        (await consumerHarness.Consumed.Any<TestMsg>()).Should().BeTrue();

        harness.Published
            .Select<AuditLogMessage>()
            .Should()
            .ContainSingle()
            .Which.Context.Message.Should()
            .BeEquivalentTo(
                new
                {
                    ActionName = typeof(Consumer).FullName,
                    ActorId,
                    DateOccurred = Time.NowWithOffset,
                },
                opt => opt.ComparingByMembers<JsonElement>()
            )
            .And.Subject.Should()
            .Match((s) => s.As<AuditLogMessage>().SpanId != null && s.As<AuditLogMessage>().TraceId != null);
    }

    public sealed class TestMsg
    {
        public string ActorId { get; set; } = null!;
    }

    public class Consumer : IConsumer<TestMsg>
    {
        private readonly TestDbContext dbContext;

        public Consumer(TestDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task Consume(ConsumeContext<TestMsg> context)
        {
            if (context.Message.ActorId is not null)
            {
                Activity.Current!.AddBaggage(IdentityTraceBaggageHelpers.CurrentUserIdKey, ActorId);
            }
            dbContext.Add(TestEntity);
            return Task.CompletedTask;
        }
    }

    public class DefaultConsumerDefinition<TConsumer> : ConsumerDefinition<TConsumer>
        where TConsumer : class, IConsumer
    {
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<TConsumer> consumerConfigurator,
            IRegistrationContext context
        )
        {
            endpointConfigurator.UseMessageRetry(
                r => r.Immediate(1).Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
            );
            endpointConfigurator.UseAuditLogs<TestDbContext>(context);
        }
    }
}
