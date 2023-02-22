using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default;

public class CQRSModule : AppModule
{
    private readonly List<IModule> modules = new(2);

    public CQRSModule()
    {
        modules.Add(new SharedCQRSModule());
    }

    protected override void Load(ContainerBuilder builder)
    {
        foreach (var m in modules)
        {
            builder.RegisterModule(m);
        }
    }

    [Obsolete("Use `WithCustomPipelines(TypesCatalog, CommandBuilder<TAppContext>, QueryBuilder<TAppContext>, OperationBuilder<TAppContext>)` overload which also configures operations")]
    public CQRSModule WithCustomPipelines<TAppContext>(
        TypesCatalog handlersCatalog,
        CommandBuilder<TAppContext> commandBuilder,
        QueryBuilder<TAppContext> queryBuilder)
        where TAppContext : IPipelineContext
    {
        return WithCustomPipelines(handlersCatalog, commandBuilder, queryBuilder, o => o);
    }

    public CQRSModule WithCustomPipelines<TAppContext>(
        TypesCatalog handlersCatalog,
        CommandBuilder<TAppContext> commandBuilder,
        QueryBuilder<TAppContext> queryBuilder,
        OperationBuilder<TAppContext> operationBuilder)
        where TAppContext : IPipelineContext
    {
        modules.Add(new TypedCQRSModule<TAppContext>(handlersCatalog, commandBuilder, queryBuilder, operationBuilder));

        return this;
    }
}
