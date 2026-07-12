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
using HarmonyLib;
using Mirror;
using PlayerRoles;
using Respawning.NamingRules;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.Patches;

internal static class UcrSpawnContext
{
    [ThreadStatic] private static int _depth;

    internal static bool Active => _depth > 0;

    internal static void Enter()
    {
        _depth++;
    }

    internal static void Exit()
    {
        if (_depth > 0)
            _depth--;
    }
}

[HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.InitializeNewRole))]
internal class SetRolePatch
{
    private static void Prefix(PlayerRoleManager __instance, RoleTypeId targetId, RoleChangeReason reason,
        RoleSpawnFlags spawnFlags = RoleSpawnFlags.All, NetworkReader data = null)
    {
        if (SummonedCustomRole.TryGet(__instance.Hub, out var role))
            role.Destroy();
    }

    private static void Postfix(PlayerRoleManager __instance, RoleChangeReason reason)
    {
        if (!UcrSpawnContext.Active || reason is not RoleChangeReason.Respawn)
            return;

        if (__instance.CurrentRole is HumanRole humanRole
            && NamingRulesManager.TryGetNamingRule(humanRole.Team, out _)
            && NamingRulesManager.GeneratedNames.TryGetValue(humanRole.Team, out var names)
            && names.Count > 0
            && humanRole.UnitNameId >= names.Count)
            humanRole.UnitNameId = (byte)(names.Count - 1);
    }
}