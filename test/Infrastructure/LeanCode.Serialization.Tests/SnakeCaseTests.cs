using Xunit;

namespace LeanCode.Serialization.Tests
{
    public class SnakeCaseTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("snake_case", "snake_case")]
        [InlineData("lower case", "lower_case")]
        // [InlineData("UPPER CASE", "upper_case")] // not supported
        [InlineData("Title Case", "title_case")]
        [InlineData("camelCase", "camel_case")]
        [InlineData("PascalCase", "pascal_case")]
        [InlineData("Darwin_Case", "darwin_case")]
        [InlineData("kebab-case", "kebab_case")]
        [InlineData("MACRO_CASE", "macro_case")]
        [InlineData("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟", "𐓷𐓘𐓻𐓘𐓻𐓟_𐓣𐓟")]
        public void Correctly_converts_string_to_snake_case(string input, string expectedOutput)
        {
            var converted = input.ToSnakeCase();

            Assert.Equal(expectedOutput, converted);
        }
    }
}
