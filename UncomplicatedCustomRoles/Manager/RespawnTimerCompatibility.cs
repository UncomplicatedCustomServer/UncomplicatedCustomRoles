using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using HarmonyLib;
using PlayerRoles;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Interfaces;

namespace UncomplicatedCustomRoles.Manager
{
    internal static class RespawnTimerCompatibility
    {
        const string RespawnTimerTextKey = "role";
        const string DefaultValueWhenNoRole = "...";
        const string ReplaceHelperFullName = "RespawnTimer.API.Features.TimerView:ReplaceHelper";

        // Can be null if not using the RespawnTimer compatiblity or if the RespawnTimer is not loaded
        private static Dictionary<string, Func<Player, string>> ReplaceHelperRespawTimer = null;

        public static void Enable()
        {
            var propertyReplaceHelperRespawTimer = AccessTools.PropertyGetter(ReplaceHelperFullName);
            if (propertyReplaceHelperRespawTimer == null)
            {
                Log.Debug("hook to RespawnTimer, RespawnTimer.API.Features.TimerView.ReplaceHelper not found.");
                return;
            }
            
            ReplaceHelperRespawTimer = propertyReplaceHelperRespawTimer.Invoke(null, new object[0]) as Dictionary<string, Func<Player, string>>;
            if (ReplaceHelperRespawTimer == null)
            {
                Log.Debug("hook to RespawnTimer, faild to get the dictionary.");
                return;
            }

            ReplaceHelperRespawTimer.Add(RespawnTimerTextKey, GetPublicRoleName);
            Log.Debug("hook to RespawnTimer, succes.");
        }

        public static void Disable()
        {
            // that mean Enable do not find the RespawnTimer plugin
            if (ReplaceHelperRespawTimer == null)
                return;

            ReplaceHelperRespawTimer.Remove(RespawnTimerTextKey);
        }

        public static string GetPublicCustomRoleName(ICustomRole role, Player watcherPlayer)
        {
            if (!Plugin.Instance.Config.HiddenRolesId.TryGetValue(role.Id, out var information))
                return role.Name;

            if (information.OnlyVisibleOnOverwatch)
            {
                if (watcherPlayer.Role == RoleTypeId.Overwatch)
                {
                    return role.Name;
                }
            }
            else
            {
                if (watcherPlayer.RemoteAdminAccess)
                {
                    return role.Name;
                }
            }
            return information.RoleNameWhenHidden;
        }

        public static string GetPublicRoleName(Player player)
        {
            if (player.Role is not SpectatorRole spectator) return DefaultValueWhenNoRole;

            var spectated = spectator.SpectatedPlayer;

            if (spectated == null)
                return DefaultValueWhenNoRole;
            
            if (spectated.TryGetCustomRole(out var customRole))
                return GetPublicCustomRoleName(customRole, player);
            
            return spectated.Role.Name;
        }
    }
}