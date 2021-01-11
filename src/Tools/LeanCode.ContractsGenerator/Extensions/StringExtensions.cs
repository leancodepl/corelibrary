namespace LeanCode.ContractsGenerator.Extensions
{
    internal static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            return char.ToLowerInvariant(str[0]) + str[1..];
        }

        public static string ToPascalCase(this string str)
        {
            return char.ToUpperInvariant(str[0]) + str[1..];
        }
    }
}
