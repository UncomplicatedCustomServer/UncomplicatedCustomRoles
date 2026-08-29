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
using CommandSystem;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Commands;

public class Percentages : IUCRCommand
{
    public string Name { get; } = "percentages";

    public string Description { get; } = "See every spawn percentage of any role";

    public string RequiredPermission { get; } = "ucr.percentages";

    public bool Executor(List<string> args, ICommandSender sender, out string response)
    {
        response = "Spawn percentages for each base Role:";

        foreach (RoleTypeId role in Enum.GetValues(typeof(RoleTypeId)))
        {
            List<ICustomRole> customRoles = CustomRole.List.Where(r =>
                r.SpawnSettings?.CanReplaceRoles != null && r.SpawnSettings.CanReplaceRoles.Contains(role) &&
                !r.IgnoreSpawnSystem && r.SpawnSettings.SpawnDelay <= 0).ToList();

            if (!customRoles.Any())
                continue;

            float total = customRoles.Sum(r => r.SpawnSettings.SpawnChance);

            float effective = Math.Min(total, 100);
            response += $"\n\n{(total >= 100 ? "<color=#ff0000>❗</color>" : "<color=#00ff00>✔️</color>")} <color={role.GetColor().ToHex()}><b>{role.GetFullName()}</b></color> ({customRoles.Count})";
            response += $"\nChance of spawning as a <b>CustomRole</b>: {effective}%\nChance of spawning as a regular role: {100 - effective}%";

            if (total > 100)
                response += $"\n<color=#ff0000>The configured chances add up to {total}%, so this role is always replaced and the chances below only weight which CustomRole wins.</color>";

            foreach (ICustomRole customRole in customRoles.OrderByDescending(r => r.SpawnSettings.SpawnChance))
            {
                float chance = customRole.SpawnSettings.SpawnChance;

                float actual = total <= 0 ? 0 : chance / Math.Max(total, 100) * 100;

                response += chance <= 0
                    ? $"\n  ∟ {customRole} - <color=#ff0000>never spawns (spawn_chance is {chance})</color>"
                    : $"\n  ∟ {customRole} - <b>{actual:0.##}%</b>{(Math.Abs(actual - chance) > 0.01f ? $" (configured: {chance}%)" : string.Empty)}";
            }
        }

        List<ICustomRole> delayedRoles = CustomRole.List.Where(r =>
            !r.IgnoreSpawnSystem && r.SpawnSettings is { SpawnDelay: > 0 } &&
            r.SpawnSettings.CanReplaceRoles is { Count: > 0 }).ToList();
        if (delayedRoles.Any())
        {
            response += $"\n\n<color=#ffff00>⏱️</color> <b>Roles spawned on a timer</b> ({delayedRoles.Count}) - handed out after the round started, not at spawn:";
            foreach (ICustomRole customRole in delayedRoles)
                response += $"\n  ∟ {customRole} - after <b>{customRole.SpawnSettings.SpawnDelay}s</b>, {customRole.SpawnSettings.SpawnChance}% for each {string.Join("/", customRole.SpawnSettings.CanReplaceRoles)}, up to {customRole.SpawnSettings.MaxPlayers} player(s)";
        }

        List<ICustomRole> manualRoles = CustomRole.List.Where(r =>
            r.IgnoreSpawnSystem || r.SpawnSettings?.CanReplaceRoles == null ||
            !r.SpawnSettings.CanReplaceRoles.Any()).ToList();
        if (manualRoles.Any())
        {
            response += $"\n\n<color=#00ffff>ℹ️</color> <b>Roles that never spawn on their own</b> ({manualRoles.Count}) - spawned manually or by another plugin:";
            foreach (ICustomRole customRole in manualRoles)
            {
                response += customRole.IgnoreSpawnSystem
                    ? $"\n  ∟ {customRole} - ignore_spawn_system is enabled"
                    : $"\n  ∟ {customRole} - no can_replace_roles";
            }
        }

        response += "\n<size=1>OwO</size>"; // We want to render everything

        return true;
    }
}