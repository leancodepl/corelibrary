using System.Threading.Tasks;
using Autofac;
using BenchmarkDotNet.Attributes;
using LeanCode.Components;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Security;
using LeanCode.Cache.AspNet;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Reflection;

namespace LeanCode.Benchmarks
{
    [CoreJob]
    [MarkdownExporterAttribute.Atlassian]
    [MemoryDiagnoser]
    public class InProcCQRS__Queries
    {
        private static Assembly assembly = typeof(InProcCQRS__Queries).Assembly;
        private static TypesCatalog catalog = new TypesCatalog(assembly);

        private IQueryExecutor<SampleAppContext> simple;
        private IQueryExecutor<SampleAppContext> cached;
        private IQueryExecutor<SampleAppContext> secured;

        private PlainQuery plainQuery = new PlainQuery();
        private CachedQuery cachedQuery = new CachedQuery();
        private UserQuery userQuery = new UserQuery();
        private AdminQuery adminQuery = new AdminQuery();
        private SampleDTO stubResult = new SampleDTO();
        private SampleObjContext objContext = new SampleObjContext();
        private SampleAppContext appContext;

        [GlobalSetup]
        public void Setup()
        {
            simple = PrepareExecutor(b => b);
            cached = PrepareExecutor(b => b.Cache(), new InMemoryCacheModule());
            secured = PrepareExecutor(b => b.Secure());

            appContext = new SampleAppContext
            {
                User = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(new []
                    {
                        new Claim("role", "user")
                    }, "system", "sub", "role")
                })
            };
        }

        [Benchmark(Baseline = true)]
        public Task<SampleDTO> QueryWithoutInlineObjContext() =>
            simple.GetAsync(appContext, plainQuery);

        [Benchmark]
        public Task<SampleDTO> PlainQueryWithCachedPipeline() =>
            cached.GetAsync(appContext, cachedQuery);

        [Benchmark]
        public Task<SampleDTO> CachedQueryWithCachedPipeline() =>
            cached.GetAsync(appContext, cachedQuery);

        [Benchmark]
        public Task<SampleDTO> PlainQueryWithSecuredPipeline() =>
            secured.GetAsync(appContext, plainQuery);

        [Benchmark]
        public Task<SampleDTO> SecuredQueryWithSecuredPipeline() =>
            secured.GetAsync(appContext, userQuery);

        [Benchmark]
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
            params AppModule[] additionalModules)
        {
            var builder = new ContainerBuilder();
            var sc = new ServiceCollection();

            var benchModule = new BenchmarkModule();
            var module = new CQRSModule()
                .WithCustomPipelines<SampleAppContext>(catalog, b => b, queryBuilder);

            builder.RegisterModule(module);
            module.ConfigureServices(sc);
            builder.RegisterModule(benchModule);
            benchModule.ConfigureServices(sc);

            foreach (var mod in additionalModules)
            {
                builder.RegisterModule(mod);
                mod.ConfigureServices(sc);
            }

            builder.Populate(sc);
            var container = builder.Build();

            var queryExecutor = container.Resolve<IQueryExecutor<SampleAppContext>>();
            return queryExecutor;
        }
    }
}
