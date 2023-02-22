using System;
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests;

public class StubQueryExecutor : IQueryExecutor<AppContext>
{
    public AppContext? LastAppContext { get; private set; }
    public IQuery? LastQuery { get; private set; }

    public object? NextResult { get; set; }

    public Task<TResult> GetAsync<TResult>(
        AppContext appContext,
        IQuery<TResult> query)
    {
        LastAppContext = appContext;
        LastQuery = query;
        if (NextResult is null)
        {
            return Task.FromResult(default(TResult)!);
        }
        else
        {
            return Task.FromResult((TResult)NextResult!);
        }
    }
}
