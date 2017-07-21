using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Domain.Default
{
    using CommandBuilder = Func<PipelineBuilder<ExecutionContext, ICommand, CommandResult>, PipelineBuilder<ExecutionContext, ICommand, CommandResult>>;
    using QueryBuilder = Func<PipelineBuilder<ExecutionContext, IQuery, object>, PipelineBuilder<ExecutionContext, IQuery, object>>;

    public class DomainComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public DomainComponent(
            TypesCatalog catalog,
            CommandBuilder cmdBuilder,
            QueryBuilder queryBuilder)
        {
            AutofacModule = new DomainModule(catalog, cmdBuilder, queryBuilder);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
