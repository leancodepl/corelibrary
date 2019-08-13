using System.Linq;
using System.Text;

namespace LeanCode.ContractsGenerator.Extensions
{
    internal static class StringExtensions
    {
        public static string Uncapitalize(this string str)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static string Capitalize(this string str)
        {
            return char.ToUpperInvariant(str[0]) + str.Substring(1);
        }
    }
}
