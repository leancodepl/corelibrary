using System.Security.Claims;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LeanCode.Components;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Benchmarks;

[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[MarkdownExporterAttribute.Atlassian]
[MemoryDiagnoser]
public class InProcCQRS__Queries
{
    private static readonly TypesCatalog Catalog = TypesCatalog.Of<InProcCQRS__Queries>();

    private readonly PlainQuery plainQuery = new PlainQuery();
    private readonly UserQuery userQuery = new UserQuery();
    private readonly AdminQuery adminQuery = new AdminQuery();
    private readonly SampleDTO stubResult = new SampleDTO();

    private IQueryExecutor<SampleAppContext> simple;
    private IQueryExecutor<SampleAppContext> secured;

    private SampleAppContext appContext;

    [GlobalSetup]
    public void Setup()
    {
        simple = PrepareExecutor(b => b);
        secured = PrepareExecutor(b => b.Secure());

        appContext = new SampleAppContext
        {
            User = new ClaimsPrincipal(
                new[] { new ClaimsIdentity(new[] { new Claim("role", "user"), }, "system", "sub", "role"), }
            ),
        };
    }

    [Benchmark(Baseline = true)]
    public Task<SampleDTO> QueryWithoutInlineObjContext() => simple.GetAsync(appContext, plainQuery);

    [Benchmark]
    public Task<SampleDTO> PlainQueryWithSecuredPipeline() => secured.GetAsync(appContext, plainQuery);

    [Benchmark]
    public Task<SampleDTO> SecuredQueryWithSecuredPipeline() => secured.GetAsync(appContext, userQuery);

    [Benchmark]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1031", Justification = "Deliberate approach.")]
    public async Task<SampleDTO> SecuredQueryButFailingWithSecuredPipeline()
    {
        try
        {
            return await secured.GetAsync(appContext, adminQuery);
        }
        catch
        {
            return stubResult;
        }
    }

    private static IQueryExecutor<SampleAppContext> PrepareExecutor(
        QueryBuilder<SampleAppContext> queryBuilder,
        params AppModule[] additionalModules
    )
    {
        var sc = new ServiceCollection();

        var benchModule = new BenchmarkModule();
        var module = new CQRSModule().WithCustomPipelines(Catalog, b => b, queryBuilder, b => b);

        module.ConfigureServices(sc);
        benchModule.ConfigureServices(sc);

        foreach (var mod in additionalModules)
        {
            mod.ConfigureServices(sc);
        }

        var serviceProvider = sc.BuildServiceProvider();

        var queryExecutor = serviceProvider.GetRequiredService<IQueryExecutor<SampleAppContext>>();
        return queryExecutor;
    }
}
