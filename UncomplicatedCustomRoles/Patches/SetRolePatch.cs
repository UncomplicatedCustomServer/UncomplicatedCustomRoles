/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using Mirror;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.Patches
{
#pragma warning disable IDE0060
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.InitializeNewRole))]
    internal class SetRolePatch
    {
        static void Prefix(PlayerRoleManager __instance, RoleTypeId targetId, RoleChangeReason reason, RoleSpawnFlags spawnFlags = RoleSpawnFlags.All, NetworkReader data = null)
        {
            if (SummonedCustomRole.TryGet(__instance.Hub, out SummonedCustomRole role))
                role.Destroy();
        }
    }
}
