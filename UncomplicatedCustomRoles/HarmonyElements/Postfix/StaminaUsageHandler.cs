using HarmonyLib;
using InventorySystem;
using PluginAPI.Core;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.HarmonyElements.Postfix
{
#pragma warning disable IDE0051 // Method is not called
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.StaminaUsageMultiplier), MethodType.Getter)]
    internal class StaminaUsageHandler
    {
        static void Postfix(Inventory __instance, ref float __result)
        {
            if (Player.TryGet(__instance._hub, out Player player) && player.TryGetSummonedInstance(out SummonedCustomRole customRole))
            {
                if (customRole.Role.Stamina is not null)
                {
                    __result *= !customRole.Role.Stamina.Infinite ? customRole.Role.Stamina.UsageMultiplier : 0;
                    __result *= !customRole.Role.Stamina.Infinite ? 1 : 0;
                }
            }
        }
    }
}
