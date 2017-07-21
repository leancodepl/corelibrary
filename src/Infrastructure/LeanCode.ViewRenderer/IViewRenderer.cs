using System.IO;
using System.Threading.Tasks;

namespace LeanCode.ViewRenderer
{
    public interface IViewRenderer
    {
        /// <remarks>
        /// <see cref="RenderToStream" /> is generally faster alternative, use that if possible.
        ///
        /// <paramref name="model" /> should be a <b>public</b> type because currenty
        /// the (only) implementation based on Razor uses dynamic types and the object binder
        /// respects the visibility.
        /// </remarks>
        Task<string> RenderToString(string viewName, object model);

        /// <remarks>
        /// <paramref name="model" /> should be a <b>public</b> type because currenty
        /// the (only) implementation based on Razor uses dynamic types and the object binder
        /// respects the visibility.
        /// </remarks>
        Task RenderToStream(string viewName, object model, Stream outputStream);
    }
}
