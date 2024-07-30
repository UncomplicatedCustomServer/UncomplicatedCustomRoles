using HarmonyLib;
using PlayerStatsSystem;
using System.Collections.Generic;
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

            KeyValuePair<bool, HurtingEventArgs> Results = EventManager.InvokeEvent("HurtingPlayer", EventArgs);

            handler = Results.Value.DamageHandler;
            return Results.Key;
        }
    }
}
