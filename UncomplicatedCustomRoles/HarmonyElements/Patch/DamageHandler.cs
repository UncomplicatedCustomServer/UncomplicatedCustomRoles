using HarmonyLib;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.Events;
using UncomplicatedCustomRoles.Events.Args;

namespace UncomplicatedCustomRoles.HarmonyElements.Patch
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.DealDamage))]
    internal class DamageHandler
    {
        static bool Prefix(PlayerStats __instance, ref DamageHandlerBase handler)
        {
            // Attacker
            AttackerDamageHandler attackerDamageHandler = handler as AttackerDamageHandler;
            HurtingEventArgs EventArgs = new(__instance._hub, attackerDamageHandler);

            EventManager.InvokeEvent("HurtingPlayer", EventArgs);

            handler = EventArgs.DamageHandler;
            return EventArgs.IsAllowed;
        }
    }
}
