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

        public Task<CommandResult> RunAsync<TContext, TCommand>(
            TAppContext appContext,
            TCommand command)
            where TCommand : ICommand<TContext>
        {
            var executor = Context.Container.Resolve<ICommandExecutor<TAppContext>>();
            return executor.RunAsync(appContext, command);
        }

        public Task<CommandResult> RunAsync<TContext, TCommand>(
            TCommand command)
            where TCommand : ICommand<TContext>
        {
            var executor = Context.Container.Resolve<ICommandExecutor<TAppContext>>();
            return executor.RunAsync(GetDefaultContext(), command);
        }

        public Task<TResult> GetAsync<TContext, TResult>(
            TAppContext appContext,
            IQuery<TContext, TResult> query)
        {
            var executor = Context.Container.Resolve<IQueryExecutor<TAppContext>>();
            return executor.GetAsync(appContext, query);
        }

        public Task<TResult> GetAsync<TContext, TResult>(
            IQuery<TContext, TResult> query)
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
