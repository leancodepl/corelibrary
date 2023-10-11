using System.Security.Claims;
using FluentAssertions;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Default.Tests.Security;

public class CustomAuthorizerTests
{
    private readonly AClass aClass = new() { AProperty = "Hello" };
    private readonly AnotherClass anotherClass = new() { AnotherProperty = "World" };

    [Fact]
    public async Task CustomAuthorizerTObjectTCustomData_casts_data_properly_as_ICustomAuthorizer()
    {
        var customAuthorizer = new CustomAuthorizer();

        await customAuthorizer
            .Awaiting(c => c.CheckIfAuthorizedAsync(default!, new object(), new object()))
            .Should()
            .ThrowAsync<InvalidCastException>();

        customAuthorizer.InternalWasCalled.Should().BeFalse();

        await customAuthorizer
            .Awaiting(c => c.CheckIfAuthorizedAsync(default!, aClass, new object()))
            .Should()
            .ThrowAsync<ArgumentException>();

        customAuthorizer.InternalWasCalled.Should().BeFalse();

        await customAuthorizer
            .Awaiting(c => c.CheckIfAuthorizedAsync(default!, aClass, anotherClass))
            .Should()
            .NotThrowAsync();

        customAuthorizer.InternalWasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task CustomAuthorizerTObjectTCustomData_casts_data_properly_as_IHttpContextCustomAuthorizer()
    {
        var httpContext = Substitute.For<HttpContext>();

        var customAuthorizer = new CustomAuthorizer();

        await customAuthorizer
            .Awaiting(
                c => ((IHttpContextCustomAuthorizer)c).CheckIfAuthorizedAsync(httpContext, new object(), new object())
            )
            .Should()
            .ThrowAsync<InvalidCastException>();

        customAuthorizer.InternalWasCalled.Should().BeFalse();

        await customAuthorizer
            .Awaiting(c => ((IHttpContextCustomAuthorizer)c).CheckIfAuthorizedAsync(httpContext, aClass, new object()))
            .Should()
            .ThrowAsync<ArgumentException>();

        customAuthorizer.InternalWasCalled.Should().BeFalse();

        await customAuthorizer
            .Awaiting(c => ((IHttpContextCustomAuthorizer)c).CheckIfAuthorizedAsync(httpContext, aClass, anotherClass))
            .Should()
            .NotThrowAsync();

        customAuthorizer.InternalWasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task HttpContextCustomAuthorizerTObjectTCustomData_casts_data_properly_as_IHttpContextCustomAuthorizer()
    {
        var customAuthorizer = new HttpContextCustomAuthorizer();

        await customAuthorizer
            .Awaiting(c => c.CheckIfAuthorizedAsync(default!, new object(), new object()))
            .Should()
            .ThrowAsync<InvalidCastException>();

        customAuthorizer.InternalWasCalled.Should().BeFalse();

        await customAuthorizer
            .Awaiting(c => ((IHttpContextCustomAuthorizer)c).CheckIfAuthorizedAsync(default!, aClass, new object()))
            .Should()
            .ThrowAsync<ArgumentException>();

        customAuthorizer.InternalWasCalled.Should().BeFalse();

        await customAuthorizer
            .Awaiting(c => ((IHttpContextCustomAuthorizer)c).CheckIfAuthorizedAsync(default!, aClass, anotherClass))
            .Should()
            .NotThrowAsync();

        customAuthorizer.InternalWasCalled.Should().BeTrue();
    }

    private class HttpContextCustomAuthorizer : HttpContextCustomAuthorizer<AClass, AnotherClass>
    {
        protected override Task<bool> CheckIfAuthorizedAsync(HttpContext context, AClass obj, AnotherClass customData)
        {
            InternalWasCalled = true;
            return Task.FromResult(true);
        }

        public bool InternalWasCalled { get; private set; }
    }

    private class CustomAuthorizer : CustomAuthorizer<AClass, AnotherClass>
    {
        protected override Task<bool> CheckIfAuthorizedAsync(
            ClaimsPrincipal user,
            AClass obj,
            AnotherClass customData,
            CancellationToken cancellationToken
        )
        {
            InternalWasCalled = true;
            return Task.FromResult(true);
        }

        public bool InternalWasCalled { get; private set; }
    }

    private class AClass
    {
        public string AProperty { get; set; }
    }

    private class AnotherClass
    {
        public string AnotherProperty { get; set; }
    }
}
