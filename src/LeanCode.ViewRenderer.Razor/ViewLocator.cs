using System.IO;

namespace LeanCode.ViewRenderer.Razor
{
    class ViewLocator
    {
        private readonly RazorViewRendererOptions options;

        public ViewLocator(RazorViewRendererOptions options)
        {
            this.options = options;
        }

        public string LocateView(string viewName) => LocateFile(GetFileName(viewName));

        private string GetFileName(string viewName) => viewName + options.Extension;

        private string LocateFile(string fileName)
        {
            foreach (var path in options.ViewLocations)
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath.Replace("\\", "/");
                }
            }
            return null;
        }
    }
}
