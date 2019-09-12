using System.Globalization;

namespace LeanCode.Localization.StringLocalizers
{
    public static class IStringLocalizerExtensions
    {
        public static string Format(
            this IStringLocalizer @this,
            CultureInfo culture,
            string name,
            params object[] arguments)
        {
            return string.Format(culture, @this[culture, name], arguments);
        }
    }
}
