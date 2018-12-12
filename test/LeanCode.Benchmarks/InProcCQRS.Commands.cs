using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
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
    [CoreJob]
    [MarkdownExporterAttribute.Atlassian]
    [MemoryDiagnoser]
    public class InProcCQRS__Commands
    {
        private static Assembly assembly = typeof(InProcCQRS__Commands).Assembly;
        private static TypesCatalog catalog = new TypesCatalog(assembly);

        private ICommandExecutor<SampleAppContext> simple;
        private ICommandExecutor<SampleAppContext> secured;
        private ICommandExecutor<SampleAppContext> validated;

        private PlainCommand plainCommand = new PlainCommand();
        private UserCommand userCommand = new UserCommand();
        private AdminCommand adminCommand = new AdminCommand();
        private ValidCommand validCommand = new ValidCommand();
        private InvalidCommand invalidCommand = new InvalidCommand();

        private CommandResult stubResult = CommandResult.Success();

        private SampleObjContext objContext = new SampleObjContext();
        private SampleAppContext appContext;

        [GlobalSetup]
        public void Setup()
        {
            simple = PrepareExecutor(b => b);
            secured = PrepareExecutor(b => b.Secure());
            validated = PrepareExecutor(b => b.Validate(), new FluentValidationModule(catalog));

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
                return await secured.RunAsync(appContext, userCommand); ;
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
                .WithCustomPipelines<SampleAppContext>(catalog, commandBuilder, b => b);

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
