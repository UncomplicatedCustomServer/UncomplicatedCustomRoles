/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using InventorySystem.Configs;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class PlayerExtension
    {
        /// <summary>
        /// Check if a <see cref="Player"/> is currently a <see cref="ICustomRole"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <returns><see langword="true"/> if the player is a custom role.</returns>
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
            SpawnManager.SummonCustomSubclass(player, role.Id);
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> (via it's Id) to a <see cref="Player"/> without a coroutine.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRoleSync(this Player player, int role)
        {
            SpawnManager.ClearCustomTypes(player);
            SpawnManager.SummonCustomSubclass(player, role);
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
        /// Try to get the current <see cref="SummonedCustomRole"/> of a <see cref="Player"/> if it has one.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="summonedInstance"></param>
        /// <returns>true if the player is currently <see cref="SummonedCustomRole"/></returns>
        public static bool TryGetSummonedInstance(this Player player, out SummonedCustomRole summonedInstance)
        {
            summonedInstance = GetSummonedInstance(player);
            return summonedInstance != null;
        }

        /// <summary>
        /// Try to get the current <see cref="SummonedCustomRole"/> of a <see cref="ReferenceHub"/> if it has one.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="summonedInstance"></param>
        /// <returns>true if the player is currently <see cref="SummonedCustomRole"/></returns>
        public static bool TryGetSummonedInstance(this ReferenceHub player, out SummonedCustomRole summonedInstance)
        {
            summonedInstance = GetSummonedInstance(player);
            return summonedInstance != null;
        }

        /// <summary>
        /// Get the current <see cref="SummonedCustomRole"/> of a <see cref="Player"/> if it has one.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>The current <see cref="SummonedCustomRole"/> if the player has one, otherwise <see langword="null"/></returns>
        public static SummonedCustomRole GetSummonedInstance(this Player player) => SummonedCustomRole.Get(player);

        /// <summary>
        /// Get the current <see cref="SummonedCustomRole"/> of a <see cref="ReferenceHub"/> if it has one.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>The current <see cref="SummonedCustomRole"/> if the player has one, otherwise <see langword="null"/></returns>
        public static SummonedCustomRole GetSummonedInstance(this ReferenceHub player) => SummonedCustomRole.Get(player);

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
                RoleTypeId role = result.Role.Role;
                result.Destroy();

                if (doResetRole)
                {
                    Vector3 originalPosition = player.Position;

                    player.SetRole(role, RoleChangeReason.Destroyed, RoleSpawnFlags.AssignInventory);

                    player.Position = originalPosition;
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
            if (!NicknameSync.ValidateCustomInfo(value, out string error))
            {
                var accepted = string.Join(", ", SpawnManager.colorMap.Values.Select(h => $"<color={h}>{h}</color>"));
                LogManager.Error($"CustomInfo color tags is not correct. Setting CustomInfo to empty. Accepted: {accepted}\nError: {error}");
                value = string.Empty;
            }
            string nick = player.DisplayName.StripColorTags().Replace("<color=#855439>*</color>", "").Trim();
            player.InfoArea &= ~PlayerInfoArea.Nickname;
            player.InfoArea = string.IsNullOrEmpty(value)
                ? player.InfoArea & ~PlayerInfoArea.CustomInfo
                : player.InfoArea | PlayerInfoArea.CustomInfo;
            player.CustomInfo = string.IsNullOrEmpty(value) ? nick : $"{ProcessCustomInfo(value)}\n{nick}";
        }
        
        /// <summary>
        /// Changes the CustomInfo of a <see cref="Player"/> and overrides the player Role
        /// </summary>
        public static void ApplyCustomInfoAndRoleName(this Player player, ICustomRole role)
        {
            player.InfoArea |= PlayerInfoArea.CustomInfo;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.InfoArea &= ~PlayerInfoArea.Nickname;
        
            string customInfo = PlaceholderManager.ApplyPlaceholders(role.CustomInfo, player, role);
            string roleName = role.Name.StripColorTags(); // Strip tags from role name text
            string nick = player.DisplayName.StripColorTags().Replace("<color=#855439>*</color>", "").Trim();
            
            bool customInfoValid = NicknameSync.ValidateCustomInfo(customInfo, out string ciError);
            if (!customInfoValid)
            {
                var accepted = string.Join(", ", SpawnManager.colorMap.Values.Select(h => $"<color={h}>{h}</color>"));
                LogManager.Error($"CustomInfo color tags is not correct. Accepted: {accepted}\nRole: {role}.\nError: {ciError}");
                // If custominfo is wrong, show default info (nothing custom)
                player.InfoArea &= ~PlayerInfoArea.CustomInfo;
                player.InfoArea |= PlayerInfoArea.Role | PlayerInfoArea.Nickname;
                return;
            }
            
            // If role name contains any color tag but custom info has none, revert to default
            bool customInfoHasColor = customInfo?.IndexOf("<color=", StringComparison.OrdinalIgnoreCase) >= 0;
            bool roleNameHasColor = role.Name.IndexOf("<color=", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!customInfoHasColor && roleNameHasColor)
            {
                LogManager.Error("Role name color requires custom info color. Setting RoleName to default. Role: " + role);
                player.InfoArea &= ~PlayerInfoArea.CustomInfo;
                player.InfoArea |= PlayerInfoArea.Role | PlayerInfoArea.Nickname;
                ApplyClearCustomInfo(player, customInfo);
                return;
            }
        
            if (string.IsNullOrEmpty(customInfo))
            {
                LogManager.Silent("Applying only role name (NICK-ROLE)");
                if (!NicknameSync.ValidateCustomInfo(role.Name, out string errorRole))
                {
                    var accepted = string.Join(", ", SpawnManager.colorMap.Values.Select(h => $"<color={h}>{h}</color>"));
                    LogManager.Error($"RoleName color tags is not correct. Showing default PlayerInfo. Accepted: {accepted}\nRole: {role}.\nError: {errorRole}");
                    player.InfoArea &= ~PlayerInfoArea.CustomInfo;
                    player.InfoArea |= PlayerInfoArea.Role | PlayerInfoArea.Nickname;
                    return;
                }
                player.CustomInfo = $"{nick}\n{role.Name}"; // keep original roleName formatting if valid
            }
            else
            {
                LogManager.Silent("Applying role name and custom info (CI-NICK-ROLE)");
                player.CustomInfo = $"{ProcessCustomInfo(customInfo)}\n{nick}\n{roleName}"; // roleName stripped of color tags per requirement
            }
        }

        /// <summary>
        /// Changes in the given string [br] with the UNICODE escape char "\n"
        /// </summary>
        /// <param name="customInfo"></param>
        /// <returns></returns>
        private static string ProcessCustomInfo(string customInfo) => customInfo.Replace("[br]", "\n");

        // REF https://gitlab.com/exmod-team/EXILED/-/blob/master/EXILED/Exiled.API/Features/Player.cs?ref_type=heads#L2558
        internal static void SetCategoryLimit(this Player player, ItemCategory category, sbyte limit)
        {
            int index = InventoryLimits.StandardCategoryLimits.Where(x => x.Value >= 0).OrderBy(x => x.Key).ToList().FindIndex(x => x.Key == category);

            if (index is -1)
                return;

            MirrorExtensions.SendFakeSyncObject(player, ServerConfigSynchronizer.Singleton.netIdentity, typeof(ServerConfigSynchronizer), writer =>
            {
                writer.WriteULong(1ul);
                writer.WriteUInt(1);
                writer.WriteByte((byte)SyncList<sbyte>.Operation.OP_SET);
                writer.WriteInt(index);
                writer.WriteSByte(limit);
            });
        }

        // REF https://gitlab.com/exmod-team/EXILED/-/blob/master/EXILED/Exiled.API/Features/Player.cs?ref_type=heads#L2584
        internal static void ResetCategoryLimit(this Player player, ItemCategory category)
        {
            int index = InventoryLimits.StandardCategoryLimits.Where(x => x.Value >= 0).OrderBy(x => x.Key).ToList().FindIndex(x => x.Key == category);

            if (index is -1) 
                return;

            MirrorExtensions.SendFakeSyncObject(player, ServerConfigSynchronizer.Singleton.netIdentity, typeof(ServerConfigSynchronizer), writer =>
            {
                writer.WriteULong(1ul);
                writer.WriteUInt(1);
                writer.WriteByte((byte)SyncList<sbyte>.Operation.OP_SET);
                writer.WriteInt(index);
                writer.WriteSByte(ServerConfigSynchronizer.Singleton.CategoryLimits[index]);
            });
        }

        internal static void ResetInventory(this Player player, IEnumerable<ItemType> items)
        {
            if (items is null)
                return;

            player.ClearInventory();
            foreach (ItemType item in items)
                player.AddItem(item);
        }

        // REF https://gitlab.com/exmod-team/EXILED/-/blob/master/EXILED/Exiled.API/Features/Player.cs?ref_type=heads#L2458
        internal static ushort GetAmmoLimit(this Player player, ItemType type, bool ignoreArmor = false)
        {
            if (ignoreArmor)
                return ServerConfigSynchronizer.Singleton.AmmoLimitsSync.FirstOrDefault(x => x.AmmoType == type).Limit;

            return InventoryLimits.GetAmmoLimit(type, player.ReferenceHub);
        }

        // REF https://gitlab.com/exmod-team/EXILED/-/blob/master/EXILED/Exiled.API/Features/Player.cs?ref_type=heads#L2479
        internal static void SetAmmoLimit(this Player player, ItemType type, ushort limit)
        {
            int index = ServerConfigSynchronizer.Singleton.AmmoLimitsSync.FindIndex(x => x.AmmoType == type);
            MirrorExtensions.SendFakeSyncObject(player, ServerConfigSynchronizer.Singleton.netIdentity, typeof(ServerConfigSynchronizer), writer =>
            {
                writer.WriteULong(2ul);
                writer.WriteUInt(1);
                writer.WriteByte((byte)SyncList<ServerConfigSynchronizer.AmmoLimit>.Operation.OP_SET);
                writer.WriteInt(index);
                writer.WriteAmmoLimit(new() { Limit = limit, AmmoType = type, });
            });
        }

        // REF https://gitlab.com/exmod-team/EXILED/-/blob/master/EXILED/Exiled.API/Features/Player.cs?ref_type=heads#L2499
        internal static void ResetAmmoLimit(this Player player, ItemType type)
        {
            int index = ServerConfigSynchronizer.Singleton.AmmoLimitsSync.FindIndex(x => x.AmmoType == type);
            MirrorExtensions.SendFakeSyncObject(player, ServerConfigSynchronizer.Singleton.netIdentity, typeof(ServerConfigSynchronizer), writer =>
            {
                writer.WriteULong(2ul);
                writer.WriteUInt(1);
                writer.WriteByte((byte)SyncList<ServerConfigSynchronizer.AmmoLimit>.Operation.OP_SET);
                writer.WriteInt(index);
                writer.WriteAmmoLimit(ServerConfigSynchronizer.Singleton.AmmoLimitsSync[index]);
            });
        }

    }
}
