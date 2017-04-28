using Xunit;

namespace LeanCode.ViewRenderer.Razor.Tests
{
    public class ViewLocatorTests
    {
        private static RazorViewRendererOptions BothDefault = new RazorViewRendererOptions(
            "./Views/A",
            "./Views/B"
        );

        private static RazorViewRendererOptions BothTXT = new RazorViewRendererOptions(
            ".cstxt",
            new[] {
                "./Views/A",
                "./Views/B"
            }
        );

        [Fact]
        public void Returns_null_if_view_cannot_be_located()
        {
            var locator = new ViewLocator(BothDefault);

            Assert.Null(locator.LocateView("NonExistingView"));
        }

        [Fact]
        public void Correctly_locates_view()
        {
            var locator = new ViewLocator(BothDefault);

            Assert.Equal("./Views/A/ViewC.cshtml", locator.LocateView("ViewC"));
        }

        [Fact]
        public void Selects_first_view_if_multiple_are_present()
        {
            var locator = new ViewLocator(BothDefault);

            Assert.Equal("./Views/A/ViewA.cshtml", locator.LocateView("ViewA"));
        }

        [Fact]
        public void Searches_all_locations()
        {
            var locator = new ViewLocator(BothDefault);

            Assert.Equal("./Views/B/ViewD.cshtml", locator.LocateView("ViewD"));
        }

        [Fact]
        public void Respects_different_extensions()
        {
            var locator = new ViewLocator(BothTXT);

            Assert.Equal("./Views/B/ViewB.cstxt", locator.LocateView("ViewB"));
        }
    }
}
