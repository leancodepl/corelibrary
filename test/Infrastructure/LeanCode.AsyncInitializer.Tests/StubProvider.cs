using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AsyncInitializer.Tests
{
    class StubProvider : IServiceProvider, IServiceScopeFactory, IServiceScope
    {
        private readonly InitProvider provider;

        public List<CountedInitializer> Initializers { get; private set; }

        public StubProvider(List<CountedInitializer> initializers)
        {
            Initializers = initializers;
            provider = new InitProvider(initializers.Cast<IAsyncInitializable>());
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceScopeFactory))
            {
                return this;
            }
            else
            {
                throw new InvalidOperationException("Service not supported");
            }
        }

        IServiceScope IServiceScopeFactory.CreateScope() => this;
        IServiceProvider IServiceScope.ServiceProvider => provider;

        void IDisposable.Dispose()
        { }

        class InitProvider : IServiceProvider
        {
            private readonly IEnumerable<IAsyncInitializable> initializers;

            public InitProvider(IEnumerable<IAsyncInitializable> initializers)
            {
                this.initializers = initializers;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IEnumerable<IAsyncInitializable>))
                {
                    return initializers;
                }
                else
                {
                    throw new InvalidOperationException("Service not supported");
                }
            }
        }
    }
}
