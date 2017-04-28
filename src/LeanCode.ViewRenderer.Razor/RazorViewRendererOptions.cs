using System.Collections.Generic;

namespace LeanCode.ViewRenderer.Razor
{
    public class RazorViewRendererOptions
    {
        public const string DefaultExtension = ".cshtml";

        public IReadOnlyList<string> ViewLocations { get; }
        public string Extension { get; }

        public RazorViewRendererOptions(IReadOnlyList<string> viewLocations, string extension = DefaultExtension)
        {
            ViewLocations = viewLocations;
            Extension = extension;
        }

        public RazorViewRendererOptions(params string[] viewLocations)
        {
            ViewLocations = viewLocations;
            Extension = DefaultExtension;
        }

        public RazorViewRendererOptions(string extension, string[] viewLocations)
        {
            ViewLocations = viewLocations;
            Extension = extension;
        }
    }
}
