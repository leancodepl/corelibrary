using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace LeanCode.Pipelines.Tests
{
    public class PipelineExecutorTests
    {
        [Fact]
        public async Task Executes_finalizer_when_there_are_no_elements()
        {
            var fin = Finalizer(0, 1);

            var result = await Prepare(fin).ExecuteAsync(0);

            _ = fin.Received().ExecuteAsync(0);
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task Executes_single_element()
        {
            var el = Pass(0);
            var fin = Finalizer(1, 2);

            var result = await Prepare(fin, el).ExecuteAsync(0);

            _ = el.Received().ExecuteAsync(0, Arg.Any<Func<int, Task<int>>>());
        }

        [Fact]
        public async Task Executes_multiple_elements()
        {
            var el1 = Pass(0);
            var el2 = Pass(0);
            var fin = Finalizer(0, 2);

            var result = await Prepare(fin, el1, el2).ExecuteAsync(0);

            _ = el1.Received().ExecuteAsync(0, Arg.Any<Func<int, Task<int>>>());
            _ = el2.Received().ExecuteAsync(0, Arg.Any<Func<int, Task<int>>>());
        }

        [Fact]
        public async Task Executes_finalizer_if_all_elements_pass()
        {
            var el1 = Pass(0);
            var el2 = Pass(0);
            var fin = Finalizer(0, 2);

            var result = await Prepare(fin, el1, el2).ExecuteAsync(0);

            _ = fin.Received().ExecuteAsync(0);
        }

        [Fact]
        public async Task If_element_breaks_pipeline_Following_elements_are_not_executed()
        {
            var el1 = Pass(0);
            var el2 = Break(0, 10);
            var el3 = Pass(10);
            var fin = Finalizer(10, 2);

            var result = await Prepare(fin, el1, el2, el3).ExecuteAsync(0);

            _ = el1.Received().ExecuteAsync(0, Arg.Any<Func<int, Task<int>>>());
            _ = el2.Received().ExecuteAsync(0, Arg.Any<Func<int, Task<int>>>());
            _ = el3.DidNotReceiveWithAnyArgs().ExecuteAsync(0, null);
            _ = fin.DidNotReceiveWithAnyArgs().ExecuteAsync(0);
        }

        [Fact]
        public async Task Respects_element_modifications()
        {
            var el1 = Mod(0, 1);
            var el2 = Mod(1, 1);
            var fin = Finalizer(2, 10);

            var result = await Prepare(fin, el1, el2).ExecuteAsync(0);

            _ = el1.Received().ExecuteAsync(0, Arg.Any<Func<int, Task<int>>>());
            _ = el2.Received().ExecuteAsync(1, Arg.Any<Func<int, Task<int>>>());
            _ = fin.Received().ExecuteAsync(2);

            Assert.Equal(10, result);
        }

        private static IPipelineElement<int, int> Pass(int input)
        {
            var e = Substitute.For<IPipelineElement<int, int>>();
            e.ExecuteAsync(input, Arg.Any<Func<int, Task<int>>>())
                .Returns(c => c.Arg<Func<int, Task<int>>>().Invoke(input));
            return e;
        }

        private static IPipelineElement<int, int> Mod(int input, int mod)
        {
            var e = Substitute.For<IPipelineElement<int, int>>();
            e.ExecuteAsync(input, Arg.Any<Func<int, Task<int>>>())
                .Returns(c => c.Arg<Func<int, Task<int>>>().Invoke(input + mod));
            return e;
        }

        private static IPipelineElement<int, int> Break(int input, int output)
        {
            var e = Substitute.For<IPipelineElement<int, int>>();
            e.ExecuteAsync(input, Arg.Any<Func<int, Task<int>>>())
                .Returns(Task.FromResult(output));
            return e;
        }

        private static IPipelineFinalizer<int, int> Finalizer(int input, int output)
        {
            var fin = Substitute.For<IPipelineFinalizer<int, int>>();
            fin.ExecuteAsync(input).Returns(Task.FromResult(output));
            return fin;
        }

        private static PipelineExecutor<TInput, TOutput> Prepare<TInput, TOutput>(
            IPipelineFinalizer<TInput, TOutput> fin,
            params IPipelineElement<TInput, TOutput>[] elements
        )
        {
            var scope = Substitute.For<IPipelineScope>();
            var factory = Substitute.For<IPipelineFactory>();

            factory.BeginScope().Returns(scope);

            scope.ResolveFinalizer<TInput, TOutput>(null)
                .ReturnsForAnyArgs(fin);

            if (elements.Length > 0)
            {
                scope.ResolveElement<TInput, TOutput>(null)
                    .ReturnsForAnyArgs(elements[0], elements.Skip(1).ToArray());
            }

            var cfg = new ConfiguredPipeline<TInput, TOutput>(
                    elements
                    .Select(e => e.GetType())
                    .ToArray(),
                fin.GetType());
            return PipelineExecutor.Create(factory, cfg);
        }
    }
}
