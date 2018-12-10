using System.Linq;
using System.Text;

namespace LeanCode.ContractsGenerator.Extensions
{
    static class StringExtensions
    {
        public static string Uncapitalize(this string str)
        {
            return str.First().ToString().ToLowerInvariant() + string.Join("", str.Skip(1));
        }

        public static string Capitalize(this string str)
        {
            return str.First().ToString().ToUpperInvariant() + string.Join("", str.Skip(1));
        }
    }
}
