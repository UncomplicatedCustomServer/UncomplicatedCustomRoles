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
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Extensions;

public static class CustomRoleExtension
{
    /// <summary>
    ///     Spawn the given <see cref="Player" /> as this <see cref="ICustomRole" />.
    ///     Works both for roles with and without <see cref="ICustomRole.SpawnSettings" />.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="player"></param>
    /// <returns>The created <see cref="SummonedCustomRole" /> instance or <see cref="null" /> if the spawn failed.</returns>
    public static SummonedCustomRole Spawn(this ICustomRole role, Player player)
    {
        if (role is null || player is null)
            return null;

        return SummonedCustomRole.Summon(player, role);
    }

    /// <summary>
    ///     Remove this <see cref="ICustomRole" /> from the given <see cref="Player" /> if they are currently playing it.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="player"></param>
    /// <returns><see cref="true" /> if the player was playing this role, and it has been removed.</returns>
    public static bool RemoveFrom(this ICustomRole role, Player player)
    {
        if (role is null || !SummonedCustomRole.TryGet(player, out var summoned) || summoned.Role.Id != role.Id)
            return false;

        summoned.Destroy();
        return true;
    }

    /// <summary>
    ///     Gets every active <see cref="SummonedCustomRole" /> instance of this <see cref="ICustomRole" />.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public static List<SummonedCustomRole> GetSpawnedInstances(this ICustomRole role)
    {
        return role is null ? [] : SummonedCustomRole.Get(role);
    }

    /// <summary>
    ///     Gets the number of players currently playing this <see cref="ICustomRole" />.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public static int GetSpawnedCount(this ICustomRole role)
    {
        return role is null ? 0 : SummonedCustomRole.Count(role);
    }

    /// <summary>
    ///     Gets whether a <see cref="ICustomRole" /> with this role's Id is currently registered.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public static bool IsRegistered(this ICustomRole role)
    {
        return role is not null && CustomRole.IsRegistered(role.Id);
    }

    /// <summary>
    ///     Register this <see cref="ICustomRole" /> inside UCR.
    /// </summary>
    /// <param name="role"></param>
    /// <returns>The <see cref="LoadStatusType" /> result of the registration.</returns>
    public static LoadStatusType Register(this ICustomRole role)
    {
        return CustomRole.Register(role);
    }

    /// <summary>
    ///     Unregister this <see cref="ICustomRole" /> from UCR.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="removeFromPlayers">
    ///     If true every player currently playing this role will lose it
    /// </param>
    /// <returns><see cref="true" /> if the role was registered and has been removed.</returns>
    public static bool Unregister(this ICustomRole role, bool removeFromPlayers = false)
    {
        return CustomRole.Unregister(role, removeFromPlayers);
    }
}
