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
using System.Text.RegularExpressions;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;
#nullable enable

public class InfoTag : CustomModule
{
    internal const string DefaultOrder = "%custominfo%%nickname%%rolename% %unitname%";

    private static readonly string[] KnownTokens = ["custominfo", "nickname", "rolename", "unitname"];

    private static readonly Regex TokenRegex = new("%([a-zA-Z_]+)%", RegexOptions.Compiled);

    private static readonly Regex MultiNewline = new("\n{2,}", RegexOptions.Compiled);

    internal string Order => TryGetStringValue("order", DefaultOrder);

    internal string UnitFormat => TryGetStringValue("unit_format", "({unit})");

    internal bool ShowUnitName => TryGetCastedValue("show_unitname", true);

    internal bool ShowBadge => TryGetCastedValue("show_badge", true);

    internal bool ShowPowerStatus => TryGetCastedValue("show_powerstatus", true);

    private (string Token, string Color, bool Bold)[] Parts =>
    [
        ("custominfo", TryGetStringValue("custominfo_color"), TryGetCastedValue("custominfo_bold", false)),
        ("nickname", TryGetStringValue("nickname_color"), TryGetCastedValue("nickname_bold", false)),
        ("rolename", TryGetStringValue("rolename_color"), TryGetCastedValue("rolename_bold", false)),
        ("unitname", TryGetStringValue("unitname_color"), TryGetCastedValue("unitname_bold", false))
    ];

    public override bool Validate(out string error)
    {
        foreach ((string? token, string? color, bool _) in Parts)
            if (!string.IsNullOrWhiteSpace(color) && !InfoColors.TryResolve(color, out _))
            {
                error = $"'{token}_color' '{color}' is not a colour the game allows on the name tag. Allowed names: {string.Join(", ", InfoColors.Names)} (or an accepted hex code).";
                return false;
            }

        if (UnitFormat.Contains("[") || UnitFormat.Contains("]"))
        {
            error = $"'unit_format' cannot contain square brackets ('[' or ']'), got '{UnitFormat}'. Use () instead.";
            return false;
        }

        List<string> tokens = TokenRegex.Matches(Order).Cast<Match>().Select(m => m.Groups[1].Value).ToList();

        List<string> unknown = tokens.Where(t => !KnownTokens.Contains(t, StringComparer.OrdinalIgnoreCase)).Distinct().ToList();
        if (unknown.Count > 0)
            LogManager.Warn($"[CustomModule] InfoTag 'order' contains unknown token(s): {string.Join(", ", unknown.Select(t => $"%{t}%"))}; they will be shown as-is. Valid tokens: %custominfo%, %nickname%, %rolename%, %unitname%.");

        if (!tokens.Any(t => KnownTokens.Contains(t, StringComparer.OrdinalIgnoreCase)))
        {
            error = "'order' must contain at least one of %custominfo%, %nickname%, %rolename% or %unitname%; otherwise the name tag would show static text only.";
            return false;
        }

        error = null!;
        return true;
    }

    internal string Compose(Player player, string customInfoText, string nickname, string roleName, string unitName, bool showUnit)
    {
        Dictionary<string, (string Color, bool Bold)> parts = Parts.ToDictionary(p => p.Token, p => (p.Color, p.Bold));

        string template = Order.Replace("%%", "%\n%");
        string result = TokenRegex.Replace(template, m => Render(m.Groups[1].Value.ToLowerInvariant()));

        result = MultiNewline.Replace(result, "\n").Trim('\n', ' ');

        return string.IsNullOrEmpty(result) ? string.Empty : $"<color=#FFFFFF></color>{result}";

        string Render(string token)
        {
            string content = token switch
            {
                "custominfo" => customInfoText,
                "nickname" => string.IsNullOrEmpty(nickname) ? player.Nickname : nickname,
                "rolename" => roleName,
                "unitname" => showUnit && ShowUnitName && !string.IsNullOrEmpty(unitName) ? UnitFormat.Replace("{unit}", unitName) : string.Empty,
                _ => $"%{token}%"
            };

            if (string.IsNullOrEmpty(content) || !parts.TryGetValue(token, out (string Color, bool Bold) style))
                return content;

            if (style.Bold)
                content = $"<b>{content}</b>";

            if (!string.IsNullOrWhiteSpace(style.Color) && InfoColors.TryResolve(style.Color, out string? hex))
                content = $"<color=#{hex}>{content}</color>";

            return content;
        }
    }

    public override void OnAdded()
    {
        Timing.CallDelayed(Timing.WaitForOneFrame, () => { CustomRole.CustomInfo.UpdateInfo(CustomRole.Player); });
        base.OnAdded();
    }
}