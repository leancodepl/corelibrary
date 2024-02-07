using LeanCode.CQRS.AspNetCore.Local.Context;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullResponseCookiesTests
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Security",
        "CA5382:Use Secure Cookies In ASP.NET Core",
        Justification = "Tests."
    )]
    public void Append_should_not_throw()
    {
        NullResponseCookies.Empty.Append("", "");
    }

    [Fact]
    public void Append_with_options_should_not_throw()
    {
        NullResponseCookies.Empty.Append("", "", new());
    }

    [Fact]
    public void Delete_should_not_throw()
    {
        NullResponseCookies.Empty.Delete("");
    }

    [Fact]
    public void Delete_with_options_should_not_throw()
    {
        NullResponseCookies.Empty.Delete("", new());
    }
}
