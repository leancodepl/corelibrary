using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.IntegrationTestHelpers.Tests.CQRS
{
    public class AppContext : IEventsContext, IPipelineContext
    {
        public List<IDomainEvent> SavedEvents { get; set; }
        public List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; }
        public List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; }
        public IPipelineScope Scope { get; set; }
    }

    public class EmptyContext { }

    public class Result
    {
        public string Value { get; set; }
    }

    public class TestQuery : IQuery<EmptyContext, Result>
    { }

    public class TestQueryHandler : IQueryHandler<EmptyContext, TestQuery, Result>
    {
        public Task<Result> ExecuteAsync(EmptyContext context, TestQuery query)
        {
            return Task.FromResult(new Result { Value = "abc" });
        }
    }

    public class TestCommand : ICommand<EmptyContext>
    {
        public string Name { get; set; }
    }

    public class TestCommandHandler : ICommandHandler<EmptyContext, TestCommand>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<TestCommandHandler>();

        private readonly TestDbContext dbContext;

        public TestCommandHandler(TestDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task ExecuteAsync(EmptyContext context, TestCommand command)
        {
            dbContext.Entities.Add(new Entity { Id = Guid.NewGuid(), Value = command.Name });
            await dbContext.SaveChangesAsync();

            logger.Information("Entity added");
        }
    }
}
