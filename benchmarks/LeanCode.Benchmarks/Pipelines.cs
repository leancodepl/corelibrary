using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LeanCode.Benchmarks.Pipelines;
using LeanCode.Pipelines;
using LeanCode.Pipelines.MicrosoftDI;
using Microsoft.Extensions.DependencyInjection;
using Executor = LeanCode.Pipelines.PipelineExecutor<
    LeanCode.Benchmarks.Pipelines.Context,
    LeanCode.Benchmarks.Pipelines.Input,
    LeanCode.Benchmarks.Pipelines.Output
>;

namespace LeanCode.Benchmarks;

[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[MarkdownExporterAttribute.Atlassian]
[MemoryDiagnoser]
public class PipelinesBenchmark
{
    private Executor emptyStatic;
    private Executor singleElementStatic;
    private Executor emptyMicrosoftDI;
    private Executor singleElementMicrosoftDI;

    [GlobalSetup]
    public void Setup()
    {
        var emptyCfg = Pipeline.Build<Context, Input, Output>().Finalize<Finalizer>();
        var singleCfg = Pipeline.Build<Context, Input, Output>().Use<PassthroughElement>().Finalize<Finalizer>();

        var services = new ServiceCollection();
        services.AddTransient<MicrosoftDIPipelineFactory>();
        services.AddSingleton<Finalizer>();
        services.AddSingleton<PassthroughElement>();
        var serviceProvider = services.BuildServiceProvider();

        emptyStatic = new Executor(new StaticFactory(), emptyCfg);
        singleElementStatic = new Executor(new StaticFactory(), singleCfg);
        emptyMicrosoftDI = new Executor(serviceProvider.GetRequiredService<MicrosoftDIPipelineFactory>(), emptyCfg);
        singleElementMicrosoftDI = new Executor(
            serviceProvider.GetRequiredService<MicrosoftDIPipelineFactory>(),
            singleCfg
        );
    }

    [Benchmark(Baseline = true)]
    public Task<Output> Empty() => emptyStatic.ExecuteAsync(new Context(), new Input());

    [Benchmark]
    public Task<Output> SingleElement() => singleElementStatic.ExecuteAsync(new Context(), new Input());

    [Benchmark]
    public Task<Output> EmptyMicrosoftDI() => emptyMicrosoftDI.ExecuteAsync(new Context(), new Input());

    [Benchmark]
    public Task<Output> SingleElementMicrosoftDI() => singleElementMicrosoftDI.ExecuteAsync(new Context(), new Input());
}
