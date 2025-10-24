/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Reflection;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using PlayerRoles;
using PlayerRoles.Spectating;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations
{
#pragma warning disable CS8974 // Conversione del gruppo di metodi in un tipo non delegato
    internal static class RespawnTimer
    {
        const string RespawnTimerTextKey = "CUSTOM_ROLE";

        public static void Enable()
        {
            DynamicInvoke.GetMethod("RespawnTimer", "RespawnTimer.API.Placeholder.Register")?.Invoke(null, new object[]
            {
                RespawnTimerTextKey,
                GetPublicRoleName
            });

            LogManager.Debug("Compatibility loader for RespawnTimer: success");
        }

        public static string GetPublicCustomRoleName(ICustomRole role, Player watcherPlayer)
        {
            if (!Plugin.Instance.Config.HiddenRolesId.TryGetValue(role.Id, out HiddenRoleInformation information))
                return role.Name;


            if ((information.OnlyVisibleOnOverwatch && watcherPlayer.Role == RoleTypeId.Overwatch) || watcherPlayer.RemoteAdminAccess)
                return Plugin.Instance.Config.RespawnTimerContent.Replace("%customrole%", role.Name);

            return information.RoleNameWhenHidden;
        }

        public static string GetPublicRoleName(Player player)
        {
            if (player.RoleBase is not SpectatorRole spectator) return Plugin.Instance.Config.RespawnTimerContentEmpty;

            Player spectated = Player.Get(spectator.SyncedSpectatedNetId);

            if (spectated is null)
                return string.Empty;
            
            if (spectated.TryGetSummonedInstance(out SummonedCustomRole summoned))
                return GetPublicCustomRoleName(summoned.Role, player);
            
            return Plugin.Instance.Config.RespawnTimerContentEmpty;
        }
    }
}