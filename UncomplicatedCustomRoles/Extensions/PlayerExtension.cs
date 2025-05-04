﻿/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

/*
 * > 05/06/2024 - A really really good day :)
*/

namespace UncomplicatedCustomRoles.Extensions
{
    public static class PlayerExtension
    {
        /// <summary>
        /// Check if a <see cref="Player"/> is currently a <see cref="ICustomRole"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <returns><see cref="true"/> if the player is a custom role.</returns>
        public static bool HasCustomRole(this Player player)
        {
            return SummonedCustomRole.TryGet(player, out _);
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> to a <see cref="Player"/> without a coroutine.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRoleSync(this Player player, ICustomRole role)
        {
            SpawnManager.ClearCustomTypes(player);
            SpawnManager.SummonCustomSubclass(player, role.Id, true);
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> (via it's Id) to a <see cref="Player"/> without a coroutine.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRoleSync(this Player player, int role)
        {
            SpawnManager.ClearCustomTypes(player);
            SpawnManager.SummonCustomSubclass(player, role, true);
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> (via it's Id) to a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRole(this Player player, int role)
        {
            SpawnManager.ClearCustomTypes(player);
            Timing.RunCoroutine(Events.EventHandler.DoSpawnPlayer(player, role));
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> to a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRole(this Player player, ICustomRole role)
        {
            SpawnManager.ClearCustomTypes(player);
            Timing.RunCoroutine(Events.EventHandler.DoSpawnPlayer(player, role.Id));
        }

        /// <summary>
        /// Set every attribute of a given <see cref="ICustomRole"/> to a <see cref="Player"/> without considering the <see cref="ICustomRole.SpawnSettings"/>.<br></br>
        /// Use this only at your own risk and only if you know what you are doing!
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        [Obsolete("You should not use this function unless you want to handle the role spawn by yourself!", false)]
        public static void SetCustomRoleAttributes(this Player player, ICustomRole role)
        {
            SpawnManager.ClearCustomTypes(player);
            SpawnManager.SummonSubclassApplier(player, role);
        }

        /// <summary>
        /// Try to get the current <see cref="SummonedCustomRole"/> of a <see cref="Player"/> if it's one.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        /// <returns>true if the player is currently <see cref="SummonedCustomRole"/></returns>
        public static bool TryGetSummonedInstance(this Player player, out SummonedCustomRole summonedInstance)
        {
            summonedInstance = GetSummonedInstance(player);
            return summonedInstance != null;
        }

        /// <summary>
        /// Get the current <see cref="SummonedCustomRole"/> of a <see cref="Player"/> if it's one.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>The current <see cref="SummonedCustomRole"/> if the player has one, otherwise <see cref="null"/></returns>
        public static SummonedCustomRole GetSummonedInstance(this Player player)
        {
            return SummonedCustomRole.Get(player);
        }

        /// <summary>
        /// Try to remove a <see cref="ICustomRole"/> from a <see cref="Player"/> if it has one.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="doResetRole">If true the role will be resetted => modified stats like health and other things will be lost</param>
        /// <returns>True if success</returns>
        public static bool TryRemoveCustomRole(this Player player, bool doResetRole = false)
        {
            if (SummonedCustomRole.TryGet(player, out SummonedCustomRole result))
            {
                RoleTypeId Role = result.Role.Role;
                result.Destroy();

                if (doResetRole)
                {
                    Vector3 OriginalPosition = player.Position;

                    player.Role.Set(Role, RoleSpawnFlags.AssignInventory);

                    player.Position = OriginalPosition;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Changes the CustomInfo of a <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="value"></param>
        public static void ApplyClearCustomInfo(this Player player, string value)
        {
            player.ReferenceHub.nicknameSync.Network_playerInfoToShow = string.IsNullOrEmpty(value) ? player.ReferenceHub.nicknameSync.Network_playerInfoToShow & ~PlayerInfoArea.CustomInfo : player.ReferenceHub.nicknameSync.Network_playerInfoToShow |= PlayerInfoArea.CustomInfo;
            player.ReferenceHub.nicknameSync.Network_customPlayerInfoString = ProcessCustomInfo(value);
        }

        /// <summary>
        /// Changes the CustomInfo of a <see cref="Player"/> overriding also the player Role
        /// </summary>
        /// <param name="player"></param>
        /// <param name="customInfo"></param>
        /// <param name="role"></param>
        public static void ApplyCustomInfoAndRoleName(this Player player, string customInfo, string role)
        {
            player.ReferenceHub.nicknameSync.Network_playerInfoToShow |= PlayerInfoArea.CustomInfo;
            player.ReferenceHub.nicknameSync.Network_playerInfoToShow &= ~PlayerInfoArea.Role; // Hide role
            player.ReferenceHub.nicknameSync.Network_playerInfoToShow &= ~PlayerInfoArea.Nickname; // Hide nickname

            if (role.Contains("</"))
                LogManager.Error($"Failed to apply CustomInfo with Role name at PlayerExtension::ApplyCustomInfoAndRoleName(%Player, string, string): role name can't contains any end tag like </color>, </b>, </size> etc...!\nCustomInfo won't be applied to player {player.Nickname} ({player.Id}) -- Found: {role}");

            if (customInfo.StartsWith("<"))
                LogManager.Error($"Failed to apply CustomInfo with Role name at PlayerExtension::ApplyCustomInfoAndRoleName(%Player, string, string): role custom_info can't contains any tag like </olor>, <b>, <size> etc...!\nCustomInfo won't be applied to player {player.Nickname} ({player.Id}) -- Found: {customInfo}");

            if (customInfo is null || customInfo.Length < 1)
            {
                LogManager.Silent("Applying only role name (order: NICK-ROLE)");
                player.ReferenceHub.nicknameSync.Network_customPlayerInfoString = $"{player.DisplayNickname}\n{role}";
            }
            else
            {
                LogManager.Silent("Applying role name and custom info (CI-NICK-ROLE)");
                player.ReferenceHub.nicknameSync.Network_customPlayerInfoString = $"{ProcessCustomInfo(customInfo)}\n{player.DisplayNickname}\n{role}";
            }
        }

        /// <summary>
        /// Changes in the given string [br] with the UNICODE escape char "\n"
        /// </summary>
        /// <param name="customInfo"></param>
        /// <returns></returns>
        private static string ProcessCustomInfo(string customInfo) => customInfo.Replace("[br]", "\n");
    }
}
