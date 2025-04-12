﻿/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
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
#pragma warning disable CS8974 // Conversione del gruppo di metodi in un tipo non delegato

    internal class RespawnTimer
    {
        // Get the IPlugin<IConfig> of the RespawnTimer plugin 
        public static readonly IPlugin<IConfig> RespawnTimerPlugin = Exiled.Loader.Loader.GetPlugin("RespawnTimer");

        // Check if the respawn timer plugin is with us
        public static readonly bool Allowed = RespawnTimerPlugin is not null;

        const string RespawnTimerTextKey = "CUSTOM_ROLE";

        internal static bool GetReplaceHelper(out Type TimerView)
        {
            if (!Allowed)
            {
                TimerView = null;
                return false;
            }

            TimerView = RespawnTimerPlugin.Assembly.GetType("RespawnTimer.API.Features.TimerView");
            if (TimerView is null)
            {
                LogManager.Debug("Compatibility loader for RespawnTimer failed: no class 'RespawnTimer.API.Features.TimerView' present!");
                return false;
            }

            return true;
        }

        public static void Enable()
        {
            if (!GetReplaceHelper(out Type TimerView))
                return;

            if (TimerView.GetMethod("AddReplaceHelper") is null)
                return;

            TimerView.GetMethod("AddReplaceHelper").Invoke(null, new object[]
            {
                RespawnTimerTextKey,
                GetPublicRoleName
            });

            LogManager.Debug("Compatibility loader for RespawnTimer: success");
        }

        public static void Disable()
        {
            // that mean Enable do not find the RespawnTimer plugin
            if (!GetReplaceHelper(out Type TimerView))
                return;

            if (TimerView.GetMethod("RemoveReplaceHelper") is null)
                return;

            TimerView.GetMethod("RemoveReplaceHelper").Invoke(null, new object[]
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