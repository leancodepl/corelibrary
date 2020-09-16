namespace System
{
    /// <summary>
    /// Provides an enumeration of AM or PM to support 12-hour clock values in the <see cref="Time"/> type.
    /// </summary>
    /// <remarks>
    /// Though commonly used in English, these abbreviations derive from Latin.
    /// AM is an abbreviation for "Ante Meridiem", meaning "before mid-day".
    /// PM is an abbreviation for "Post Meridiem", meaning "after mid-day".
    /// </remarks>
    public enum Meridiem
    {
        /// <summary>
        /// An abbreviation for "Ante Meridiem", meaning "before mid-day".
        /// </summary>
        AM,

        /// <summary>
        /// An abbreviation for "Post Meridiem", meaning "after mid-day".
        /// </summary>
        PM,
    }
}
