using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq;

internal static class EnumerableAsyncExtensions
{
    internal static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source)
    {
        await foreach (var item in source)
        {
            return item;
        }

        return default!;
    }

    internal static Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source, Predicate<T> predicate)
    {
        return source.WhereAsync(predicate).FirstOrDefaultAsync();
    }

    internal static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Task<TResult>> selector
    )
    {
        foreach (var item in source)
        {
            yield return await selector(item);
        }
    }

    internal static async IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> source, Predicate<T> predicate)
    {
        await foreach (var item in source)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }
}
