using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.AsyncInitializer.Tests
{
    internal class Counter
    {
        private int i = -1;

        public int Next() => Interlocked.Increment(ref i);
    }

    internal class CountedInitializer : IAsyncInitializable
    {
        private readonly int order;
        private readonly Counter counter;

        public CountedInitializer(int order, Counter counter)
        {
            this.order = order;
            this.counter = counter;
        }

        public int? InitOrder { get; private set; }
        public int? DeinitOrder { get; private set; }

        int IAsyncInitializable.Order => order;

        Task IAsyncInitializable.InitializeAsync()
        {
            InitOrder = counter.Next();
            return Task.CompletedTask;
        }

        Task IAsyncInitializable.DeinitializeAsync()
        {
            DeinitOrder = counter.Next();
            return Task.CompletedTask;
        }
    }
}
