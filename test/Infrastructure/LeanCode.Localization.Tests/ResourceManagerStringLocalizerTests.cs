using System;
using LeanCode.Localization.StringLocalizers;
using Xunit;
using static System.Globalization.CultureInfo;

namespace LeanCode.Localization.Tests
{
    public class ResourceManagerStringLocalizerTests
    {
        private readonly ResourceManagerStringLocalizer stringLocalizer;

        public ResourceManagerStringLocalizerTests()
        {
            this.stringLocalizer = new ResourceManagerStringLocalizer(
                new LocalizationConfiguration()
                {
                    ResourceSource = typeof(ResourceManagerStringLocalizerTests)
                });
        }

        [Fact]
        public void Correct_string_is_returned_for_InvariantCulture()
        {
            string value = stringLocalizer[InvariantCulture, "order.simple"];

            Assert.Equal("Order", value);
        }

        [Fact]
        public void Correct_string_is_returned_for_specific_culture()
        {
            string value = stringLocalizer[GetCultureInfo("pl"), "order.simple"];

            Assert.Equal("Zamówienie", value);
        }

        [Fact]
        public void Correct_string_is_returned_for_more_specific_culture()
        {
            string value = stringLocalizer[GetCultureInfo("pl-PL"), "order.simple"];

            Assert.Equal("Zamówienie", value);
        }

        [Fact]
        public void Correct_string_is_returned_for_another_more_specific_culture()
        {
            string value = stringLocalizer[GetCultureInfo("pl_pl"), "order.simple"];

            Assert.Equal("Zamówienie", value);
        }

        [Fact]
        public void Default_string_is_returned_for_missing_culture()
        {
            string value = stringLocalizer[GetCultureInfo("es"), "order.simple"];

            Assert.Equal("Order", value);
        }

        [Fact]
        public void Exception_is_thrown_when_one_of_arguments_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = stringLocalizer[null, "order"];
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = stringLocalizer[InvariantCulture, null];
            });
        }

        [Fact]
        public void Exception_is_thrown_when_resource_is_missing()
        {
            Assert.Throws<LocalizedResourceNotFoundException>(() =>
            {
                _ = stringLocalizer[InvariantCulture, "missing"];
            });
        }
    }
}
