using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LeanCode.Serialization
{
    public static class StringSnakeCaseExtensions
    {
        private static readonly Rune LowLine = new Rune('_');
        private static readonly Rune HyphenMinus = new Rune('-');
        private static readonly Rune Space = new Rune(' ');

        // https://github.com/JamesNK/Newtonsoft.Json/blob/6b9f467e817854532ea31e6c08abe47c53ac8b5c/Src/Newtonsoft.Json/Utilities/StringUtils.cs#L214
        [return: NotNullIfNotNull("s")]
        public static string? ToSnakeCase(this string? s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var sb = new StringBuilder(s.Length * 2);
            var state = SnakeCaseState.Start;

            for (var i = 0; i < s.Length;)
            {
                var rune = Rune.GetRuneAt(s, i);

                if (rune == Space)
                {
                    if (state != SnakeCaseState.Start)
                    {
                        state = SnakeCaseState.NewWord;
                    }
                }
                else if (Rune.IsUpper(rune))
                {
                    switch (state)
                    {
                        case SnakeCaseState.Upper:
                            if (i > 0 && i + rune.Utf16SequenceLength < s.Length)
                            {
                                var nextRune = Rune.GetRuneAt(s, i + rune.Utf16SequenceLength);

                                if (!Rune.IsUpper(nextRune) && nextRune != LowLine)
                                {
                                    sb.Append(LowLine);
                                }
                            }

                            break;
                        case SnakeCaseState.Lower:
                        case SnakeCaseState.NewWord:
                            sb.Append(LowLine);
                            break;
                    }

                    sb.Append(Rune.ToLowerInvariant(rune));

                    state = SnakeCaseState.Upper;
                }
                else if (rune == LowLine || rune == HyphenMinus)
                {
                    sb.Append(LowLine);
                    state = SnakeCaseState.Start;
                }
                else
                {
                    if (state == SnakeCaseState.NewWord)
                    {
                        sb.Append(LowLine);
                    }

                    sb.Append(rune);
                    state = SnakeCaseState.Lower;
                }

                i += rune.Utf16SequenceLength;
            }

            return sb.ToString();
        }

        private enum SnakeCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord,
        }
    }
}
