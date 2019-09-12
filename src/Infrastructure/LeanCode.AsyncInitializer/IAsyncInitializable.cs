using System;
using System.Threading.Tasks;

namespace LeanCode.AsyncInitializer
{
    public interface IAsyncInitializable : IAsyncDisposable
    {
        int Order { get; }
        ValueTask InitializeAsync();
    }
}
