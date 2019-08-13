using System.Linq;
using System.Text;

namespace LeanCode.ContractsGenerator.Extensions
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendSpaces(this StringBuilder builder, int level)
        {
            return builder.Append(string.Join(string.Empty, Enumerable.Repeat("    ", level)));
        }
    }
}
