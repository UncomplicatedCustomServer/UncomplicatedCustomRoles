using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
#nullable enable
    public class SpawnBehaviour
    {
        // Spawn Behaviour for the plugin (global)
        public static bool DoHandleWave { get; internal set; } = true;

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

        public Vector3 SpawnPosition { get; set; } = new();

        public Vector3 SpawnOffset { get; set; } = new();

        public string? RequiredPermission { get; set; } = string.Empty;

        // You can use this class to handle the RespawnWave like to disable it
        public static void DisableSpawnWave()
        {
            DoHandleWave = false;
            Plugin.RoleSpawnQueue.Clear();
        }
    }
}
