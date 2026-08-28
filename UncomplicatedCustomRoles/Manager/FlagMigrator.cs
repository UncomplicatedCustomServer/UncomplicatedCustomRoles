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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Manager;
#nullable enable

internal static class FlagMigrator
{
    private const string InfoTagDefaultOrder = "%custominfo%%nickname%%rolename%";
    private static readonly Regex RoleNameToken = new("%rolename%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    internal static List<ICustomRole> Migrated { get; } = [];

    internal static void Migrate(ICustomRole role)
    {
        if (role.CustomFlags is not { Count: > 0 } flags)
            return;

        string? order = null;
        string? nickColor = null;
        var hasOrder = false;
        var hasColor = false;
        var hasNoUnit = false;
        var hasInfoTag = false;
        List<object> deprecated = [];

        foreach (var flag in flags)
        {
            var (name, args) = Parse(flag);
            switch (name?.ToLowerInvariant())
            {
                case "infotag":
                    hasInfoTag = true;
                    break;
                case "custominfoorder":
                    hasOrder = true;
                    deprecated.Add(flag);
                    if (args is not null && args.TryGetValue("order", out var o))
                        order = o?.ToString();
                    break;
                case "colorfulnickname":
                    hasColor = true;
                    deprecated.Add(flag);
                    if (args is not null && args.TryGetValue("color", out var c))
                        nickColor = c?.ToString();
                    break;
                case "nounitname":
                    hasNoUnit = true;
                    deprecated.Add(flag);
                    break;
            }
        }

        if (deprecated.Count == 0)
            return;

        var used = DeprecatedList(hasOrder, hasColor, hasNoUnit);

        if (hasInfoTag)
        {
            foreach (var flag in deprecated)
                flags.Remove(flag);

            LogManager.Warn(
                $"[Flag Migrator] Role {role} uses both the new 'InfoTag' flag and the deprecated name-tag flag(s) {used}. " +
                "The deprecated one(s) were ignored; please remove them from your config.");
            return;
        }

        var infoOrder = string.IsNullOrEmpty(order) ? InfoTagDefaultOrder : order!;
        var infoArgs = new Dictionary<object, object>();

        if (hasNoUnit)
            infoArgs["show_unitname"] = false;
        else if (RoleNameToken.IsMatch(infoOrder))
            infoOrder = RoleNameToken.Replace(infoOrder, "%rolename% %unitname%");


        infoArgs["order"] = infoOrder;
        if (hasColor && !string.IsNullOrEmpty(nickColor))
            infoArgs["nickname_color"] = nickColor!;

        foreach (var flag in deprecated)
            flags.Remove(flag);

        flags.Add(new Dictionary<object, object> { ["InfoTag"] = infoArgs });

        if (!Migrated.Contains(role))
            Migrated.Add(role);

        LogManager.Warn(
            $"[Flag Migrator] Role {role} still uses the deprecated name-tag flag(s) {used}. " +
            "They were automatically migrated to the 'InfoTag' flag.\n" +
            $"To persist this to the config file automatically run 'ucr update {role.Id}' (or 'ucr update all'), " +
            "or replace those flags manually in your custom_flags with:\n" +
            RenderYaml(infoArgs));
    }

    private static string DeprecatedList(bool order, bool color, bool noUnit)
    {
        List<string> names = [];
        if (order) names.Add("CustomInfoOrder");
        if (color) names.Add("ColorfulNickname");
        if (noUnit) names.Add("NoUnitName");
        return string.Join(", ", names);
    }

    private static (string? name, Dictionary<string, object>? args) Parse(object flag)
    {
        switch (flag)
        {
            case string s:
                return (s, null);
            case Dictionary<object, object> d:
                foreach (var kv in d)
                {
                    var args = kv.Value as Dictionary<object, object>;
                    return (kv.Key?.ToString(),
                        args?.ToDictionary(x => x.Key.ToString(), x => x.Value));
                }

                return (null, null);
            default:
                return (null, null);
        }
    }

    private static string RenderYaml(Dictionary<object, object> infoArgs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("custom_flags:");
        sb.AppendLine("- InfoTag:");
        foreach (var kv in infoArgs)
        {
            var value = kv.Value is bool b ? b ? "true" : "false" : $"\"{kv.Value}\"";
            sb.AppendLine($"    {kv.Key}: {value}");
        }

        return sb.ToString().TrimEnd();
    }
}