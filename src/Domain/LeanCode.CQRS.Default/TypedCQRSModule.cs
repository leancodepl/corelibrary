using Autofac;
using LeanCode.Components;
using LeanCode.CQRS.Default.Autofac;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default;

internal class TypedCQRSModule<TAppContext> : Module
    where TAppContext : IPipelineContext
{
    private readonly TypesCatalog catalog;
    private readonly CommandBuilder<TAppContext> cmdBuilder;
    private readonly QueryBuilder<TAppContext> queryBuilder;
    private readonly OperationBuilder<TAppContext> operationBuilder;

    public TypedCQRSModule(
        TypesCatalog catalog,
        CommandBuilder<TAppContext> cmdBuilder,
        QueryBuilder<TAppContext> queryBuilder,
        OperationBuilder<TAppContext> operationBuilder)
    {
        this.catalog = catalog;
        this.cmdBuilder = cmdBuilder;
        this.queryBuilder = queryBuilder;
        this.operationBuilder = operationBuilder;
    }

    protected override void Load(ContainerBuilder builder)
    {
        var assemblies = catalog.Assemblies.ToArray();
        builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(ICommandHandler<,>));
        builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(IQueryHandler<,,>));
        builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(IOperationHandler<,,>));

        builder.RegisterType<AutofacCommandHandlerResolver<TAppContext>>().As<ICommandHandlerResolver<TAppContext>>();
        builder.RegisterType<AutofacQueryHandlerResolver<TAppContext>>().As<IQueryHandlerResolver<TAppContext>>();
        builder.RegisterType<AutofacOperationHandlerResolver<TAppContext>>().As<IOperationHandlerResolver<TAppContext>>();
        builder.RegisterType<AutofacAuthorizerResolver<TAppContext>>().As<IAuthorizerResolver<TAppContext>>();
        builder.RegisterType<AutofacValidatorResolver<TAppContext>>().As<ICommandValidatorResolver<TAppContext>>();

        builder.Register(c =>
            new CommandExecutor<TAppContext>(
                c.Resolve<IPipelineFactory>(),
                cmdBuilder))
            .As<ICommandExecutor<TAppContext>>()
            .SingleInstance();

        builder.Register(c =>
            new QueryExecutor<TAppContext>(
                c.Resolve<IPipelineFactory>(),
                queryBuilder))
            .As<IQueryExecutor<TAppContext>>()
            .SingleInstance();

        builder.Register(c =>
            new OperationExecutor<TAppContext>(
                c.Resolve<IPipelineFactory>(),
                operationBuilder))
            .As<IOperationExecutor<TAppContext>>()
            .SingleInstance();
    }
}
