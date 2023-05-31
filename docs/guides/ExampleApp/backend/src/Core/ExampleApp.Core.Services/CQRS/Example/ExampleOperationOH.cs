using ExampleApp.Core.Contracts.Example;
using LeanCode.CQRS.Execution;

namespace ExampleApp.Core.Services.CQRS.Example;

public class ExampleOperationOH : IOperationHandler<CoreContext, ExampleOperation, OperationResultDTO>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ExampleOperationOH>();

    public Task<OperationResultDTO> ExecuteAsync(CoreContext context, ExampleOperation operation)
    {
        logger.Information("ExampleOperationOH called");
        return Task.FromResult(new OperationResultDTO { Greeting = $"Hi {operation.Name}!", });
    }
}
