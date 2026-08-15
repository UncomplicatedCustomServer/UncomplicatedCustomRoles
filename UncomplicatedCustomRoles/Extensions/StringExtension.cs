/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Extensions;

public static class StringExtension
{
    public static readonly HashSet<char> INTChars =
    [
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
    ];
    
    private static readonly Regex CustomInfoRejectedChars = new(@"[\[\]]|[^\p{L}\p{P}\p{Sc}\p{N} ^=+|~`<>\n]", RegexOptions.Compiled);
    
    public static string SanitizeCustomInfo(this string str)
    {
        return string.IsNullOrEmpty(str) ? str : CustomInfoRejectedChars.Replace(str, string.Empty);
    }

    public static string ToInt(this string str, string separator = "")
    {
        List<char> result = [];

        foreach (var ch in str)
            if (INTChars.Contains(ch))
                result.Add(ch);

        return string.Join(separator, result);
    }

    public static string BulkReplace(this string str, Dictionary<string, object> replace, string matrix = null)
    {
        foreach (var kvp in replace.Where(kvp => kvp.Value is not null))
            str = str.Replace(matrix is null ? kvp.Key : matrix.Replace("<val>", kvp.Key), kvp.Value?.ToString());

        return str;
    }

    public static string GenerateWithBuffer(this string str, int bufferSize)
    {
        for (var a = str.Length; a < bufferSize; a++)
            str += " ";

        return str;
    }

    public static string RemoveBracketsOnEndOfName(this string name)
    {
        var bracketStart = name.IndexOf('(');

        if (bracketStart > 0)
            name = name.Remove(bracketStart, name.Length - bracketStart);

        return name;
    }

    public static HttpStatusCode GetStatusCode(this string str, out string message)
    {
        LogManager.Debug($"Parsing JSON for status code: {str}");
        var doc = JsonDocument.Parse(str);
        var root = doc.RootElement;

        message = null;
        if (root.TryGetProperty("message", out var messageElement))
        {
            message = messageElement.GetString();
            LogManager.Debug($"Extracted message: {message}");
        }

        if (root.TryGetProperty("status", out var status) &&
            Enum.TryParse(status.ToString(), out HttpStatusCode statusCode))
        {
            LogManager.Debug($"Extracted status code: {statusCode}");
            return statusCode;
        }

        LogManager.Debug("Status code not found, returning HttpStatusCode.Unused");
        return HttpStatusCode.Unused;
    }
}