using System.Threading.Tasks;

namespace LeanCode.AsyncTasks
{
    public interface IAsyncTask
    {
        Task RunAsync();
    }

    public interface IAsyncTask<TParams>
    {
        Task RunAsync(TParams @params);
    }
}
