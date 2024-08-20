using HarmonyLib;
using PlayerRoles;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
    internal class Announcer
    {
        static bool Prefix(ReferenceHub scp, DamageHandlerBase hit, NineTailedFoxAnnouncer __instance)
        {
            if (scp.GetTeam() is Team.SCPs && SummonedCustomRole.TryGet(scp, out SummonedCustomRole role) && role.GetModule(out CustomScpAnnouncer announcer))
            {
                announcer.Awake(hit);

                SpawnManager.HandleRecontainmentAnnoucement(announcer);

                return false;
            }

            return true;
        }
    }
}
