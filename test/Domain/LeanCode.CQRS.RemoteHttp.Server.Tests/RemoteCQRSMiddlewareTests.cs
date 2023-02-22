using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests;

public class RemoteCQRSMiddlewareTests : BaseMiddlewareTests
{
    public RemoteCQRSMiddlewareTests()
        : base("query", typeof(RemoteCQRSMiddlewareTests)) { }

    [Fact]
    public async Task Writes_MethodNotAllowed_if_using_PUT()
    {
        var (status, _) = await Invoke("/query", method: "PUT");
        Assert.Equal(StatusCodes.Status405MethodNotAllowed, status);
    }

    [Fact]
    public async Task Passes_execution_further_if_path_cannot_be_matched()
    {
        var (status, _) = await Invoke("/non-query");
        Assert.Equal(PipelineContinued, status);
    }
}
