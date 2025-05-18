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
