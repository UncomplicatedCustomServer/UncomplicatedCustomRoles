using HarmonyLib;
using InventorySystem;
using PluginAPI.Core;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.HarmonyElements.Patch
{
#pragma warning disable IDE0051 // Method is not called
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.StaminaRegenMultiplier), MethodType.Getter)]
    internal class StaminaRegenHandler
    {
        private static void Postfix(Inventory __instance, ref float __result)
        {
            if (Player.TryGet(__instance._hub, out Player player) && player.TryGetSummonedInstance(out SummonedCustomRole customRole) && customRole.Role.Stamina is not null)
                __result *= customRole.Role.Stamina.RegenMultiplier;
        }
    }
}
