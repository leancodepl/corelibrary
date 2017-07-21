using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Default
{
    using CommandBuilder = Func<PipelineBuilder<ExecutionContext, ICommand, CommandResult>, PipelineBuilder<ExecutionContext, ICommand, CommandResult>>;
    using QueryBuilder = Func<PipelineBuilder<ExecutionContext, IQuery, object>, PipelineBuilder<ExecutionContext, IQuery, object>>;

    public class CQRSComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public CQRSComponent(
            TypesCatalog catalog,
            CommandBuilder cmdBuilder,
            QueryBuilder queryBuilder)
        {
            AutofacModule = new CQRSModule(catalog, cmdBuilder, queryBuilder);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
