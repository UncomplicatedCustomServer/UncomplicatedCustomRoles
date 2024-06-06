using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.API.Interfaces;
using PlayerRoles;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Interfaces;

namespace UncomplicatedCustomRoles.Manager
{
#pragma warning disable CS8974 // Conversione del gruppo di metodi in un tipo non delegato

    internal class RespawnTimerCompatibility
    {
        // Get the IPlugin<IConfig> of the RespawnTimer plugin 
        public static readonly IPlugin<IConfig> RespawnTimer = Exiled.Loader.Loader.GetPlugin("RespawnTimer");

        // Check if the respawn timer plugin is with us
        public static readonly bool Allowed = RespawnTimer is not null;

        const string RespawnTimerTextKey = "CUSTOM_ROLE";

        internal static bool GetReplaceHelper(out Type TimerView)
        {
            TimerView = RespawnTimer.Assembly.GetType("RespawnTimer.API.Features.TimerView");
            if (TimerView is null)
            {
                Log.Debug("Compatibility loader for RespawnTimer failed: no class 'RespawnTimer.API.Features.TimerView' present!");
                return false;
            }

            if (TimerView.GetProperty("ReplaceHelper") is null)
                return false;

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

            Log.Debug("Compatibility loader for RespawnTimer: success");
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
            
            if (spectated.TryGetCustomRole(out var customRole))
                return GetPublicCustomRoleName(customRole, player);
            
            return Plugin.Instance.Config.RespawnTimerContentEmpty;
        }
    }
}