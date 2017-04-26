using LeanCode.CQRS.Default.Security;
using LeanCode.CQRS.Security;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Default.Tests.Security
{
    public class DefaultAuthorizerTests
    {
        private const string DerivedAttributeParam = nameof(DerivedAttributeParam);

        private readonly DefaultAuthorizer authorizer;
        private readonly IAuthorizerResolver authorizerResovler;
        private IFirstAuthorizer firstAuthorizer;
        private ISecondAuthorizer secondAuthorizer;
        private IDerivedAuthorizer derivedAuthorizer;

        public DefaultAuthorizerTests()
        {
            authorizerResovler = Substitute.For<IAuthorizerResolver>();

            authorizer = new DefaultAuthorizer(authorizerResovler);
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
        public void Object_with_no_auhorizers_authorizes()
        {
            var obj = new NoAuthorizers();

            var isAuthorized = authorizer.CheckIfAuthorized(obj);

            Assert.True(isAuthorized);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Object_with_single_authorizer_authorizes_accordingly(bool isPositive)
        {
            var obj = new SingleAuthorizer();
            SetUpFirstAuthorizer(isPositive);

            var isAuthorized = authorizer.CheckIfAuthorized(obj);

            Assert.Equal(isPositive, isAuthorized);
            firstAuthorizer.Received().CheckIfAuthorized(obj, Arg.Any<object>());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Object_with_multiple_authorizers_authorizes_accordingly(bool isFirstAuthorizerPositive, bool isSecondAuthorizerPositive)
        {
            var obj = new MultipleAuthorizers();
            SetUpFirstAuthorizer(isFirstAuthorizerPositive);
            SetUpSecondAuthorizer(isSecondAuthorizerPositive);

            var isAuthorized = authorizer.CheckIfAuthorized(obj);

            Assert.Equal(isFirstAuthorizerPositive && isSecondAuthorizerPositive, isAuthorized);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Object_with_derived_authorize_when_atribute_authorizes_correctly(bool isPositive)
        {
            var obj = new DerivedAuthorizer();
            SetUpDerivedAuthorizer(isPositive);

            var isAuthorized = authorizer.CheckIfAuthorized(obj);

            Assert.Equal(isPositive, isAuthorized);
            derivedAuthorizer.Received().CheckIfAuthorized(obj, DerivedAttributeParam);
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
