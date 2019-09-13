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
        public int? DisposeOrder { get; private set; }

        int IAsyncInitializable.Order => order;

        ValueTask IAsyncInitializable.InitializeAsync()
        {
            InitOrder = counter.Next();
            return default;
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            DisposeOrder = counter.Next();
            return default;
        }
    }
}
