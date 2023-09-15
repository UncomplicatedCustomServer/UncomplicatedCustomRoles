using Exiled.API.Enums;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UncomplicatedCustomRoles.Structures;

namespace UncomplicatedCustomRoles.Elements
{
    public class CustomRole : ICustomRole
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "Janitor";
        public SpawnCondition SpawnCondition { get; set; } = SpawnCondition.RoundStart;
        public int MaxPlayers { get; set; } = 5;
        public int SpawnChance { get; set; } = 60;
        public RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public List<RoleTypeId> CanReplaceRoles { get; set; } = new()
        {
            RoleTypeId.ClassD
        };
        public string SpawnBroadcast { get; set; } = "You are a <color=orange><b>Janitor</b></color>!\nClean the Light Containment Zone!";
        public ushort SpawnBroadcastDuration { get; set; } = 5;
        public List<ItemType> Inventory { get; set; } = new()
        {
            ItemType.Flashlight,
            ItemType.KeycardJanitor
        };
        public List<uint> CustomItemsInventory { get; set; } = new();
        public SpawnType Spawn { get; set; } = SpawnType.RoomsSpawn;
        public List<ZoneType> SpawnZones { get; set; } = new();
        public List<RoomType> SpawnRooms { get; set; } = new()
        {
            RoomType.LczToilets
        };
        public bool IgnoreSpawnSystem { get; set; } = false;
    }
}
