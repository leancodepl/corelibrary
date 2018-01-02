using System.Threading.Tasks;
using Autofac;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    class CommandExecutor<TAppContext> : ICommandExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly PipelineExecutor<TAppContext, CommandExecutionPayload, CommandResult> executor;

        private readonly ILifetimeScope lifetimeScope;

        public CommandExecutor(
            IPipelineFactory factory,
            CommandBuilder<TAppContext> config,
            ILifetimeScope lifetimeScope)
        {
            var cfg = Pipeline.Build<TAppContext, CommandExecutionPayload, CommandResult>()
                .Configure(new ConfigPipeline<TAppContext, CommandExecutionPayload, CommandResult>(config))
                .Finalize<CommandFinalizer<TAppContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
            this.lifetimeScope = lifetimeScope;
        }

        public Task<CommandResult> RunAsync<TContext>(
            TAppContext appcontext,
            TContext context,
            ICommand<TContext> command)
        {
            var payload = new CommandExecutionPayload(context, command);
            return executor.ExecuteAsync(appcontext, payload);
        }

        public Task<CommandResult> RunAsync<TContext>(
            TAppContext appContext,
            ICommand<TContext> command)
        {
            var factory = lifetimeScope.Resolve<IObjectContextFromAppContextFactory<TAppContext, TContext>>();
            var context = factory.Create(appContext);
            return RunAsync(appContext, context, command);
        }
    }
}
