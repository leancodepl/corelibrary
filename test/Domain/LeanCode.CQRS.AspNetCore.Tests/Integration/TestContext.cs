using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public class TestContext
{
    public static TestContext FromHttp(HttpContext context) => new();
}
