/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Text;
using Cassie;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerStatsSystem;
using Subtitles;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Patches
{
    internal static class Announcer
    {
        internal static readonly Dictionary<int, string> SavedCustomAnnouncements = new();
    }
    
    [HarmonyPatch(typeof(CassieScpTerminationAnnouncement), nameof(CassieScpTerminationAnnouncement.AnnounceScpTermination))]
    internal class AnnounceScpTerminationPatch
    {
        private static bool Prefix(ReferenceHub scp, DamageHandlerBase hit)
        {
            if (scp.GetTeam() is Team.SCPs && SummonedCustomRole.TryGet(scp, out SummonedCustomRole role))
            {
                if (role.HasModule<SilentAnnouncer>())
                    return false;

                if (!role.TryGetModule(out CustomScpAnnouncer announcer)) return true;
                SpawnManager.AnnounceScpTermination(scp, hit);
                Announcer.SavedCustomAnnouncements[scp.PlayerId] = announcer.RoleName;
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(CassieScpTerminationAnnouncement), nameof(CassieScpTerminationAnnouncement.OnStartedPlaying))]
    internal class OnStartedPlayingPatch
    {
        private static bool Prefix(CassieScpTerminationAnnouncement __instance)
        {
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
            List<SubtitlePart> subtitlePartList = new List<SubtitlePart>();
            for (int index = 0; index < __instance.Victims.Count; ++index)
            {
                string withoutSpace;
                string withSpace;
                if (Announcer.SavedCustomAnnouncements.TryGetValue(__instance.Victims[index].PlayerId, out string value))
                {
                    CassieScpTerminationAnnouncement.ConvertSCP(value, out withoutSpace, out withSpace);
                    Announcer.SavedCustomAnnouncements.Remove(__instance.Victims[index].PlayerId);
                } else 
                    CassieScpTerminationAnnouncement.ConvertSCP(__instance.Victims[index].Role, out withoutSpace, out withSpace);

                stringBuilder.Append(index == 0 ? "SCP " : ". SCP ");
                stringBuilder.Append(withSpace);
                subtitlePartList.Add(new SubtitlePart(SubtitleType.SCP, withoutSpace));
            }
            stringBuilder.Append(__instance._announcementTts);
            subtitlePartList.AddRange(__instance._subtitles);
            __instance.Payload = new CassieTtsPayload(StringBuilderPool.Shared.ToStringReturn(stringBuilder), subtitlePartList.ToArray());
            return false;
        }
    }
}
