namespace LeanCode.ViewRenderer.Templates
{
    public interface IViewRenderer
    {
        string RenderToString<TModel>(string viewName, TModel model);
    }
}
