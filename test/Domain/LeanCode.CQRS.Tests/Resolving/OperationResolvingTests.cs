using System.Threading.Tasks;
using Xunit;

namespace LeanCode.CQRS.Tests
{
    public class OperationResolvingTests : BaseCQRSTests
    {
        public OperationResolvingTests()
        {
            Prepare();
        }

        [Fact]
        public void Resolves_OH_for_existing_operation()
        {
            var (handler, underlying) = FindSampleOperationHandler();

            Assert.NotNull(handler);
            Assert.NotNull(underlying);
        }

        [Fact]
        public void Resolves_diffrent_OH_if_called_multiple_times_for_existing_operation()
        {
            var (handler1, _) = FindSampleOperationHandler();
            var (handler2, _) = FindSampleOperationHandler();

            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.NotSame(handler1, handler2);
        }

        [Fact]
        public void Resolves_different_instances_of_underlying_OH_when_resolving_the_same_operation_multiple_times()
        {
            var (_, underlying1) = FindSampleOperationHandler();
            var (_, underlying2) = FindSampleOperationHandler();

            Assert.NotNull(underlying1);
            Assert.NotNull(underlying2);
            Assert.NotSame(underlying1, underlying2);
        }

        [Fact]
        public void The_wrapper_type_is_the_same_when_resolved_the_same_operation_multiple_times()
        {
            var (handler1, _) = FindSampleOperationHandler();
            var (handler2, _) = FindSampleOperationHandler();

            Assert.Same(handler1.GetType(), handler2.GetType());
        }

        [Fact]
        public void Resolves_null_if_OH_is_not_found()
        {
            var handler = OHResolver.FindOperationHandler(typeof(NoOHOperation));

            Assert.Null(handler);
        }

        [Fact]
        public async Task The_data_is_correctly_passed_to_underlying_OH()
        {
            var appCtx = new AppContext();
            var operation = new SampleOperation();
            var resultObj = new object();

            var (handler, underlying) = FindSampleOperationHandler();
            underlying.Result = resultObj;

            var result = await handler.ExecuteAsync(appCtx, operation);

            Assert.Equal(operation, underlying.Operation);
            Assert.Equal(appCtx, underlying.Context);
            Assert.Equal(resultObj, result);
        }
    }
}
