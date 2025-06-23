using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using LeanCode.AI.Contracts;
using LeanCode.Contracts;
using LeanCode.CQRS.RemoteHttp.Client;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using static LeanCode.AI.Contracts.McpToolHints;
using IOException = System.InvalidOperationException;

namespace LeanCode.AI.McpServer;

public abstract class CQRSAIFunction(Type cqrsObjectType) : AIFunction
{
    public static readonly McpToolAttribute EmptyMetadata = new(string.Empty);

    public Type CQRSObjectType { get; } = cqrsObjectType;
    public McpToolAttribute? Metadata { get; } = cqrsObjectType.GetCustomAttribute<McpToolAttribute>();
    public string? Title { get; protected init; }
    public McpToolHints ToolHints { get; protected init; }

    protected abstract McpToolHints DefaultHints { get; }
}

public abstract class CQRSAIFunction<TExecutor, TObject, TResult> : CQRSAIFunction
    where TExecutor : notnull
    where TObject : class
{
    private static readonly JsonSchemaExporterOptions ExporterOptions = new()
    {
        TreatNullObliviousAsNonNullable = true,
    };

    public override string Name { get; }
    public override string Description { get; }
    public override JsonElement JsonSchema { get; }
    public override JsonElement? ReturnJsonSchema { get; }

    protected CQRSAIFunction()
        : base(typeof(TObject))
    {
        var resultType = typeof(TResult);
        var metadata = Metadata ?? EmptyMetadata;

        Name = metadata.Name is { Length: > 0 } ? metadata.Name : CQRSObjectType.Name;
        Description = metadata.Description ?? string.Empty;
        Title = metadata.Title;
        ToolHints = metadata.Hints.HasFlag(Default) ? DefaultHints : metadata.Hints;

        JsonSchema = JsonSerializerOptions
            .GetJsonSchemaAsNode(CQRSObjectType, ExporterOptions)
            .Deserialize<JsonElement>(JsonSerializerOptions);

        if (resultType != typeof(CommandResult))
        {
            ReturnJsonSchema = JsonSerializerOptions
                .GetJsonSchemaAsNode(resultType, ExporterOptions)
                .Deserialize<JsonElement>(JsonSerializerOptions);
        }
    }

    protected override ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var jso = JsonSerializerOptions;
        var services = arguments.Services ?? throw new IOException("Arguments have no services.");
        var nodes = arguments.Select(kvp => new KeyValuePair<string, JsonNode?>(
            kvp.Key,
            kvp.Value is JsonElement je ? je.Deserialize<JsonNode>(jso) : null
        ));
        var jsonObject = new JsonObject(nodes);
        var cqrsObject = jsonObject.Deserialize<TObject>(jso) ?? throw new UnreachableException();

        return InvokeCoreAsync(arguments, services.GetRequiredService<TExecutor>(), cqrsObject, cancellationToken);
    }

    protected abstract ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        TExecutor executor,
        TObject cqrsObject,
        CancellationToken cancellationToken
    );

    protected async ValueTask<object?> HandleErrorsAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();

            if (result is CommandResult commandResult)
            {
                return commandResult.WasSuccessful
                    ? null
                    : commandResult
                        .ValidationErrors.Select(ve => new ErrorContent(ve.ErrorMessage)
                        {
                            ErrorCode = ve.ErrorCode.ToString(null as IFormatProvider),
                            AdditionalProperties = new() { { nameof(ve.PropertyName), ve.PropertyName } },
                        })
                        .ToList();
            }

            return result;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            var message = e.Message is { Length: > 0 }
                ? $"An error occurred invoking {Name} tool: {e.Message}"
                : $"An error occurred invoking {Name} tool.";
            return new ErrorContent(message);
        }
    }
}

public class CommandAIFunction<TExecutor, TCommand> : CQRSAIFunction<TExecutor, TCommand, CommandResult>
    where TExecutor : HttpCommandsExecutor
    where TCommand : class, ICommand
{
    protected override McpToolHints DefaultHints => Destructive | OpenWorld;

    protected override ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        TExecutor executor,
        TCommand cqrsObject,
        CancellationToken cancellationToken
    )
    {
        return HandleErrorsAsync(() => executor.RunAsync(cqrsObject, cancellationToken));
    }
}

public class QueryAIFunction<TExecutor, TQuery, TResult> : CQRSAIFunction<TExecutor, TQuery, TResult>
    where TExecutor : HttpQueriesExecutor
    where TQuery : class, IQuery<TResult>
{
    protected override McpToolHints DefaultHints => Idempotent | OpenWorld | ReadOnly;

    protected override ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        TExecutor executor,
        TQuery cqrsObject,
        CancellationToken cancellationToken
    )
    {
        return HandleErrorsAsync(() => executor.GetAsync(cqrsObject, cancellationToken));
    }
}

public class OperationAIFunction<TExecutor, TOperation, TResult> : CQRSAIFunction<TExecutor, TOperation, TResult>
    where TExecutor : HttpOperationsExecutor
    where TOperation : class, IOperation<TResult>
{
    protected override McpToolHints DefaultHints => Destructive | OpenWorld;

    protected override ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        TExecutor executor,
        TOperation cqrsObject,
        CancellationToken cancellationToken
    )
    {
        return HandleErrorsAsync(() => executor.GetAsync(cqrsObject, cancellationToken));
    }
}
