/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.Compatibility.PreviousVersionElements.Enums;

namespace UncomplicatedCustomRoles.Compatibility.PreviousVersionElements
{
#nullable enable
    internal class BonolisSpawnBehaviour
    {
        public List<RoleTypeId> CanReplaceRoles { get; set; } = new()
        {
            RoleTypeId.ClassD
        };

        public int MaxPlayers { get; set; } = 10;

        public int MinPlayers { get; set; } = 1;

        public float SpawnChance { get; set; } = 60;

        public SpawnType Spawn { get; set; } = SpawnType.RoomsSpawn;

        public List<ExiledZoneType> SpawnZones { get; set; } = new();

        public List<ExiledRoomType> SpawnRooms { get; set; } = new()
        {
            ExiledRoomType.LczClassDSpawn
        };

        public List<RoleTypeId> SpawnRoles { get; set; } = new()
        {
            RoleTypeId.ClassD
        };

        public List<string> SpawnPoints { get; set; } = new();

        public string? RequiredPermission { get; set; } = string.Empty;
    }
}
