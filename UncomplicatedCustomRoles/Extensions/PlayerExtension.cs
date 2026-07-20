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
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Configs;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Extensions;

public static class PlayerExtension
{
    /// <summary>
    ///     Check if a <see cref="Player" /> is currently a <see cref="ICustomRole" />.
    /// </summary>
    /// <param name="player"></param>
    /// <returns><see cref="true" /> if the player is a custom role.</returns>
    public static bool HasCustomRole(this Player player)
    {
        return SummonedCustomRole.TryGet(player, out _);
    }

    /// <summary>
    ///     Check if a <see cref="Player" /> is currently playing the <see cref="ICustomRole" /> with the given Id.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="id"></param>
    /// <returns><see cref="true" /> if the player is playing the given custom role.</returns>
    public static bool HasCustomRole(this Player player, int id)
    {
        return SummonedCustomRole.TryGet(player, out var summoned) && summoned.Role.Id == id;
    }

    /// <summary>
    ///     Get the <see cref="ICustomRole" /> definition the <see cref="Player" /> is currently playing.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>The <see cref="ICustomRole" /> or <see cref="null" /> if the player has no custom role.</returns>
    public static ICustomRole GetCustomRole(this Player player)
    {
        return SummonedCustomRole.Get(player)?.Role;
    }

    /// <summary>
    ///     Try to get the <see cref="ICustomRole" /> definition the <see cref="Player" /> is currently playing.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="role"></param>
    /// <returns><see cref="true" /> if the player has a custom role.</returns>
    public static bool TryGetCustomRole(this Player player, out ICustomRole role)
    {
        role = player.GetCustomRole();
        return role is not null;
    }

    internal static void ForceApplyEffect(this ReferenceHub hub, string effectName, byte intensity, float duration,
        bool addDuration = false)
    {
        if (hub is null || !hub.playerEffectsController.TryGetEffect(effectName, out var effect))
            return;

        effect.ForceIntensity(intensity);
        effect.ServerChangeDuration(duration, addDuration);
    }

    /// <summary>
    ///     Set a <see cref="ICustomRole" /> to a <see cref="Player" /> without a coroutine.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="role"></param>
    /// <returns>The created <see cref="SummonedCustomRole" /> instance or <see cref="null" /> if the spawn failed.</returns>
    public static SummonedCustomRole SetCustomRoleSync(this Player player, ICustomRole role)
    {
        if (role is null)
            return null;

        SpawnManager.ClearCustomTypes(player);
        return SummonedCustomRole.Summon(player, role);
    }

    /// <summary>
    ///     Set a <see cref="ICustomRole" /> (via it's Id) to a <see cref="Player" /> without a coroutine.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="role"></param>
    /// <returns>The created <see cref="SummonedCustomRole" /> instance or <see cref="null" /> if the spawn failed.</returns>
    public static SummonedCustomRole SetCustomRoleSync(this Player player, int role)
    {
        return CustomRole.TryGet(role, out var customRole) ? player.SetCustomRoleSync(customRole) : null;
    }

    /// <summary>
    ///     Set a <see cref="ICustomRole" /> (via it's Id) to a <see cref="Player" />.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="role"></param>
    public static void SetCustomRole(this Player player, int role)
    {
        SpawnManager.ClearCustomTypes(player);
        Timing.RunCoroutine(SpawnManager.AsyncPlayerSpawner(player, role));
    }

    /// <summary>
    ///     Set a <see cref="ICustomRole" /> to a <see cref="Player" />.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="role"></param>
    public static void SetCustomRole(this Player player, ICustomRole role)
    {
        SpawnManager.ClearCustomTypes(player);
        Timing.RunCoroutine(SpawnManager.AsyncPlayerSpawner(player, role.Id));
    }

    /// <summary>
    ///     Set every attribute of a given <see cref="ICustomRole" /> to a <see cref="Player" /> without considering the
    ///     <see cref="ICustomRole.SpawnSettings" />.<br></br>
    ///     Use this only at your own risk and only if you know what you are doing!
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
    ///     Try to get the current <see cref="SummonedCustomRole" /> of a <see cref="Player" /> if it has one.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>true if the player is currently <see cref="SummonedCustomRole" /></returns>
    public static bool TryGetSummonedInstance(this Player player, out SummonedCustomRole summonedInstance)
    {
        summonedInstance = player.GetSummonedInstance();
        return summonedInstance != null;
    }

    /// <summary>
    ///     Try to get the current <see cref="SummonedCustomRole" /> of a <see cref="ReferenceHub" /> if it has one.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>true if the player is currently <see cref="SummonedCustomRole" /></returns>
    public static bool TryGetSummonedInstance(this ReferenceHub player, out SummonedCustomRole summonedInstance)
    {
        summonedInstance = player.GetSummonedInstance();
        return summonedInstance != null;
    }

    /// <summary>
    ///     Get the current <see cref="SummonedCustomRole" /> of a <see cref="Player" /> if it has one.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>The current <see cref="SummonedCustomRole" /> if the player has one, otherwise <see cref="null" /></returns>
    public static SummonedCustomRole GetSummonedInstance(this Player player)
    {
        return SummonedCustomRole.Get(player);
    }

