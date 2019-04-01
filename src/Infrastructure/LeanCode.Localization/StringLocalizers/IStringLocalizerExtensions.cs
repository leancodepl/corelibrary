using System.Globalization;

namespace LeanCode.Localization.StringLocalizers
{
    public static class IStringLocalizerExtensions
    {
        public static string Format(
            this IStringLocalizer @this, CultureInfo culture,
            string name, params object[] arguments)
        {
            string format = @this[culture, name];
            return string.Format(culture, format, arguments);
        }
    }
}
