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
using UncomplicatedCustomRoles.API.Features.CustomModules;
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
            Timing.RunCoroutine(SpawnManager.AsyncPlayerSpawner(player, role));
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> to a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRole(this Player player, ICustomRole role)
        {
            SpawnManager.ClearCustomTypes(player);
            Timing.RunCoroutine(SpawnManager.AsyncPlayerSpawner(player, role.Id));
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
        /// <returns>The current <see cref="SummonedCustomRole"/> if the player has one, otherwise <see cref="null"/></returns>
        public static SummonedCustomRole GetSummonedInstance(this Player player) => SummonedCustomRole.Get(player);

        /// <summary>
        /// Get the current <see cref="SummonedCustomRole"/> of a <see cref="ReferenceHub"/> if it has one.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
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
                RoleTypeId Role = result.Role.Role;
                result.Destroy();

                if (doResetRole)
                {
                    Vector3 OriginalPosition = player.Position;

                    player.SetRole(Role, RoleChangeReason.Destroyed, RoleSpawnFlags.AssignInventory);

                    player.Position = OriginalPosition;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Refresh the CustomInfo of a <see cref="Player"/> that has a <see cref="ICustomRole"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="customInfo"></param>
        public static void RefreshInfoArea(this Player player, string customInfo)
        {
            ICustomRole role = player.TryGetSummonedInstance(out var summonedCustomRole) ? summonedCustomRole.Role : null;
            if (role is null)
            {
                LogManager.Warn($"Tried to refresh InfoArea for player {player.Nickname} but they don't have a custom role.");
                return;
            }
            string formattedCustomInfo = ProcessCustomInfo(PlaceholderManager.ApplyPlaceholders(customInfo, player, role));
            string roleName = role.Name;
            string nickName = player.DisplayName.Replace("<color=#855439>*</color>", "");
            bool customInfoExists = !string.IsNullOrEmpty(formattedCustomInfo);
            bool roleNameExists = role.OverrideRoleName;
            
            player.InfoArea |= PlayerInfoArea.CustomInfo;
            player.InfoArea &= ~PlayerInfoArea.Role;
            player.InfoArea &= ~PlayerInfoArea.Nickname;
            player.InfoArea &= ~PlayerInfoArea.UnitName;
            
            if (!NicknameSync.ValidateCustomInfo(formattedCustomInfo, out string customInfoError) && customInfoExists)
            {
                LogManager.Error($"CustomInfo is not correct. Setting CustomInfo to empty.\nCustomInfo: {formattedCustomInfo}\nError: {customInfoError}");
                customInfoExists = false;
            }
            
            if (!NicknameSync.ValidateCustomInfo(roleName, out string roleNameError) && roleNameExists)
            {
                LogManager.Error($"RoleName is not correct. Setting CustomInfo to empty.\nRoleName: {roleName}\nError: {roleNameError}");
                roleNameExists = false;
            }
            
            player.CustomInfo = "<color=#FFFFFF></color>%custominfo%%nickname%%rolename%";
            
            if (summonedCustomRole.TryGetModule(out CustomInfoOrder customInfoOrderModule))
                player.CustomInfo = $"<color=#FFFFFF></color>{customInfoOrderModule.Order}";
            
            if (!customInfoExists)
                player.CustomInfo = player.CustomInfo.Replace("%custominfo%", "");

            if (summonedCustomRole.TryGetModule(out ColorfulNickname colorfulNickname))
            {
                if (string.IsNullOrEmpty(colorfulNickname.Color))
                    return;
                string nick = player.DisplayName.Replace("<color=#855439>*</color>", "");
                string color = colorfulNickname.Color.StartsWith("#") ? colorfulNickname.Color : $"#{colorfulNickname.Color}";
                if (!Misc.AcceptedColours.Contains(color.Replace("#", "")))
                {
                    LogManager.Warn($"The color {color} is not acceptable by the game in ColorfulNicknames! Please use a valid hex color code.");
                    return;
                }
                nickName = $"<color={color}>{nick}</color>";
            }
            
            player.CustomInfo = player.CustomInfo.Replace("%%", "%\n%").BulkReplace(new()
            {
                { "custominfo",  customInfoExists ? $"{formattedCustomInfo}" : "" },
                { "nickname", nickName },
                { "rolename", roleNameExists ? $"{roleName}" : role.Role.GetFullName() },
            }, "%<val>%");
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
