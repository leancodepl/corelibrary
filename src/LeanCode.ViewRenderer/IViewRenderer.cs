namespace LeanCode.ViewRenderer
{
    public interface IViewRenderer
    {
        /// <remarks>
        /// <typeparamref name="TModel" /> Should be a <b>public</p> type, cause currenty,
        /// the only implementation based on Razor, uses dynamic types and the object binder,
        /// respects the visibility.
        /// </remarks>
        string RenderToString<TModel>(string viewName, TModel model);
    }
}
