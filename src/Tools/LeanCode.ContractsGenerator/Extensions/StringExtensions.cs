namespace LeanCode.ContractsGenerator.Extensions
{
    internal static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            return char.ToLowerInvariant(str[0]) + str[1..];
        }
    }
}