    /// <summary>
    ///     Get the current <see cref="SummonedCustomRole" /> of a <see cref="ReferenceHub" /> if it has one.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static SummonedCustomRole GetSummonedInstance(this ReferenceHub player)
    {
        return SummonedCustomRole.Get(player);
    }

    /// <summary>
    ///     Try to remove a <see cref="ICustomRole" /> from a <see cref="Player" /> if it has one.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="doResetRole">If true the role will be resetted => modified stats like health and other things will be lost</param>
    /// <returns>True if success</returns>
    public static bool TryRemoveCustomRole(this Player player, bool doResetRole = false)
    {
        if (SummonedCustomRole.TryGet(player, out var result))
        {
            var Role = result.Role.Role;
            result.Destroy();

            if (doResetRole)
            {
                var OriginalPosition = player.Position;

                player.SetRole(Role, RoleChangeReason.Destroyed, RoleSpawnFlags.AssignInventory);

                player.Position = OriginalPosition;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Refresh the CustomInfo of a <see cref="Player" /> that has a <see cref="ICustomRole" />.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="customInfo"></param>
    [Obsolete("This method is now obsolete, use the CustomInfo class instead!", true)]
    public static void RefreshInfoArea(this Player player, string customInfo)
    {
        _ = new CustomInfo(player, ProcessCustomInfo(customInfo));
    }

    /// <summary>
    ///     Changes in the given string [br] with the UNICODE escape char "\n"
    /// </summary>
    /// <param name="customInfo"></param>
    /// <returns></returns>
    private static string ProcessCustomInfo(string customInfo)
    {
        return customInfo.Replace("[br]", "\n");
    }

    // REF https://gitlab.com/exmod-team/EXILED/-/blob/master/EXILED/Exiled.API/Features/Player.cs?ref_type=heads#L2558
    internal static void SetCategoryLimit(this Player player, ItemCategory category, sbyte limit)
    {
        InventoryLimitOverride.Set(player.PlayerId, category, limit);
        SendCategoryLimit(player, category, limit);
    }

    // REF https://gitlab.com/exmod-team/EXILED/-/blob/master/EXILED/Exiled.API/Features/Player.cs?ref_type=heads#L2584
    internal static void ResetCategoryLimit(this Player player, ItemCategory category)
    {
        InventoryLimitOverride.Clear(player.PlayerId, category);

        var config = ServerConfigSynchronizer.Singleton;
        var index = (int)category;
        if (config is null || index < 0 || index >= config.CategoryLimits.Count)
            return;

        SendCategoryLimit(player, category, config.CategoryLimits[index]);
    }

    private static void SendCategoryLimit(Player player, ItemCategory category, sbyte limit)
    {
        var config = ServerConfigSynchronizer.Singleton;
        var index = (int)category;
        if (config is null || index < 0 || index >= config.CategoryLimits.Count)
            return;

        MirrorExtensions.SendFakeSyncObject(player, config.netIdentity, typeof(ServerConfigSynchronizer), writer =>
        {
            writer.WriteULong(1ul);
            writer.WriteUInt(1);
            writer.WriteByte((byte)SyncList<sbyte>.Operation.OP_SET);
            writer.WriteUInt((uint)index);
            writer.WriteSByte(limit);
        });
    }

    internal static void ResetInventory(this Player player, IEnumerable<ItemType> items)
    {
        if (items is null)
            return;

        player.ClearInventory();
        foreach (var item in items)
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
        var index = ServerConfigSynchronizer.Singleton.AmmoLimitsSync.FindIndex(x => x.AmmoType == type);
        MirrorExtensions.SendFakeSyncObject(player, ServerConfigSynchronizer.Singleton.netIdentity,
            typeof(ServerConfigSynchronizer), writer =>
            {
                writer.WriteULong(2ul);
                writer.WriteUInt(1);
                writer.WriteByte((byte)SyncList<ServerConfigSynchronizer.AmmoLimit>.Operation.OP_SET);
                writer.WriteInt(index);
                writer.WriteAmmoLimit(new ServerConfigSynchronizer.AmmoLimit { Limit = limit, AmmoType = type });
            });
    }

    // REF https://gitlab.com/exmod-team/EXILED/-/blob/master/EXILED/Exiled.API/Features/Player.cs?ref_type=heads#L2499
    internal static void ResetAmmoLimit(this Player player, ItemType type)
    {
        var index = ServerConfigSynchronizer.Singleton.AmmoLimitsSync.FindIndex(x => x.AmmoType == type);
        MirrorExtensions.SendFakeSyncObject(player, ServerConfigSynchronizer.Singleton.netIdentity,
            typeof(ServerConfigSynchronizer), writer =>
            {
                writer.WriteULong(2ul);
                writer.WriteUInt(1);
                writer.WriteByte((byte)SyncList<ServerConfigSynchronizer.AmmoLimit>.Operation.OP_SET);
                writer.WriteInt(index);
                writer.WriteAmmoLimit(ServerConfigSynchronizer.Singleton.AmmoLimitsSync[index]);
            });
    }
}