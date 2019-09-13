using System.Threading.Tasks;
using Autofac;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.DomainModels.EventsExecution;
using Xunit;

namespace LeanCode.IntegrationTestHelpers
{
    public abstract class CQRSTestBase<T, TAppContext> : IClassFixture<T>
        where T : notnull, CQRSTestContextBase
        where TAppContext : notnull
    {
        public T Context { get; }

        protected CQRSTestBase(T context)
        {
            Context = context;
        }

        public Task<CommandResult> RunAsync<TCommand>(TAppContext appContext, TCommand command)
            where TCommand : notnull, ICommand
        {
            return Context.Container.Resolve<ICommandExecutor<TAppContext>>().RunAsync(appContext, command);
        }

        public Task<CommandResult> RunAsync<TCommand>(TCommand command)
            where TCommand : notnull, ICommand
        {
            return Context.Container.Resolve<ICommandExecutor<TAppContext>>().RunAsync(GetDefaultContext(), command);
        }

        public Task<TResult> GetAsync<TResult>(TAppContext appContext, IQuery<TResult> query) =>
            Context.Container.Resolve<IQueryExecutor<TAppContext>>().GetAsync(appContext, query);

        public Task<TResult> GetAsync<TResult>(IQuery<TResult> query) =>
            Context.Container.Resolve<IQueryExecutor<TAppContext>>().GetAsync(GetDefaultContext(), query);

        protected abstract TAppContext GetDefaultContext();
    }

    public abstract class CQRSTestBase<TAppContext> : CQRSTestBase<CQRSTestContextBase<TAppContext>, TAppContext>
        where TAppContext : notnull, IEventsContext
    {
        protected CQRSTestBase(CQRSTestContextBase<TAppContext> context)
            : base(context) { }
    }
}
