using HarmonyLib;
using PlayerRoles;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.HarmonyElements.Prefix
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
    internal class Announcer
    {
        static bool Prefix(ReferenceHub scp, DamageHandlerBase hit)
        {
            if (scp.GetTeam() is Team.SCPs && SummonedCustomRole.TryGet(scp, out SummonedCustomRole role))
            {
                if (role.HasModule<SilentAnnouncer>())
                    return false;

                if (role.GetModule(out CustomScpAnnouncer announcer))
                {
                    announcer.Awake(hit);

                    SpawnManager.HandleRecontainmentAnnoucement(announcer);

                    return false;
                }
            }

            return true;
        }
    }
}