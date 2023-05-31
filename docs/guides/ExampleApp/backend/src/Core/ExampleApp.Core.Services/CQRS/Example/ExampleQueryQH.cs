using ExampleApp.Core.Contracts.Example;
using LeanCode.CQRS.Execution;

namespace ExampleApp.Core.Services.CQRS.Example;

public class ExampleQueryQH : IQueryHandler<CoreContext, ExampleQuery, QueryResultDTO>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ExampleQueryQH>();

    public Task<QueryResultDTO> ExecuteAsync(CoreContext context, ExampleQuery query)
    {
        logger.Information("ExampleQueryQH called");
        return Task.FromResult(new QueryResultDTO { Greeting = $"Hello {query.Name}!", });
    }
}
