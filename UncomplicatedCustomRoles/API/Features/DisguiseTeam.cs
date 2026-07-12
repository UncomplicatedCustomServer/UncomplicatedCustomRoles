/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Concurrent;
using PlayerRoles;
using UncomplicatedCustomRoles.Patches;

namespace UncomplicatedCustomRoles.API.Features;

public class DisguiseTeam
{
    /// <summary>
    ///     Maps a player id to the <see cref="Team" /> their real team is being faked as.
    /// </summary>
    public static readonly ConcurrentDictionary<int, Team> List = new();

    /// <summary>
    ///     Maps a player id to the overridden <see cref="PlayerRoleBase" /> used to trick the server into
    ///     treating the player as (not) an human.
    /// </summary>
    public static readonly ConcurrentDictionary<int, PlayerRoleBase> RoleBaseList = new();

    /// <summary>
    ///     Registers a faked <see cref="Team" /> together with the overridden <see cref="PlayerRoleBase" /> that is
    ///     exposed as the player's current role. The role base is always set alongside its team, so there is no
    ///     separate "role base only" entry point.
    /// </summary>
    /// <param name="playerId">The player id.</param>
    /// <param name="team">The team to fake.</param>
    /// <param name="roleBase">The role base to expose as the player's current role.</param>
    public static void Set(int playerId, Team team, PlayerRoleBase roleBase)
    {
        List[playerId] = team;
        RoleBaseList[playerId] = roleBase;
        TeamPatchManager.EnsurePatched();
    }

    /// <summary>
    ///     Removes every disguise data for the given player and, if no disguise is left, removes the team patches.
    /// </summary>
    /// <param name="playerId">The player id.</param>
    public static void Remove(int playerId)
    {
        List.TryRemove(playerId, out _);
        RoleBaseList.TryRemove(playerId, out _);

        if (List.IsEmpty)
            TeamPatchManager.EnsureUnpatched();
    }

    /// <summary>
    ///     Clears every disguise data and removes the team patches. Used during plugin (re)load.
    /// </summary>
    public static void Clear()
    {
        List.Clear();
        RoleBaseList.Clear();
        TeamPatchManager.EnsureUnpatched();
    }
}