using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class StubCommandExecutor : ICommandExecutor<AppContext>
    {
        public static readonly ValidationError SampleError = new ValidationError("Prop", "999", 2);

        public AppContext LastAppContext { get; private set; }
        public object LastContext { get; private set; }
        public ICommand LastCommand { get; private set; }

        public Task<CommandResult> RunAsync<TContext>(
            AppContext appContext,
            TContext context,
            ICommand<TContext> command)
        {
            LastAppContext = appContext;
            LastContext = context;
            LastCommand = command;
            if (LastCommand is SampleRemoteCommand cmd && cmd.Prop == 999)
            {
                return Task.FromResult(CommandResult.NotValid(
                    new ValidationResult(new[] { SampleError })
                ));
            }
            return Task.FromResult(CommandResult.Success());
        }

        public Task<CommandResult> RunAsync<TContext>(
            AppContext appContext,
            ICommand<TContext> command)
        {
            if (typeof(TContext) == typeof(ObjContext))
            {
                var ctx = new ObjContextFromAppContextFactory().Create(appContext);
                return RunAsync(appContext, ctx, (ICommand<ObjContext>)command);
            }
            else if (typeof(TContext) == typeof(ObjContextWoCtor))
            {
                var ctx = new ObjContextWoCtorFromAppContextFactory().Create(appContext);
                return RunAsync(appContext, ctx, (ICommand<ObjContextWoCtor>)command);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
