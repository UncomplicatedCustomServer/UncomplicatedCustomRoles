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
using System.Linq;
using System.Text.RegularExpressions;
using MEC;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

[Obsolete("This module is deprecated and will be removed in a future version. Use InfoTag instead.")]
public class CustomInfoOrder : CustomModule
{
    private static readonly string[] KnownTokens = ["custominfo", "nickname", "rolename"];

    private static readonly Regex TokenRegex = new("%([a-zA-Z_]+)%", RegexOptions.Compiled);

    internal string Order => TryGetStringValue("order", "%custominfo%%nickname%%rolename%");

    public override bool Validate(out string error)
    {
        var tokens = TokenRegex.Matches(Order).Cast<Match>().Select(m => m.Groups[1].Value).ToList();

        var unknown = tokens
            .Where(t => !KnownTokens.Contains(t, StringComparer.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        if (unknown.Count > 0)
            LogManager.Warn(
                $"[CustomModule] CustomInfoOrder 'order' contains unknown token(s): {string.Join(", ", unknown.Select(t => $"%{t}%"))}; they will be shown as-is. Valid tokens: %custominfo%, %nickname%, %rolename%.");

        if (!tokens.Any(t => KnownTokens.Contains(t, StringComparer.OrdinalIgnoreCase)))
        {
            error =
                "'order' must contain at least one of %custominfo%, %nickname% or %rolename%; otherwise the custom info would show static text only.";
            return false;
        }

        error = null;
        return true;
    }

    public override void OnAdded()
    {
        Timing.CallDelayed(Timing.WaitForOneFrame, () => { CustomRole.CustomInfo.UpdateInfo(CustomRole.Player); });
        base.OnAdded();
    }
}