using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LeanCode.Components;
using LeanCode.CQRS;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.CQRS.Validation.Fluent;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MarkdownExporterAttribute.Atlassian]
    [MemoryDiagnoser]
    public class InProcCQRS__Commands
    {
        private static readonly TypesCatalog Catalog = TypesCatalog.Of<InProcCQRS__Commands>();

        private readonly PlainCommand plainCommand = new PlainCommand();
        private readonly UserCommand userCommand = new UserCommand();
        private readonly ValidCommand validCommand = new ValidCommand();
        private readonly InvalidCommand invalidCommand = new InvalidCommand();
        private readonly CommandResult stubResult = CommandResult.Success;

        private ICommandExecutor<SampleAppContext> simple;
        private ICommandExecutor<SampleAppContext> secured;
        private ICommandExecutor<SampleAppContext> validated;

        private SampleAppContext appContext;

        [GlobalSetup]
        public void Setup()
        {
            simple = PrepareExecutor(b => b);
            secured = PrepareExecutor(b => b.Secure());
            validated = PrepareExecutor(b => b.Validate(), new FluentValidationModule(Catalog));

            appContext = new SampleAppContext
            {
                User = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim("role", "user"),
                        },
                        "system", "sub", "role"),
                }),
            };
        }

        [Benchmark(Baseline = true)]
        public Task<CommandResult> CommandWithoutInlineObjContext() =>
            simple.RunAsync(appContext, plainCommand);

        [Benchmark]
        public Task<CommandResult> PlainCommandWithSecuredPipeline() =>
            secured.RunAsync(appContext, plainCommand);

        [Benchmark]
        public Task<CommandResult> SecuredCommandWithSecuredPipeline() =>
            secured.RunAsync(appContext, userCommand);

        [Benchmark]
        public async Task<CommandResult> SecuredButFailingCommandWithSecuredPipeline()
        {
            try
            {
                return await secured.RunAsync(appContext, userCommand);
            }
            catch
            {
                return stubResult;
            }
        }

        [Benchmark]
        public Task<CommandResult> PlainCommandWithValidatedPipeline() =>
            validated.RunAsync(appContext, plainCommand);

        [Benchmark]
        public Task<CommandResult> ValidCommandWithValidatedPipeline() =>
            validated.RunAsync(appContext, validCommand);

        [Benchmark]
        public Task<CommandResult> InvalidCommandWithValidatedPipeline() =>
            validated.RunAsync(appContext, invalidCommand);

        private static ICommandExecutor<SampleAppContext> PrepareExecutor(
            CommandBuilder<SampleAppContext> commandBuilder,
            params AppModule[] additionalModules)
        {
            var builder = new ContainerBuilder();
            var sc = new ServiceCollection();

            var benchModule = new BenchmarkModule();
            var module = new CQRSModule()
                .WithCustomPipelines<SampleAppContext>(Catalog, commandBuilder, b => b);

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

            var commandExecutor = container.Resolve<ICommandExecutor<SampleAppContext>>();
            return commandExecutor;
        }
    }
}
