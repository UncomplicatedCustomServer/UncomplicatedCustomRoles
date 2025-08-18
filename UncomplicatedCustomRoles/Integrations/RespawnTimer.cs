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
using System.Reflection;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.API.Interfaces;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations
{
    internal class RespawnTimer
    {
        public static readonly IPlugin<IConfig> RespawnTimerPlugin = Exiled.Loader.Loader.GetPlugin("RespawnTimer");

        public static readonly Type TimerView = RespawnTimerPlugin?.Assembly.GetType("RespawnTimer.API.Features.TimerView");

        public static readonly MethodInfo AddReplaceHelper = TimerView?.GetMethod("AddReplaceHelper");

        public static readonly MethodInfo RemoveReplaceHelper = TimerView?.GetMethod("RemoveReplaceHelper");

        const string RespawnTimerTextKey = "CUSTOM_ROLE";

        public static void Enable()
        {
#pragma warning disable CS8974 // Conversione del gruppo di metodi in un tipo non delegato
            AddReplaceHelper?.Invoke(null, new object[]
            {
                RespawnTimerTextKey,
                GetPublicRoleName
            });
#pragma warning restore CS8974 // Conversione del gruppo di metodi in un tipo non delegato

            LogManager.Debug("Compatibility loader for RespawnTimer: success");
        }

        public static void Disable()
        {
            RemoveReplaceHelper?.Invoke(null, new object[]
            {
                RespawnTimerTextKey
            });
        }

        public static string GetPublicCustomRoleName(ICustomRole role, Player watcherPlayer)
        {
            if (!Plugin.Instance.Config.HiddenRolesId.TryGetValue(role.Id, out var information))
                return role.Name;

            if (information.OnlyVisibleOnOverwatch)
            {
                if (watcherPlayer.Role == RoleTypeId.Overwatch)
                {
                    return Plugin.Instance.Config.RespawnTimerContent.Replace("%customrole%", role.Name);
                }
            }
            else
            {
                if (watcherPlayer.RemoteAdminAccess)
                {
                    return Plugin.Instance.Config.RespawnTimerContent.Replace("%customrole%", role.Name);
                }
            }
            return information.RoleNameWhenHidden;
        }

        public static string GetPublicRoleName(Player player)
        {
            if (player.Role is not SpectatorRole spectator) return Plugin.Instance.Config.RespawnTimerContentEmpty;

            Player spectated = spectator.SpectatedPlayer;

            if (spectated is null)
                return string.Empty;
            
            if (spectated.TryGetSummonedInstance(out SummonedCustomRole summoned))
                return GetPublicCustomRoleName(summoned.Role, player);
            
            return Plugin.Instance.Config.RespawnTimerContentEmpty;
        }
    }
}