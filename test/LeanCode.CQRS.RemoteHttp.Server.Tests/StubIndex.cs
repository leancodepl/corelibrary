using System.Reflection;
using Autofac.Features.Indexed;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    class StubIndex<T> : IIndex<Assembly, T>
    {
        private readonly T obj;
        public T this[Assembly key] => obj;

        public StubIndex(T obj)
        {
            this.obj = obj;
        }

        public bool TryGetValue(Assembly key, out T value)
        {
            value = obj;
            return true;
        }
    }

    static class StubIndex
    {
        public static StubIndex<T> Create<T>(T obj) => new StubIndex<T>(obj);
    }
}
