using LeanCode.Contracts;
using LeanCode.CQRS.Execution;

namespace LeanCode.TestBed.Api;

[Obsolete]
public class TestQuery : IQuery<TestQueryResult> { }

public class TestQueryResult
{
    public Guid Id { get; set; }
    public string Property1 { get; set; }
    public TestQueryResult? Inner { get; set; }
}

public class TestQueryQH : IQueryHandler<TestQuery, TestQueryResult>
{
    public Task<TestQueryResult> ExecuteAsync(HttpContext context, TestQuery query)
    {
        return Task.FromResult(
            new TestQueryResult
            {
                Id = Guid.NewGuid(),
                Property1 = "Some value",
                Inner = new() { Id = Guid.NewGuid(), Property1 = "Other value" },
            }
        );
    }
}
