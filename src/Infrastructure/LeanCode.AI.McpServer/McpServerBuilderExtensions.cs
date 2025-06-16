using System.Reflection;
using LeanCode.AI.Contracts;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.RemoteHttp.Client;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace LeanCode.AI.McpServer;

public static class McpServerBuilderExtensions
{
    private delegate Type FunctionTypeConstructor(Type executorType, Type cqrsObjectType, Type resultType);

    private static IMcpServerBuilder WithToolsFromContracts<TExecutor>(
        this IMcpServerBuilder builder,
        CQRSObjectKind objectKind,
        FunctionTypeConstructor functionTypeConstructor,
        TypesCatalog contracts,
        Action<TypeInfo, CQRSObjectKind, McpServerToolCreateOptions>? configureAction
    )
    {
        var executorType = typeof(TExecutor);
        var tools = new List<McpServerTool>();

        foreach (var (contract, kind, result) in contracts.GetCQRSObjects())
        {
            if (kind != objectKind || contract.GetCustomAttribute<McpToolAttribute>() is null)
            {
                continue;
            }

            var type = functionTypeConstructor(executorType, contract, result);
            var function = (CQRSAIFunction)Activator.CreateInstance(type)!;

            McpServerToolCreateOptions options = new()
            {
                Name = function.Name,
                Description = function.Description,
                Title = function.Title,
                Destructive = function.ToolHints.HasFlag(McpToolHints.Destructive),
                Idempotent = function.ToolHints.HasFlag(McpToolHints.Idempotent),
                OpenWorld = function.ToolHints.HasFlag(McpToolHints.OpenWorld),
                ReadOnly = function.ToolHints.HasFlag(McpToolHints.ReadOnly),
                // TODO: UseStructuredContent = true,
            };

            configureAction?.Invoke(contract, kind, options);

            tools.Add(McpServerTool.Create(function, options));
        }

        return builder.WithTools(tools);
    }

    public static IMcpServerBuilder WithToolsFromCommands<TExecutor>(
        this IMcpServerBuilder builder,
        TypesCatalog contracts,
        Action<TypeInfo, CQRSObjectKind, McpServerToolCreateOptions>? configureAction = null
    )
        where TExecutor : HttpCommandsExecutor
    {
        return builder.WithToolsFromContracts<TExecutor>(
            CQRSObjectKind.Command,
            (executor, cqrsObject, result) => typeof(CommandAIFunction<,>).MakeGenericType([executor, cqrsObject]),
            contracts,
            configureAction
        );
    }

    public static IMcpServerBuilder WithToolsFromQueries<TExecutor>(
        this IMcpServerBuilder builder,
        TypesCatalog contracts,
        Action<TypeInfo, CQRSObjectKind, McpServerToolCreateOptions>? configureAction = null
    )
        where TExecutor : HttpQueriesExecutor
    {
        return builder.WithToolsFromContracts<TExecutor>(
            CQRSObjectKind.Query,
            (executor, cqrsObject, result) =>
                typeof(QueryAIFunction<,,>).MakeGenericType([executor, cqrsObject, result]),
            contracts,
            configureAction
        );
    }

    public static IMcpServerBuilder WithToolsFromOperations<TExecutor>(
        this IMcpServerBuilder builder,
        TypesCatalog contracts,
        Action<TypeInfo, CQRSObjectKind, McpServerToolCreateOptions>? configureAction = null
    )
        where TExecutor : HttpOperationsExecutor
    {
        return builder.WithToolsFromContracts<TExecutor>(
            CQRSObjectKind.Operation,
            (executor, cqrsObject, result) =>
                typeof(OperationAIFunction<,,>).MakeGenericType([executor, cqrsObject, result]),
            contracts,
            configureAction
        );
    }
}
