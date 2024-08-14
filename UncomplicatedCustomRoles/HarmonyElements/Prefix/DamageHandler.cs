using HarmonyLib;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.Events;
using UncomplicatedCustomRoles.Events.Args;
using UncomplicatedCustomRoles.Events.Enums;

namespace UncomplicatedCustomRoles.HarmonyElements.Prefix
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.DealDamage))]
    internal class DamageHandler
    {
        static bool Prefix(PlayerStats __instance, ref DamageHandlerBase handler)
        {
            // Attacker
            AttackerDamageHandler attackerDamageHandler = handler as AttackerDamageHandler;
            HurtingEventArgs EventArgs = new(__instance._hub, attackerDamageHandler);

            EventManager.InvokeEventClear(EventName.PlayerHurting, EventArgs);

            handler = EventArgs.DamageHandler;
            return EventArgs.IsAllowed;
        }
    }
}
