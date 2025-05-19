using MapGeneration;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.Compatibility.PreviousVersionElements
{
#nullable enable

    internal class CaparezzaSpawnBehaviour
    {
        public List<RoleTypeId> CanReplaceRoles { get; set; } = new()
        {
            RoleTypeId.ClassD
        };

        public int MaxPlayers { get; set; } = 10;

        public int MinPlayers { get; set; } = 1;

        public float SpawnChance { get; set; } = 60;

        public SpawnType Spawn { get; set; } = SpawnType.RoomsSpawn;

        public List<FacilityZone> SpawnZones { get; set; } = new();

        public List<string> SpawnRooms { get; set; } = new()
        {
            "LCZ_ClassDSpawn"
        };

        public List<RoleTypeId> SpawnRoles { get; set; } = new()
        {
            RoleTypeId.ClassD
        };

        public List<string> SpawnPoints { get; set; } = new();

        public PlayerPermissions[]? RequiredPermission { get; set; } = new PlayerPermissions[] { };
    }
}
