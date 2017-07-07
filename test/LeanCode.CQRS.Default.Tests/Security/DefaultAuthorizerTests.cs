using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.CQRS.Default.Security;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Default.Tests.Security
{
    public class DefaultAuthorizerTests
    {
        private const string DerivedAttributeParam = nameof(DerivedAttributeParam);

        private readonly HttpContext httpContext;
        private readonly DefaultAuthorizer authorizer;
        private readonly IAuthorizerResolver authorizerResovler;
        private IFirstAuthorizer firstAuthorizer;
        private ISecondAuthorizer secondAuthorizer;
        private IDerivedAuthorizer derivedAuthorizer;

        public DefaultAuthorizerTests()
        {
            authorizerResovler = Substitute.For<IAuthorizerResolver>();
            httpContext = Substitute.For<HttpContext>();

            var accessor = Substitute.For<IHttpContextAccessor>();
            accessor.HttpContext.Returns(httpContext);

            authorizer = new DefaultAuthorizer(accessor, authorizerResovler);

            httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity("TEST")));
        }

        private void SetUpFirstAuthorizer(bool isPositive)
        {
            firstAuthorizer = Substitute.For<IFirstAuthorizer>();
            firstAuthorizer.CheckIfAuthorized(Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResovler.FindAuthorizer(typeof(IFirstAuthorizer)).Returns(firstAuthorizer);
        }

        private void SetUpSecondAuthorizer(bool isPositive)
        {
            secondAuthorizer = Substitute.For<ISecondAuthorizer>();
            secondAuthorizer.CheckIfAuthorized(Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResovler.FindAuthorizer(typeof(ISecondAuthorizer)).Returns(secondAuthorizer);
        }

        private void SetUpDerivedAuthorizer(bool isPositive)
        {
            derivedAuthorizer = Substitute.For<IDerivedAuthorizer>();
            derivedAuthorizer.CheckIfAuthorized(Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResovler.FindAuthorizer(typeof(IDerivedAuthorizer)).Returns(derivedAuthorizer);
        }

        [Fact]
        public async Task Object_with_no_auhorizers_authorizes()
        {
            var obj = new NoAuthorizers();

            var result = await authorizer.CheckIfAuthorized(obj);

            Assert.Equal(AuthorizationResult.Authorized, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Object_with_single_authorizer_authorizes_accordingly(bool isPositive)
        {
            var obj = new SingleAuthorizer();
            SetUpFirstAuthorizer(isPositive);

            var result = await authorizer.CheckIfAuthorized(obj);

            if (isPositive)
            {
                Assert.Equal(AuthorizationResult.Authorized, result);
            }
            else
            {
                Assert.Equal(AuthorizationResult.InsufficientPermission, result);
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

            var result = await authorizer.CheckIfAuthorized(obj);

            if (isFirstAuthorizerPositive && isSecondAuthorizerPositive)
            {
                Assert.Equal(AuthorizationResult.Authorized, result);
            }
            else
            {
                Assert.Equal(AuthorizationResult.InsufficientPermission, result);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Object_with_derived_authorize_when_attribute_authorizes_correctly(bool isPositive)
        {
            var obj = new DerivedAuthorizer();
            SetUpDerivedAuthorizer(isPositive);

            var result = await authorizer.CheckIfAuthorized(obj);

            if (isPositive)
            {
                Assert.Equal(AuthorizationResult.Authorized, result);
            }
            else
            {
                Assert.Equal(AuthorizationResult.InsufficientPermission, result);
            }
            _ = derivedAuthorizer.Received().CheckIfAuthorized(obj, DerivedAttributeParam);
        }

        [Fact]
        public async Task Requires_user_if_command_has_authorizers()
        {
            var obj = new SingleAuthorizer();

            SetUpFirstAuthorizer(true);
            httpContext.User.Returns((ClaimsPrincipal)null);

            var result = await authorizer.CheckIfAuthorized(obj);

            Assert.Equal(AuthorizationResult.Unauthenticated, result);
        }

        [Fact]
        public async Task Requires_user_authentication_if_command_has_authorizers()
        {
            var obj = new SingleAuthorizer();

            SetUpFirstAuthorizer(true);
            httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var result = await authorizer.CheckIfAuthorized(obj);

            Assert.Equal(AuthorizationResult.Unauthenticated, result);
        }

        [Fact]
        public async Task Does_not_require_user_authentication_if_command_does_not_have_authorizers()
        {
            var obj = new NoAuthorizers();
            httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var result = await authorizer.CheckIfAuthorized(obj);

            Assert.Equal(AuthorizationResult.Authorized, result);
        }

        [Fact]
        public async Task Does_not_require_user_if_command_does_not_have_authorizers()
        {
            var obj = new NoAuthorizers();
            httpContext.User.Returns((ClaimsPrincipal)null);

            var result = await authorizer.CheckIfAuthorized(obj);

            Assert.Equal(AuthorizationResult.Authorized, result);
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
