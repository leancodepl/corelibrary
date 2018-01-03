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
            var objCtx = new ObjContext();
            var cmd = new SingleInstanceCommand();
            var handler = Container.Resolve<SingleInstanceCommandHandler>();

            var result = await CommandExecutor.RunAsync(appCtx, objCtx, cmd);

            Assert.True(result.WasSuccessful);
            Assert.Equal(objCtx, handler.Context);
            Assert.Equal(cmd, handler.Command);
        }

        [Fact]
        public async Task Correctly_routes_data_through_pipeline_elements()
        {
            Prepare(
                c => c.Use<SamplePipelineElement<CommandExecutionPayload, CommandResult>>()
            );

            var appCtx = new AppContext();
            var objCtx = new ObjContext();
            var cmd = new SampleCommand();

            var element = Container.Resolve<SamplePipelineElement<CommandExecutionPayload, CommandResult>>();

            await CommandExecutor.RunAsync(appCtx, objCtx, cmd);

            Assert.Equal(appCtx, element.AppContext);
            Assert.Equal(objCtx, element.Data.Context);
            Assert.Equal(cmd, element.Data.Object);
        }

        [Fact]
        public async Task Creates_obj_context_based_on_app_context_if_needed()
        {
            Prepare();

            var appCtx = new AppContext();
            var cmd = new SingleInstanceCommand();

            var handler = Container.Resolve<SingleInstanceCommandHandler>();

            await CommandExecutor.RunAsync(appCtx, cmd);

            Assert.Equal(appCtx, handler.Context.SourceAppContext);
            Assert.Equal(cmd, handler.Command);
        }
    }
}
