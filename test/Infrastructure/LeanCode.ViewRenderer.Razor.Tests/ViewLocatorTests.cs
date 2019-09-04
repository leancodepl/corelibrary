using System.IO;
using Xunit;

namespace LeanCode.ViewRenderer.Razor.Tests
{
    public class ViewLocatorTests
    {
        private static readonly RazorViewRendererOptions BothDefault = new RazorViewRendererOptions(
            "./Views/A",
            "./Views/B");

        private static readonly RazorViewRendererOptions BothTXT = new RazorViewRendererOptions(
            ".cstxt",
            new[]
            {
                "./Views/A",
                "./Views/B",
            });

        [Fact]
        public void Returns_non_existing_item_if_view_cannot_be_located()
        {
            var locator = new ViewLocator(BothDefault);

            Assert.False(locator.GetItem("NonExistingView", null).Exists);
        }

        [Fact]
        public void Correctly_locates_view()
        {
            AssertPath("ViewC", "./Views/A/ViewC.cshtml");
        }

        [Fact]
        public void Selects_first_view_if_multiple_are_present()
        {
            AssertPath("ViewA", "./Views/A/ViewA.cshtml");
        }

        [Fact]
        public void Searches_all_locations()
        {
            AssertPath("ViewD", "./Views/B/ViewD.cshtml");
        }

        [Fact]
        public void Respects_different_extensions()
        {
            AssertPath("ViewB", "./Views/B/ViewB.cstxt", BothTXT);
        }

        private static void AssertPath(string viewName, string relativeLocation, RazorViewRendererOptions opts = null)
        {
            opts = opts ?? BothDefault;

            var expected = Path.GetFullPath(relativeLocation);
            var real = new ViewLocator(opts).GetItem(viewName, null);
            Assert.Equal(expected, real.PhysicalPath);
        }
    }
}
