using Autofac;
using LeanCode.Components;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Tests
{
    public class BaseCQRSTests
    {
        protected IContainer Container { get; private set; }

        protected ICommandExecutor<AppContext> CommandExecutor { get; private set; }
        protected IQueryExecutor<AppContext> QueryExecutor { get; private set; }
        protected IOperationExecutor<AppContext> OperationExecutor { get; private set; }

        protected ICommandHandlerResolver<AppContext> CHResolver { get; private set; }
        protected IQueryHandlerResolver<AppContext> QHResolver { get; private set; }
        protected IOperationHandlerResolver<AppContext> OHResolver { get; private set; }
        protected IAuthorizerResolver<AppContext> AuthResolver { get; private set; }
        protected ICommandValidatorResolver<AppContext> ValidatorResolver { get; private set; }

        public void Prepare(
            CommandBuilder<AppContext> cmdBuilder = null,
            QueryBuilder<AppContext> queryBuilder = null,
            OperationBuilder<AppContext> operationBuilder = null)
        {
            cmdBuilder ??= c => c;
            queryBuilder ??= q => q;
            operationBuilder ??= o => o;

            var catalog = new TypesCatalog(typeof(BaseCQRSTests));
            var builder = new ContainerBuilder();
            builder.RegisterModule(new CQRSModule().WithCustomPipelines(catalog, cmdBuilder, queryBuilder, operationBuilder));
            builder.RegisterType<SampleAuthorizer>().AsImplementedInterfaces();
            builder.RegisterType<SampleValidator>().AsImplementedInterfaces();
            builder.RegisterType<SingleInstanceCommandHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<SingleInstanceQueryHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<SingleInstanceOperationHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<SamplePipelineElement<ICommand, CommandResult>>().AsSelf().SingleInstance();
            builder.RegisterType<SamplePipelineElement<IQuery, object>>().AsSelf().SingleInstance();
            builder.RegisterType<SamplePipelineElement<IOperation, object>>().AsSelf().SingleInstance();
            Container = builder.Build();

            CommandExecutor = Container.Resolve<ICommandExecutor<AppContext>>();
            QueryExecutor = Container.Resolve<IQueryExecutor<AppContext>>();
            OperationExecutor = Container.Resolve<IOperationExecutor<AppContext>>();

            CHResolver = Container.Resolve<ICommandHandlerResolver<AppContext>>();
            QHResolver = Container.Resolve<IQueryHandlerResolver<AppContext>>();
            OHResolver = Container.Resolve<IOperationHandlerResolver<AppContext>>();
            AuthResolver = Container.Resolve<IAuthorizerResolver<AppContext>>();
            ValidatorResolver = Container.Resolve<ICommandValidatorResolver<AppContext>>();
        }

        protected (ICommandHandlerWrapper, SampleCommandHandler) FindSampleCommandHandler()
        {
            var handler = CHResolver.FindCommandHandler(typeof(SampleCommand));
            var underlying = SampleCommandHandler.LastInstance.Value;
            return (handler, underlying);
        }

        protected (IQueryHandlerWrapper, SampleQueryHandler) FindSampleQueryHandler()
        {
            var handler = QHResolver.FindQueryHandler(typeof(SampleQuery));
            var underlying = SampleQueryHandler.LastInstance.Value;
            return (handler, underlying);
        }

        protected (IOperationHandlerWrapper, SampleOperationHandler) FindSampleOperationHandler()
        {
            var handler = OHResolver.FindOperationHandler(typeof(SampleOperation));
            var underlying = SampleOperationHandler.LastInstance.Value;
            return (handler, underlying);
        }

        protected (ICustomAuthorizerWrapper, SampleAuthorizer) FindSampleAuthorizer<TData>()
            where TData : class
        {
            ICustomAuthorizerWrapper wrapper;
            if (typeof(TData).IsAssignableTo<ICommand>())
            {
                wrapper = AuthResolver.FindAuthorizer(typeof(IHasSampleAuthorizer), typeof(ICommand));
            }
            else
            {
                wrapper = AuthResolver.FindAuthorizer(typeof(IHasSampleAuthorizer), typeof(IQuery));
            }

            var underlying = SampleAuthorizer.LastInstance.Value;
            return (wrapper, underlying);
        }

        protected (ICommandValidatorWrapper, SampleValidator) FindSampleValidator()
        {
            var handler = ValidatorResolver.FindCommandValidator(typeof(SampleCommand));
            var underlying = SampleValidator.LastInstance.Value;
            return (handler, underlying);
        }
    }
}
