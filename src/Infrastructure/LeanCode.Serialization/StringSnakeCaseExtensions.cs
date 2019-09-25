using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LeanCode.Serialization
{
    public static class StringSnakeCaseExtensions
    {
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

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    if (state != SnakeCaseState.Start)
                    {
                        state = SnakeCaseState.NewWord;
                    }
                }
                else if (char.IsUpper(s[i]))
                {
                    switch (state)
                    {
                        case SnakeCaseState.Upper:
                            if (i > 0 && i + 1 < s.Length)
                            {
                                char nextChar = s[i + 1];

                                if (!char.IsUpper(nextChar) && nextChar != '_')
                                {
                                    sb.Append('_');
                                }
                            }

                            break;
                        case SnakeCaseState.Lower:
                        case SnakeCaseState.NewWord:
                            sb.Append('_');
                            break;
                    }

                    sb.Append(char.ToLowerInvariant(s[i]));

                    state = SnakeCaseState.Upper;
                }
                else if (s[i] == '_' || s[i] == '-')
                {
                    sb.Append('_');
                    state = SnakeCaseState.Start;
                }
                else
                {
                    if (state == SnakeCaseState.NewWord)
                    {
                        sb.Append('_');
                    }

                    sb.Append(s[i]);
                    state = SnakeCaseState.Lower;
                }
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
