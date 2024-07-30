using HarmonyLib;
using PlayerStatsSystem;

namespace UncomplicatedCustomRoles.HarmonyElements.Patch
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.DealDamage))]
    internal class DamageHandler
    {
        static bool Prefix(PlayerStats __instance, DamageHandlerBase handler)
        {
            // Attacker
            if (handler is AttackerDamageHandler attackerDamageHandler)
            {
                attackerDamageHandler.Attacker.Hub;
            }

            // Sub
            __instance._hub
        }
    }
}
