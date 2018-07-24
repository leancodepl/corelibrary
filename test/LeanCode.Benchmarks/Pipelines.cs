using BenchmarkDotNet.Attributes;
using LeanCode.Pipelines;
using LeanCode.Benchmarks.Pipelines;
using System.Threading.Tasks;
using Autofac;
using LeanCode.Pipelines.Autofac;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Attributes.Exporters;

namespace LeanCode.Benchmarks
{
    using Executor = PipelineExecutor<Context, Input, Output>;

    [CoreJob]
    [MarkdownExporterAttribute.Atlassian]
    [MemoryDiagnoser]
    public class PipelinesBenchmark
    {
        private Executor emptyStatic;
        private Executor singleElementStatic;
        private Executor emptyAutofac;
        private Executor singleElementAutofac;

        [GlobalSetup]
        public void Setup()
        {
            var emptyCfg = Pipeline.Build<Context, Input, Output>()
                .Finalize<Finalizer>();
            var singleCfg = Pipeline.Build<Context, Input, Output>()
                .Use<PassthroughElement>()
                .Finalize<Finalizer>();

            var builder = new ContainerBuilder();
            builder.RegisterType<AutofacPipelineFactory>().AsSelf();
            builder.RegisterType<Finalizer>().AsSelf().SingleInstance();
            builder.RegisterType<PassthroughElement>().AsSelf().SingleInstance();
            var container = builder.Build();

            emptyStatic = new Executor(new StaticFactory(), emptyCfg);
            singleElementStatic = new Executor(new StaticFactory(), singleCfg);
            emptyAutofac = new Executor(container.Resolve<AutofacPipelineFactory>(), emptyCfg);
            singleElementAutofac = new Executor(container.Resolve<AutofacPipelineFactory>(), singleCfg);
        }

        [Benchmark(Baseline = true)]
        public Task<Output> Empty() =>
            emptyStatic.ExecuteAsync(new Context(), new Input());

        [Benchmark]
        public Task<Output> SingleElement() =>
            singleElementStatic.ExecuteAsync(new Context(), new Input());

        [Benchmark]
        public Task<Output> EmptyAutofac() =>
            emptyAutofac.ExecuteAsync(new Context(), new Input());

        [Benchmark]
        public Task<Output> SingleElementAutofac() =>
            singleElementAutofac.ExecuteAsync(new Context(), new Input());
    }
}
