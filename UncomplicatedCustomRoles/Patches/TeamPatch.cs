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
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.CurrentRole), MethodType.Getter)]
    internal class PlayerRoleManagerPatch
    {
        static bool Prefix(PlayerRoleManager __instance, ref PlayerRoleBase __result)
        {
            if (__instance.Hub?.netId is 0)
                return true;

            if (__instance.Hub is not null && DisguiseTeam.RoleBaseList.TryGetValue(__instance.Hub.PlayerId, out PlayerRoleBase role))
            {
                if (role is null)
                    LogManager.Error($"[UCR] Disguised role for player {__instance.Hub.PlayerId} is null!");

                __result = role;

                return false;
            }

            return true;
        }
    }
}