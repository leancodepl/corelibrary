using System.Threading.Tasks;

namespace LeanCode.AsyncInitializer
{
    public interface IAsyncInitializable
    {
        int Order { get; }
        Task InitializeAsync();
        Task DeinitializeAsync();
    }
}
