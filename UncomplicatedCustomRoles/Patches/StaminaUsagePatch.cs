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
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.StaminaUsageMultiplier), MethodType.Getter)]
    public class StaminaUsagePatch
    {
        public static void Postfix(Inventory __instance, ref float __result)
        {
            if (!__instance._hub.TryGetSummonedInstance(out var role))
                return;
            __result *= role.Role.Stamina.Infinite ? 0 : role.Role.Stamina.UsageMultiplier;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.StaminaRegenMultiplier), MethodType.Getter)]
    public class StaminaRegenPatch
    {
        public static void Postfix(Inventory __instance, ref float __result)
        {
            if (!__instance._hub.TryGetSummonedInstance(out var role))
                return;
            __result *= role.Role.Stamina.RegenMultiplier;
        }
    }
}