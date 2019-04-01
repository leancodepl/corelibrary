using System.Globalization;

namespace LeanCode.Localization.StringLocalizers
{
    /// <summary>
    /// Represents a service that provides localized strings for any given culture.
    /// </summary>
    public interface IStringLocalizer
    {
        /// <summary>
        /// Gets the string resource with the given name and localized for the specified culture.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to get the string for.</param>
        /// <param name="name">The name of the string resource.</param>
        /// <returns>The localized string.</returns>
        /// <exception cref="System.ArgumentNullException">One or both arguments are `null`.</exception>
        /// <exception cref="LocalizedResourceNotFoundException">Localized resource could not be found.</exception>
        string this[CultureInfo culture, string name] { get; }
    }
}
