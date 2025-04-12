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

        public static string BulkReplace(this string str, Dictionary<string, object> replace, string matrix = null)
        {
            foreach (KeyValuePair<string, object> kvp in replace)
                str.Replace(matrix is null ? kvp.Key : matrix.Replace("<val>", kvp.Key), kvp.Value.ToString());

            return str;
        }

        public static string GenerateWithBuffer(this string str, int bufferSize)
        {
            for (int a = str.Length; a < bufferSize; a++)
                str += " ";

            return str;
        }
    }
}
