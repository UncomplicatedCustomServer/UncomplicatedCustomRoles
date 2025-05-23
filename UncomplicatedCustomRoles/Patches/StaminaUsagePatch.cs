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
using InventorySystem;
using PlayerRoles.FirstPersonControl;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(Inventory), "get_StaminaUsageMultiplier")]
    public class StaminaUsagePatch
    {
        public static bool Prefix(FpcStateProcessor __instance, ref float __result)
        {
            if (__instance.Hub.TryGetSummonedInstance(out SummonedCustomRole role) && role.Role.Stamina is StaminaBehaviour stamina)
            {
                __result *= stamina.Infinite ? 0 : stamina.UsageMultiplier;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory), "get_StaminaRegenMultiplier")]
    public class StaminaRegenPatch
    {
        public static bool Prefix(FpcStateProcessor __instance, ref float __result)
        {
            if (__instance.Hub.TryGetSummonedInstance(out SummonedCustomRole role) && role.Role.Stamina is StaminaBehaviour stamina)
            {
                __result *= stamina.RegenMultiplier;
                return false;
            }

            return true;
        }
    }
}
