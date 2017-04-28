using System.IO;
using System.Threading.Tasks;

namespace LeanCode.ViewRenderer
{
    public interface IViewRenderer
    {
        /// <remarks>
        /// <see cref="RenderToStream" /> is generally faster alternative, use that if possible.
        ///
        /// <typeparamref name="TModel" /> Should be a <b>public</p> type, cause currenty,
        /// the only implementation based on Razor, uses dynamic types and the object binder,
        /// respects the visibility.
        /// </remarks>
        Task<string> RenderToString<TModel>(string viewName, TModel model);

        /// <remarks>
        /// <typeparamref name="TModel" /> Should be a <b>public</p> type, cause currenty,
        /// the only implementation based on Razor, uses dynamic types and the object binder,
        /// respects the visibility.
        /// </remarks>
        Task RenderToStream<TModel>(string viewName, TModel model, Stream outputStream);
    }
}
