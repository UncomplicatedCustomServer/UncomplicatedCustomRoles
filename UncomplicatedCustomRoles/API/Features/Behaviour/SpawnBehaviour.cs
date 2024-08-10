using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
#nullable enable
    public class SpawnBehaviour
    {
        // Spawn Behaviour for the roles (role-based)
        public List<RoleTypeId> CanReplaceRoles { get; set; } = new()
        {
            RoleTypeId.ClassD
        };

        public int MaxPlayers { get; set; } = 10;

        public int MinPlayers { get; set; } = 1;

        public int SpawnChance { get; set; } = 60;

        public SpawnLocationType Spawn { get; set; } = SpawnLocationType.RoomsSpawn;

        public List<ZoneType> SpawnZones { get; set; } = new();

        public List<RoomType> SpawnRooms { get; set; } = new()
        {
            RoomType.LczClassDSpawn
        };

        public string? SpawnPoint { get; set; } = null;

        public string? RequiredPermission { get; set; } = string.Empty;
    }
}
