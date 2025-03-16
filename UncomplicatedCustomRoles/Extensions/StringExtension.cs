using System.Collections.Generic;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class StringExtension
    {
        public static readonly char[] _intChars = new[]
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9'
        };

        public static string ToInt(this string str, string separator = "")
        {
            List<char> result = new();

            foreach (char ch in str)
                if (_intChars.Contains(ch))
                    result.Add(ch);

            return string.Join(separator, result);
        }
    }
}
