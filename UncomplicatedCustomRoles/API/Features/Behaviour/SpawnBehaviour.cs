/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
#nullable enable
    public class SpawnBehaviour
    {
        /// <summary>
        /// Gets or sets a <see cref="List{T}"/> of <see cref="RoleTypeId"/> that this role can override
        /// </summary>
        public List<RoleTypeId> CanReplaceRoles { get; set; } = new()
        {
            RoleTypeId.ClassD
        };

        /// <summary>
        /// Gets or sets the maximum number of the given <see cref="ICustomRole"/> can be alive at the same time
        /// </summary>
        public int MaxPlayers { get; set; } = 10;

        /// <summary>
        /// Gets or sets the minimum number of players that are required by the given <see cref="ICustomRole"/> to spawn
        /// </summary>
        public int MinPlayers { get; set; } = 1;

        /// <summary>
        /// Gets or sets the spawn chance of the role.<br></br>
        /// 0 is 0% and 100 is 100%
        /// </summary>
        public float SpawnChance { get; set; } = 60;

        /// <summary>
        /// Gets or sets the <see cref="SpawnType"/> of the role
        /// </summary>
        public SpawnType Spawn { get; set; } = SpawnType.RoomsSpawn;

        /// <summary>
        /// Gets or sets a <see cref="List{T}"/> of zones that will be evaluated as spawnpoints
        /// </summary>
        public List<ZoneType> SpawnZones { get; set; } = new();

        /// <summary>
        /// Gets or sets a <see cref="List{T}"/> of rooms that will be evaluated as spawnpoints
        /// </summary>
        public List<RoomType> SpawnRooms { get; set; } = new()
        {
            RoomType.LczClassDSpawn
        };

        /// <summary>
        /// Gets or sets a <see cref="List{T}"/> of SpawnPoints that will be evaluated as spawnpoints
        /// </summary>
        public List<string> SpawnPoints { get; set; } = new();

        /// <summary>
        /// Gets or sets the required Exiled.Permission to spawn as the given <see cref="ICustomRole"/>
        /// </summary>
        public string? RequiredPermission { get; set; } = string.Empty;
    }
}
