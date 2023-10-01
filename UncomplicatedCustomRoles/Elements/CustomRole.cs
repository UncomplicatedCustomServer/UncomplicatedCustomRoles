using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;
using UnityEngine;

namespace UncomplicatedCustomRoles.Elements
{
    public class CustomRole : ICustomRole
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "Janitor";
        public string CustomInfo { get; set; } = "Clean the Light Containment Zone.";
        public SpawnCondition SpawnCondition { get; set; } = SpawnCondition.RoundStart;
        public int MaxPlayers { get; set; } = 5;
        public int MinPlayers { get; set; } = 0;
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
        public List<UCREffect> Effects { get; set; } = new()
        {
            new UCREffect()
        };
        public Vector3 Scale { get; set; } = new();
        public string SpawnBroadcast { get; set; } = "You are a <color=orange><b>Janitor</b></color>!\nClean the Light Containment Zone!";
        public ushort SpawnBroadcastDuration { get; set; } = 5;
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
        public Vector3 SpawnPosition { get; set; } = new();
        public bool IgnoreSpawnSystem { get; set; } = false;
    }
}