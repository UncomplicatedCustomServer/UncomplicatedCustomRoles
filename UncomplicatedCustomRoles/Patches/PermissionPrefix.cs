/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using HarmonyLib;
using System;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(Permissions), nameof(Permissions.CheckPermission), new Type[] { typeof(Player), typeof(string) })]
    public class PermissionPrefix
    {
        static bool Prefix(Player player, string permission, ref bool __result)
        {
            if (player.TryGetSummonedInstance(out SummonedCustomRole role) && role.TryGetModule(out CustomPermissions module) && module.Permissions.Contains(permission))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
