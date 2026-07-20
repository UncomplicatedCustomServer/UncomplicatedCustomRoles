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

namespace UncomplicatedCustomRoles.Manager;
#nullable enable

internal static class InfoColors
{
    private static readonly Dictionary<string, string> NameToHex = new(StringComparer.OrdinalIgnoreCase)
    {
        { "pink", "FF96DE" },
        { "red", "C50000" },
        { "brown", "944710" },
        { "silver", "A0A0A0" },
        { "lightgreen", "32CD32" },
        { "crimson", "DC143C" },
        { "cyan", "00B7EB" },
        { "aqua", "00FFFF" },
        { "deeppink", "FF1493" },
        { "tomato", "FF6448" },
        { "yellow", "FAFF86" },
        { "magenta", "FF0090" },
        { "bluegreen", "4DFFB8" },
        { "orange", "FF9966" },
        { "lime", "BFFF00" },
        { "green", "228B22" },
        { "emerald", "50C878" },
        { "carmine", "960018" },
        { "nickel", "727472" },
        { "mint", "98FB98" },
        { "armygreen", "4B5320" },
        { "pumpkin", "EE7600" },
        { "white", "FFFFFF" },
        { "black", "000000" }
    };
    
    internal static IEnumerable<string> Names => NameToHex.Keys;
    
    internal static bool TryResolve(string? input, out string hex)
    {
        hex = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var raw = input!.Trim().TrimStart('#').Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);

        if (NameToHex.TryGetValue(raw, out var mapped))
        {
            hex = mapped;
            return true;
        }

        if (raw.Length == 6 && NameToHex.Values.Any(h => string.Equals(h, raw, StringComparison.OrdinalIgnoreCase)))
        {
            hex = raw.ToUpperInvariant();
            return true;
        }

        return false;
    }
}
