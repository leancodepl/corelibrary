using System.Threading.Tasks;
using LeanCode.CQRS.Validation;
using Xunit;

namespace LeanCode.CQRS.Tests
{
    public class ValidatorResolvingTests : BaseCQRSTests
    {
        public ValidatorResolvingTests()
        {
            Prepare();
        }

        [Fact]
        public void Resolves_validator()
        {
            var (handler, underlying) = FindSampleValidator();

            Assert.NotNull(handler);
            Assert.NotNull(underlying);
        }

        [Fact]
        public void Resovles_different_instances_for_subsequent_calls()
        {
            var (handler1, underlying1) = FindSampleValidator();
            var (handler2, underlying2) = FindSampleValidator();

            Assert.NotSame(handler1, handler2);
            Assert.NotSame(underlying1, underlying2);
        }

        [Fact]
        public void Resolves_null_when_validator_does_not_exist()
        {
            var handler = ValidatorResolver.FindCommandValidator(typeof(NoCHCommand));

            Assert.Null(handler);
        }

        [Fact]
        public async Task Correctly_passes_data_to_underlying_validator()
        {
            var appCtx = new AppContext();
            var objCtx = new ObjContext();
            var cmd = new SampleCommand();
            var expResult = new ValidationResult(new ValidationError[0]);

            var (handler, underlying) = FindSampleValidator();
            underlying.Result = expResult;

            var result = await handler.ValidateAsync(appCtx, objCtx, cmd);

            Assert.Equal(appCtx, underlying.AppContext);
            Assert.Equal(objCtx, underlying.Context);
            Assert.Equal(cmd, underlying.Command);
            Assert.Equal(expResult, result);
        }
    }
}
