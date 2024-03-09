using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;

namespace UncomplicatedCustomRoles.Elements
{
#nullable enable
    internal class ExternalCustomRole : IExternalCustomRole
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "Janitor";
        public string CustomInfo { get; set; } = "Clean the Light Containment Zone.";
        public string DisplayNickname { get; set; } = "D-%name%";
        public int MaxPlayers { get; set; } = 5;
        public int MinPlayers { get; set; } = 1;
        public int SpawnChance { get; set; } = 60;
        public RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public RoleTypeId RoleAppearance { get; set; } = RoleTypeId.ClassD;
        public List<RoleTypeId> CanReplaceRoles { get; set; } = new()
        {
            RoleTypeId.ClassD
        };
        public float Health { get; set; } = 100f;
        public float MaxHealth { get; set; } = 100f;
        public float Ahp { get; set; } = 0f;
        public float HumeShield { get; set; } = 0f;
        public List<UCREffect>? Effects { get; set; } = new()
        {
            new()
        };
        public bool CanEscape { get; set; } = true;
        public RoleTypeId? RoleAfterEscape { get; set; } = null;
        public string Scale { get; set; } = "0, 0, 0";
        public string SpawnBroadcast { get; set; } = "You are a <color=orange><b>Janitor</b></color>!\nClean the Light Containment Zone!";
        public ushort SpawnBroadcastDuration { get; set; } = 5;
        public string SpawnHint { get; set; } = "This is an hint shown when you spawn as a Janitor!";
        public float SpawnHintDuration { get; set; } = 3;
        public List<ItemType> Inventory { get; set; } = new()
        {
            ItemType.Flashlight,
            ItemType.KeycardJanitor
        };
        public List<uint> CustomItemsInventory { get; set; } = new();
        public Dictionary<AmmoType, ushort> Ammo { get; set; } = new()
        {
            {AmmoType.Nato9, 5 }
        };
        public SpawnLocationType Spawn { get; set; } = SpawnLocationType.RoomsSpawn;
        public List<ZoneType> SpawnZones { get; set; } = new();
        public List<RoomType> SpawnRooms { get; set; } = new()
        {
            RoomType.LczToilets
        };
        public string SpawnPosition { get; set; } = "0, 0, 0";
        public string SpawnOffset { get; set; } = "0, 0, 0";
        public float DamageMultiplier { get; set; } = 1f;
        public string? RequiredPermission { get; set; } = null;
        public bool IgnoreSpawnSystem { get; set; } = false;
    }
}
