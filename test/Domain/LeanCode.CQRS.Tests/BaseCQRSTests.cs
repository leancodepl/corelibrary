using Autofac;
using LeanCode.Components;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;

namespace LeanCode.CQRS.Tests
{
    public class BaseCQRSTests
    {
        protected IContainer Container { get; private set; }

        protected ICommandExecutor<AppContext> CommandExecutor { get; private set; }
        protected IQueryExecutor<AppContext> QueryExecutor { get; private set; }

        protected ICommandHandlerResolver<AppContext> CHResolver { get; private set; }
        protected IQueryHandlerResolver<AppContext> QHResolver { get; private set; }
        protected IAuthorizerResolver<AppContext> AuthResolver { get; private set; }
        protected IDomainEventHandlerResolver EventResolver { get; private set; }
        protected ICommandValidatorResolver<AppContext> ValidatorResolver { get; private set; }

        public void Prepare(
            CommandBuilder<AppContext> cmdBuilder = null,
            QueryBuilder<AppContext> queryBuilder = null)
        {
            cmdBuilder = cmdBuilder ?? (c => c);
            queryBuilder = queryBuilder ?? (q => q);

            var catalog = new TypesCatalog(typeof(BaseCQRSTests));
            var builder = new ContainerBuilder();
            builder.RegisterModule(new CQRSModule<AppContext>(catalog, cmdBuilder, queryBuilder));
            builder.RegisterType<SampleAuthorizer>().AsImplementedInterfaces();
            builder.RegisterType<SampleValidator>().AsImplementedInterfaces();
            builder.RegisterType<SingleInstanceCommandHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<SingleInstanceQueryHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<SamplePipelineElement<CommandExecutionPayload, CommandResult>>().AsSelf().SingleInstance();
            builder.RegisterType<SamplePipelineElement<QueryExecutionPayload, object>>().AsSelf().SingleInstance();
            Container = builder.Build();

            CommandExecutor = Container.Resolve<ICommandExecutor<AppContext>>();
            QueryExecutor = Container.Resolve<IQueryExecutor<AppContext>>();

            CHResolver = Container.Resolve<ICommandHandlerResolver<AppContext>>();
            QHResolver = Container.Resolve<IQueryHandlerResolver<AppContext>>();
            AuthResolver = Container.Resolve<IAuthorizerResolver<AppContext>>();
            EventResolver = Container.Resolve<IDomainEventHandlerResolver>();
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

        protected (ICustomAuthorizerWrapper, SampleAuthorizer) FindSampleAuthorizer<TData>()
            where TData : class
        {
            ICustomAuthorizerWrapper wrapper;
            if (typeof(TData).IsAssignableTo<ICommand>())
            {
                wrapper = AuthResolver.FindAuthorizer(typeof(HasSampleAuthorizer), typeof(CommandExecutionPayload));
            }
            else
            {
                wrapper = AuthResolver.FindAuthorizer(typeof(HasSampleAuthorizer), typeof(QueryExecutionPayload));
            }
            var underlying = SampleAuthorizer.LastInstance.Value;
            return (wrapper, underlying);
        }

        protected (IDomainEventHandlerWrapper, SampleEventHandler) FindSampleEventHandler()
        {
            var handlers = EventResolver.FindEventHandlers(typeof(SampleEvent));
            if (handlers.Count > 0)
            {
                return (handlers[0], SampleEventHandler.LastInstance.Value);
            }
            else
            {
                return (null, null);
            }
        }

        protected (ICommandValidatorWrapper, SampleValidator) FindSampleValidator()
        {
            var handler = ValidatorResolver.FindCommandValidator(typeof(SampleCommand));
            var underlying = SampleValidator.LastInstance.Value;
            return (handler, underlying);
        }
    }
}
