using Autofac.Features.Indexed;
using LeanCode.Components;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    class StubIndex<T> : IIndex<TypesCatalog, T>
    {
        private readonly T obj;
        public T this[TypesCatalog key] => obj;

        public StubIndex(T obj)
        {
            this.obj = obj;
        }

        public bool TryGetValue(TypesCatalog key, out T value)
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
