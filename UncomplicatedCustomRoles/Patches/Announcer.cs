/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

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
        static bool Prefix(ReferenceHub scp, DamageHandlerBase hit)
        {
            if (scp.GetTeam() is Team.SCPs && SummonedCustomRole.TryGet(scp, out SummonedCustomRole role))
            {
                if (role.HasModule<SilentAnnouncer>())
                    return false;

                if (role.TryGetModule(out CustomScpAnnouncer announcer)) {
                    SpawnManager.HandleRecontainmentAnnoucement(hit, announcer);

                    return false;
                }
            }

            return true;
        }
    }
}
