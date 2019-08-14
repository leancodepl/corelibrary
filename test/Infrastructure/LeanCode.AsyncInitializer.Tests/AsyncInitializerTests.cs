using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace LeanCode.AsyncInitializer.Tests
{
    public class AsyncInitializerTests
    {
        private readonly Counter globalCounter = new Counter();

        [Fact]
        public async Task StartAsync_creates_appropriate_scope_and_gets_correct_services()
        {
            var (_, init) = Prepare();

            await init.StartAsync(Substitute.For<IHttpApplication<object>>(), default);
        }

        [Fact]
        public async Task StopAsync_creates_appropriate_scope_and_gets_correct_services()
        {
            var (_, init) = Prepare();

            await init.StopAsync(default);
        }

        [Fact]
        public async Task StartAsync_calls_InitializeAsync_on_every_object()
        {
            var (sp, init) = Prepare(
                Init(0),
                Init(1),
                Init(2));

            await init.StartAsync(Substitute.For<IHttpApplication<object>>(), default);

            Assert.NotNull(sp.Initializers[0].InitOrder);
            Assert.NotNull(sp.Initializers[1].InitOrder);
            Assert.NotNull(sp.Initializers[2].InitOrder);
        }

        [Fact]
        public async Task StopAsync_calls_DisposeAsync_on_every_object()
        {
            var (sp, init) = Prepare(
                Init(0),
                Init(1),
                Init(2));

            await init.StopAsync(default);

            Assert.NotNull(sp.Initializers[0].DisposeOrder);
            Assert.NotNull(sp.Initializers[1].DisposeOrder);
            Assert.NotNull(sp.Initializers[2].DisposeOrder);
        }

        [Fact]
        public async Task StopAsync_orders_the_objects_in_reverse_order_before_calling_InitializeAsync()
        {
            var (sp, init) = Prepare(
                Init(1),
                Init(2),
                Init(0));

            await init.StopAsync(default);

            Assert.Equal(1, sp.Initializers[0].DisposeOrder);
            Assert.Equal(0, sp.Initializers[1].DisposeOrder);
            Assert.Equal(2, sp.Initializers[2].DisposeOrder);
        }

        [Fact]
        public async Task StartAsync_orders_the_objects_before_calling_InitializeAsync()
        {
            var (sp, init) = Prepare(
                Init(1),
                Init(2),
                Init(0));

            await init.StartAsync(Substitute.For<IHttpApplication<object>>(), default);

            Assert.Equal(1, sp.Initializers[0].InitOrder);
            Assert.Equal(2, sp.Initializers[1].InitOrder);
            Assert.Equal(0, sp.Initializers[2].InitOrder);
        }

        private (StubProvider, AsyncInitializer) Prepare(params CountedInitializer[] inits)
        {
            var sp = new StubProvider(inits.ToList());
            var server = Substitute.For<IServer>();
            return (sp, new AsyncInitializer(sp, server));
        }

        private CountedInitializer Init(int order)
        {
            return new CountedInitializer(order, globalCounter);
        }
    }
}
