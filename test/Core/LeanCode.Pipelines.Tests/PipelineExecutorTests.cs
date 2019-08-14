using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Element = LeanCode.Pipelines.IPipelineElement<LeanCode.Pipelines.PipelineContext, int, int>;
using Executor = LeanCode.Pipelines.PipelineExecutor<LeanCode.Pipelines.PipelineContext, int, int>;
using Finalizer = LeanCode.Pipelines.IPipelineFinalizer<LeanCode.Pipelines.PipelineContext, int, int>;
using Next = System.Func<LeanCode.Pipelines.PipelineContext, int, System.Threading.Tasks.Task<int>>;

namespace LeanCode.Pipelines.Tests
{
    public class PipelineExecutorTests
    {
        [Fact]
        public async Task Executes_finalizer_when_there_are_no_elements()
        {
            var fin = Finalizer(0, 1);

            var result = await Prepare(fin).ExecuteAsync(new PipelineContext(), 0);

            _ = fin.Received().SubExecuteAsync(0);
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task Executes_single_element()
        {
            var el = Pass(0);
            var fin = Finalizer(1, 2);

            await Prepare(fin, el).ExecuteAsync(new PipelineContext(), 0);

            _ = el.Received().SubExecuteAsync(0);
        }

        [Fact]
        public async Task Executes_multiple_elements()
        {
            var el1 = Pass(0);
            var el2 = Pass(0);
            var fin = Finalizer(0, 2);

            await Prepare(fin, el1, el2).ExecuteAsync(new PipelineContext(), 0);

            _ = el1.Received().SubExecuteAsync(0);
            _ = el2.Received().SubExecuteAsync(0);
        }

        [Fact]
        public async Task Executes_finalizer_if_all_elements_pass()
        {
            var el1 = Pass(0);
            var el2 = Pass(0);
            var fin = Finalizer(0, 2);

            await Prepare(fin, el1, el2).ExecuteAsync(new PipelineContext(), 0);

            _ = fin.Received().SubExecuteAsync(0);
        }

        [Fact]
        public async Task If_element_breaks_pipeline_Following_elements_are_not_executed()
        {
            var el1 = Pass(0);
            var el2 = Break(0, 10);
            var el3 = Pass(10);
            var fin = Finalizer(10, 2);

            await Prepare(fin, el1, el2, el3).ExecuteAsync(new PipelineContext(), 0);

            _ = el1.Received().SubExecuteAsync(0);
            _ = el2.Received().SubExecuteAsync(0);
            _ = el3.DidNotReceiveWithAnyArgs().SubExecuteAsync(0);
            _ = fin.DidNotReceiveWithAnyArgs().SubExecuteAsync(0);
        }

        [Fact]
        public async Task Respects_element_modifications()
        {
            var el1 = Mod(0, 1);
            var el2 = Mod(1, 1);
            var fin = Finalizer(2, 10);

            var result = await Prepare(fin, el1, el2).ExecuteAsync(new PipelineContext(), 0);

            _ = el1.Received().SubExecuteAsync(0);
            _ = el2.Received().SubExecuteAsync(1);
            _ = fin.Received().SubExecuteAsync(2);

            Assert.Equal(10, result);
        }

        private static Element Pass(int input)
        {
            var e = Substitute.For<Element>();
            e.SubExecuteAsync(input).Returns(c => c.Arg<Next>().Invoke(c.Arg<PipelineContext>(), input));
            return e;
        }

        private static Element Mod(int input, int mod)
        {
            var e = Substitute.For<Element>();
            e.SubExecuteAsync(input).Returns(c => c.Arg<Next>().Invoke(c.Arg<PipelineContext>(), input + mod));
            return e;
        }

        private static Element Break(int input, int output)
        {
            var e = Substitute.For<Element>();
            e.SubExecuteAsync(input).Returns(Task.FromResult(output));
            return e;
        }

        private static Finalizer Finalizer(int input, int output)
        {
            var fin = Substitute.For<Finalizer>();
            fin.SubExecuteAsync(input).Returns(Task.FromResult(output));
            return fin;
        }

        private static Executor Prepare(
            Finalizer fin,
            params Element[] elements)
        {
            var scope = Substitute.For<IPipelineScope>();
            var factory = Substitute.For<IPipelineFactory>();

            factory.BeginScope().Returns(scope);

            scope.ResolveFinalizer<PipelineContext, int, int>(null)
                .ReturnsForAnyArgs(fin);

            if (elements.Length > 0)
            {
                scope.ResolveElement<PipelineContext, int, int>(null)
                    .ReturnsForAnyArgs(elements[0], elements.Skip(1).ToArray());
            }

            var cfg = new ConfiguredPipeline<PipelineContext, int, int>(
                    elements
                    .Select(e => e.GetType())
                    .ToArray(),
                    fin.GetType());
            return PipelineExecutor.Create(factory, cfg);
        }
    }

    internal static class PipelineExtensions
    {
        public static Task<int> SubExecuteAsync(
            this Element el,
            int input)
        {
            return el.ExecuteAsync(Arg.Any<PipelineContext>(), input, Arg.Any<Func<PipelineContext, int, Task<int>>>());
        }

        public static Task<int> SubExecuteAsync(
            this Finalizer el,
            int input)
        {
            return el.ExecuteAsync(Arg.Any<PipelineContext>(), input);
        }

        public static Task<int> SubExecuteAsync(
            this Executor el,
            int input)
        {
            return el.ExecuteAsync(Arg.Any<PipelineContext>(), input);
        }
    }
}
