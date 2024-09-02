// Based on https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/String.Searching.cs#L26C13-L26C56
// as linked by docs on 02.09.2024

namespace System;

public static class MissingStringMethods
{
    public static bool Contains(this string s, string value, StringComparison comparisonType) =>
        s.IndexOf(value, comparisonType) >= 0;
}
