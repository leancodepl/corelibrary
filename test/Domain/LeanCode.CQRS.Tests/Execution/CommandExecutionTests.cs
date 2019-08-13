using System.Threading.Tasks;
using Autofac;
using LeanCode.CQRS.Execution;
using Xunit;

namespace LeanCode.CQRS.Tests
{
    public class CommandExecutionTests : BaseCQRSTests
    {
        [Fact]
        public async Task Executes_command_handler_with_correct_data()
        {
            Prepare();

            var appCtx = new AppContext();
            var cmd = new SingleInstanceCommand();
            var handler = Container.Resolve<SingleInstanceCommandHandler>();

            var result = await CommandExecutor.RunAsync(appCtx, cmd);

            Assert.True(result.WasSuccessful);
            Assert.Equal(appCtx, handler.Context);
            Assert.Equal(cmd, handler.Command);
        }

        [Fact]
        public async Task Correctly_routes_data_through_pipeline_elements()
        {
            Prepare(
                c => c.Use<SamplePipelineElement<ICommand, CommandResult>>());

            var appCtx = new AppContext();
            var cmd = new SampleCommand();

            var element = Container.Resolve<SamplePipelineElement<ICommand, CommandResult>>();

            await CommandExecutor.RunAsync(appCtx, cmd);

            Assert.Equal(appCtx, element.AppContext);
            Assert.Equal(cmd, element.Data);
        }

        [Fact]
        public async Task Creates_obj_context_based_on_app_context_if_needed()
        {
            Prepare();

            var appCtx = new AppContext();
            var cmd = new SingleInstanceCommand();

            var handler = Container.Resolve<SingleInstanceCommandHandler>();

            await CommandExecutor.RunAsync(appCtx, cmd);

            Assert.Equal(appCtx, handler.Context);
            Assert.Equal(cmd, handler.Command);
        }
    }
}
