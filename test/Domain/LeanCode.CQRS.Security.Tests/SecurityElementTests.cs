using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Security.Exceptions;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Default.Tests.Security
{
    public class DefaultAuthorizerTests
    {
        private const string DerivedAttributeParam = nameof(DerivedAttributeParam);

        private readonly IAuthorizerResolver<ISecurityContext> authorizerResolver;
        private readonly CQRSSecurityElement<ISecurityContext, TestExecutionPayload, object> element;
        private IFirstAuthorizer firstAuthorizer;
        private ISecondAuthorizer secondAuthorizer;
        private IDerivedAuthorizer derivedAuthorizer;

        private ISecurityContext context;

        public DefaultAuthorizerTests()
        {
            authorizerResolver = Substitute.For<IAuthorizerResolver<ISecurityContext>>();

            element = new CQRSSecurityElement<ISecurityContext, TestExecutionPayload, object>(
                authorizerResolver);

            context = Substitute.For<ISecurityContext>();
            context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity("TEST")));
        }

        private void SetUpFirstAuthorizer(bool isPositive)
        {
            firstAuthorizer = Substitute.For<IFirstAuthorizer>();
            firstAuthorizer.UnderlyingAuthorizer.Returns(firstAuthorizer.GetType());
            firstAuthorizer.CheckIfAuthorizedAsync(context, Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResolver.FindAuthorizer(
                typeof(IFirstAuthorizer), Arg.Any<Type>()).Returns(firstAuthorizer);
        }

        private void SetUpSecondAuthorizer(bool isPositive)
        {
            secondAuthorizer = Substitute.For<ISecondAuthorizer>();
            secondAuthorizer.UnderlyingAuthorizer.Returns(secondAuthorizer.GetType());
            secondAuthorizer.CheckIfAuthorizedAsync(context, Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResolver.FindAuthorizer(typeof(ISecondAuthorizer), Arg.Any<Type>()).Returns(secondAuthorizer);
        }

        private void SetUpDerivedAuthorizer(bool isPositive)
        {
            derivedAuthorizer = Substitute.For<IDerivedAuthorizer>();
            derivedAuthorizer.UnderlyingAuthorizer.Returns(derivedAuthorizer.GetType());
            derivedAuthorizer.CheckIfAuthorizedAsync(context, Arg.Any<object>(), Arg.Any<object>()).Returns(isPositive);

            authorizerResolver.FindAuthorizer(typeof(IDerivedAuthorizer), Arg.Any<Type>()).Returns(derivedAuthorizer);
        }

        private Task Authorize(TestExecutionPayload payload)
        {
            return element.ExecuteAsync(
                context, payload,
                (ctx, i) => Task.FromResult<object>(null));
        }

        [Fact]
        public async Task Object_with_no_auhorizers_authorizes()
        {
            var obj = new TestExecutionPayload(new NoAuthorizers());

            await Authorize(obj);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Object_with_single_authorizer_authorizes_accordingly(bool isPositive)
        {
            var obj = new TestExecutionPayload(new SingleAuthorizer());
            SetUpFirstAuthorizer(isPositive);

            if (isPositive)
            {
                await Authorize(obj);
            }
            else
            {
                await Assert.ThrowsAsync<InsufficientPermissionException>(() => Authorize(obj));
            }
            _ = firstAuthorizer.Received().CheckIfAuthorizedAsync(context, obj, Arg.Any<object>());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Object_with_multiple_authorizers_authorizes_accordingly(bool isFirstAuthorizerPositive, bool isSecondAuthorizerPositive)
        {
            var obj = new TestExecutionPayload(new MultipleAuthorizers());
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
            var obj = new TestExecutionPayload(new DerivedAuthorizer());
            SetUpDerivedAuthorizer(isPositive);

            if (isPositive)
            {
                await Authorize(obj);
            }
            else
            {
                await Assert.ThrowsAsync<InsufficientPermissionException>(() => Authorize(obj));
            }
            _ = derivedAuthorizer.Received().CheckIfAuthorizedAsync(context, obj, DerivedAttributeParam);
        }

        [Fact]
        public async Task Requires_user_if_command_has_authorizers()
        {
            var obj = new TestExecutionPayload(new SingleAuthorizer());

            SetUpFirstAuthorizer(true);
            context.User.Returns((ClaimsPrincipal)null);

            await Assert.ThrowsAsync<UnauthenticatedException>(() => Authorize(obj));
        }

        [Fact]
        public async Task Requires_user_authentication_if_command_has_authorizers()
        {
            context.User.Returns((ClaimsPrincipal)null);
            var obj = new TestExecutionPayload(new SingleAuthorizer());

            SetUpFirstAuthorizer(true);

            await Assert.ThrowsAsync<UnauthenticatedException>(() => Authorize(obj));
        }

        [Fact]
        public async Task Does_not_require_user_authentication_if_command_does_not_have_authorizers()
        {
            context.User.Returns((ClaimsPrincipal)null);
            var obj = new TestExecutionPayload(new NoAuthorizers());

            await Authorize(obj);
        }

        [Fact]
        public async Task Does_not_require_user_if_command_does_not_have_authorizers()
        {
            context.User.Returns((ClaimsPrincipal)null);
            var obj = new TestExecutionPayload(new NoAuthorizers());

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

        public interface IFirstAuthorizer : ICustomAuthorizerWrapper
        { }

        public interface ISecondAuthorizer : ICustomAuthorizerWrapper
        { }

        public interface IDerivedAuthorizer : ICustomAuthorizerWrapper
        { }

        public class DerivedAuthorizeWhenAttribute : AuthorizeWhenAttribute
        {
            public DerivedAuthorizeWhenAttribute(string param)
                : base(typeof(IDerivedAuthorizer), param)
            { }
        }

        public class TestExecutionPayload : IExecutionPayload
        {
            public object Context { get; set; }
            public object Object { get; set; }

            public TestExecutionPayload(object obj)
            {
                Object = obj;
            }
        }
    }
}
