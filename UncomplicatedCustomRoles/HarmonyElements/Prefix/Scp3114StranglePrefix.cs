using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.HarmonyElements.Prefix
{
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ValidateTarget))]
    internal class Scp3114StranglePrefix
    {
        private static bool Prefix(ReferenceHub player, ref bool __result, Scp3114Strangle __instance)
        {
            if (player.roleManager.CurrentRole is null)
                return true;

            if (SummonedCustomRole.TryGet(player, out SummonedCustomRole playerRole) && playerRole.Role.IsFriendOf is not null && playerRole.Role.IsFriendOf.Contains(__instance.Owner.roleManager.CurrentRole.Team))
            {
                // Attacked player can't be strangled by SCP-3114 as it's his friend :)
                __result = false;
                return false; // Skip
            } else if (SummonedCustomRole.TryGet(__instance.Owner, out SummonedCustomRole scpRole) && scpRole.Role.IsFriendOf is not null && scpRole.Role.IsFriendOf.Contains(player.roleManager.CurrentRole.Team))
            {
                // Attacked player can't be strangled by SCP-3114 as it's his friend :)
                __result = false;
                return false; // Skip
            }

            return true;
        }
    }
}
