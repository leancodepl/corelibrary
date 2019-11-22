using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq
{
    internal static class EnumerableAsyncExtensions
    {
        internal static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
            this IEnumerable<T> source, Func<T, Task<TResult>> selector)
        {
            foreach (var item in source)
            {
                yield return await selector(item);
            }
        }
    }
}
