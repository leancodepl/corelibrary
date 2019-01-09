using System.Threading.Tasks;
using Autofac;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.DomainModels.EventsExecution;
using Xunit;

namespace LeanCode.IntegrationTestHelpers
{
    public abstract class CQRSTestBase<T, TAppContext> : IClassFixture<T>
        where T : CQRSTestContextBase
    {
        public T Context { get; }

        protected CQRSTestBase(T context)
        {
            Context = context;
        }

        public Task<CommandResult> RunAsync<TCommand>(
            TAppContext appContext,
            TCommand command)
            where TCommand : ICommand
        {
            var executor = Context.Container.Resolve<ICommandExecutor<TAppContext>>();
            return executor.RunAsync(appContext, command);
        }

        public Task<CommandResult> RunAsync<TCommand>(
            TCommand command)
            where TCommand : ICommand
        {
            var executor = Context.Container.Resolve<ICommandExecutor<TAppContext>>();
            return executor.RunAsync(GetDefaultContext(), command);
        }

        public Task<TResult> GetAsync<TResult>(
            TAppContext appContext,
            IQuery<TResult> query)
        {
            var executor = Context.Container.Resolve<IQueryExecutor<TAppContext>>();
            return executor.GetAsync(appContext, query);
        }

        public Task<TResult> GetAsync<TResult>(
            IQuery<TResult> query)
        {
            var executor = Context.Container.Resolve<IQueryExecutor<TAppContext>>();
            return executor.GetAsync(GetDefaultContext(), query);
        }

        protected abstract TAppContext GetDefaultContext();
    }

    public abstract class CQRSTestBase<TAppContext>
        : CQRSTestBase<CQRSTestContextBase<TAppContext>, TAppContext>
        where TAppContext : IEventsContext
    {
        protected CQRSTestBase(CQRSTestContextBase<TAppContext> context)
            : base(context)
        { }
    }
}
