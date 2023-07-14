using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.MassTransitRelay.Middleware;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.DomainModels.Model;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTests.App;

[AuthorizeWhenHasAnyOf("user")]
public class AddEntity : ICommand
{
    public string Value { get; set; } = null!;

    [SuppressMessage("?", "CA1034", Justification = "Convention for error codes")]
    public static class ErrorCodes
    {
        public const int ValueRequired = 1;
    }
}

[AuthorizeWhenHasAnyOf("user")]
public class ListEntities : IQuery<List<EntityDTO>> { }

public class EntityDTO
{
    public Guid Id { get; set; }
    public string Value { get; set; } = default!;
}

public class ListEntitiesQH : IQueryHandler<ListEntities, List<EntityDTO>>
{
    private readonly TestDbContext dbContext;

    public ListEntitiesQH(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<List<EntityDTO>> ExecuteAsync(HttpContext context, ListEntities query)
    {
        return dbContext.Entities
            .Select(e => new EntityDTO { Id = e.Id, Value = e.Value })
            .ToListAsync(context.RequestAborted);
    }
}

public class AddEntityCV : AbstractValidator<AddEntity>
{
    public AddEntityCV()
    {
        RuleFor(cmd => cmd.Value).NotEmpty().WithCode(AddEntity.ErrorCodes.ValueRequired);
    }
}

public class AddEntityCH : ICommandHandler<AddEntity>
{
    private readonly TestDbContext dbContext;

    public AddEntityCH(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task ExecuteAsync(HttpContext context, AddEntity command)
    {
        var entity = new Entity { Id = Guid.NewGuid(), Value = command.Value, };
        DomainEvents.Raise(new EntityAdded(entity));

        dbContext.Entities.Add(entity);
        // No dbContext.SaveChanges - infrastructure will be handling this

        return Task.CompletedTask;
    }
}

public class EntityAddedConsumer : IConsumer<EntityAdded>
{
    private readonly TestDbContext dbContext;

    public EntityAddedConsumer(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task Consume(ConsumeContext<EntityAdded> context)
    {
        var entity = new Entity { Id = Guid.NewGuid(), Value = $"{context.Message.Value}-consumer" };

        dbContext.Entities.Add(entity);
        // No dbContext.SaveChanges - infrastructure will be handling this

        return Task.CompletedTask;
    }
}

public class EntityAddedConsumerDefinition : ConsumerDefinition<EntityAddedConsumer>
{
    private readonly IServiceProvider serviceProvider;

    public EntityAddedConsumerDefinition(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<EntityAddedConsumer> consumerConfigurator
    )
    {
        endpointConfigurator.UseEntityFrameworkOutbox<TestDbContext>(serviceProvider);
        endpointConfigurator.UseDomainEventsPublishing(serviceProvider);
    }
}
