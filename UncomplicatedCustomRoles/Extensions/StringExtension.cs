/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class StringExtension
    {
        public static string ToInt(this string str, string separator = "")
        {
            List<char> result = new();

            foreach (char ch in str)
                // Faster than LINQ Contains on every iteration
                if (ch >= '0' && ch <= '9')
                    result.Add(ch);

            return string.Join(separator, result);
        }

        public static string BulkReplace(this string str, Dictionary<string, object> replace, string matrix = null)
        {
            // Avoid LINQ allocations each call; check null inline
            foreach (var kvp in replace)
            {
                if (kvp.Value is null)
                    continue;

                var key = matrix is null ? kvp.Key : matrix.Replace("<val>", kvp.Key);
                str = str.Replace(key, kvp.Value.ToString());
            }

            return str;
        }

        public static string GenerateWithBuffer(this string str, int bufferSize)
        {
            int diff = bufferSize - str.Length;
            if (diff <= 0)
                return str;
            return str + new string(' ', diff);
        }

        public static string RemoveBracketsOnEndOfName(this string name)
        {
            var bracketStart = name.IndexOf('(');

            if (bracketStart > 0)
                name = name.Remove(bracketStart, name.Length - bracketStart);

            return name;
        }

        /// <summary>
        /// Removes Unity rich text color tags, leaving inner text intact.
        /// Handles <color=#RRGGBB>, <color=#RRGGBBAA>, and other <color=...> forms.
        /// </summary>
        public static string StripColorTags(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            // Remove opening <color=...> tags
            string withoutOpen = Regex.Replace(input, "<color=[^>]+>", string.Empty, RegexOptions.IgnoreCase);
            // Remove closing </color> tags
            return Regex.Replace(withoutOpen, "</color>", string.Empty, RegexOptions.IgnoreCase);
        }
    }
}
