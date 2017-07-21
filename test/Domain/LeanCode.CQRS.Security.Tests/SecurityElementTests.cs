using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Security.Exceptions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.Domain.Default.Tests.Security
{
    public class DefaultAuthorizerTests
    {
        private const string DerivedAttributeParam = nameof(DerivedAttributeParam);

        private readonly HttpContext httpContext;
        private readonly IAuthorizerResolver authorizerResolver;
        private readonly SecurityElement<ExecutionContext, object, object> element;
        private IFirstAuthorizer firstAuthorizer;
        private ISecondAuthorizer secondAuthorizer;
        private IDerivedAuthorizer derivedAuthorizer;

        public DefaultAuthorizerTests()
        {
            authorizerResolver = Substitute.For<IAuthorizerResolver>();
            httpContext = Substitute.For<HttpContext>();

            var accessor = Substitute.For<IHttpContextAccessor>();
            accessor.HttpContext.Returns(httpContext);

            element = new SecurityElement<ExecutionContext, object, object>(
                accessor, authorizerResolver);

            httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity("TEST")));
        }

        private void SetUpFirstAuthorizer(bool isPositive)
        {
            firstAuthorizer = Substitute.For<IFirstAuthorizer>();
            firstAuthorizer.CheckIfAuthorized(Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResolver.FindAuthorizer(typeof(IFirstAuthorizer)).Returns(firstAuthorizer);
        }

        private void SetUpSecondAuthorizer(bool isPositive)
        {
            secondAuthorizer = Substitute.For<ISecondAuthorizer>();
            secondAuthorizer.CheckIfAuthorized(Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResolver.FindAuthorizer(typeof(ISecondAuthorizer)).Returns(secondAuthorizer);
        }

        private void SetUpDerivedAuthorizer(bool isPositive)
        {
            derivedAuthorizer = Substitute.For<IDerivedAuthorizer>();
            derivedAuthorizer.CheckIfAuthorized(Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResolver.FindAuthorizer(typeof(IDerivedAuthorizer)).Returns(derivedAuthorizer);
        }

        private Task Authorize(object obj)
        {
            return element.ExecuteAsync(
                new ExecutionContext(), obj,
                (ctx, i) => Task.FromResult<object>(null));
        }

        [Fact]
        public async Task Object_with_no_auhorizers_authorizes()
        {
            var obj = new NoAuthorizers();

            await Authorize(obj);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Object_with_single_authorizer_authorizes_accordingly(bool isPositive)
        {
            var obj = new SingleAuthorizer();
            SetUpFirstAuthorizer(isPositive);

            if (isPositive)
            {
                await Authorize(obj);
            }
            else
            {
                await Assert.ThrowsAsync<InsufficientPermissionException>(() => Authorize(obj));
            }
            _ = firstAuthorizer.Received().CheckIfAuthorized(obj, Arg.Any<object>());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Object_with_multiple_authorizers_authorizes_accordingly(bool isFirstAuthorizerPositive, bool isSecondAuthorizerPositive)
        {
            var obj = new MultipleAuthorizers();
            SetUpFirstAuthorizer(isFirstAuthorizerPositive);
            SetUpSecondAuthorizer(isSecondAuthorizerPositive);

            if (isFirstAuthorizerPositive && isSecondAuthorizerPositive)
            {
                await Authorize(obj);
            }
            else
            {
                await Assert.ThrowsAsync<InsufficientPermissionException>(() => Authorize(obj));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Object_with_derived_authorize_when_attribute_authorizes_correctly(bool isPositive)
        {
            var obj = new DerivedAuthorizer();
            SetUpDerivedAuthorizer(isPositive);

            if (isPositive)
            {
                await Authorize(obj);
            }
            else
            {
                await Assert.ThrowsAsync<InsufficientPermissionException>(() => Authorize(obj));
            }
            _ = derivedAuthorizer.Received().CheckIfAuthorized(obj, DerivedAttributeParam);
        }

        [Fact]
        public async Task Requires_user_if_command_has_authorizers()
        {
            var obj = new SingleAuthorizer();

            SetUpFirstAuthorizer(true);
            httpContext.User.Returns((ClaimsPrincipal)null);

            await Assert.ThrowsAsync<UnauthenticatedException>(() => Authorize(obj));
        }

        [Fact]
        public async Task Requires_user_authentication_if_command_has_authorizers()
        {
            var obj = new SingleAuthorizer();

            SetUpFirstAuthorizer(true);
            httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            await Assert.ThrowsAsync<UnauthenticatedException>(() => Authorize(obj));
        }

        [Fact]
        public async Task Does_not_require_user_authentication_if_command_does_not_have_authorizers()
        {
            var obj = new NoAuthorizers();
            httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            await Authorize(obj);
        }

        [Fact]
        public async Task Does_not_require_user_if_command_does_not_have_authorizers()
        {
            var obj = new NoAuthorizers();
            httpContext.User.Returns((ClaimsPrincipal)null);

            await Authorize(obj);
        }

        private class NoAuthorizers
        { }

        [AuthorizeWhen(typeof(IFirstAuthorizer))]
        private class SingleAuthorizer
        { }

        [AuthorizeWhen(typeof(IFirstAuthorizer))]
        [AuthorizeWhen(typeof(ISecondAuthorizer))]
        private class MultipleAuthorizers
        { }

        [DerivedAuthorizeWhen(DerivedAttributeParam)]
        private class DerivedAuthorizer
        { }

        public interface IFirstAuthorizer : ICustomAuthorizer
        { }

        public interface ISecondAuthorizer : ICustomAuthorizer
        { }

        public interface IDerivedAuthorizer : ICustomAuthorizer
        { }

        public class DerivedAuthorizeWhenAttribute : AuthorizeWhenAttribute
        {
            public DerivedAuthorizeWhenAttribute(string param)
                : base(typeof(IDerivedAuthorizer), param)
            { }
        }
    }
}
