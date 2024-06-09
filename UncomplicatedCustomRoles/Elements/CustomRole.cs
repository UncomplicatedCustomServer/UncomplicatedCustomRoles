using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Interfaces;
using UnityEngine;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.Elements
{
#nullable enable
    public class CustomRole : ICustomRole
    {
        public int Id { get; set; } = 1;

        public string Name { get; set; } = "Janitor";

        public string CustomInfo { get; set; } = "Clean the Light Containment Zone.";

        public string BadgeName { get; set; } = string.Empty;

        public string BadgeColor { get; set; } = string.Empty;

        public RoleTypeId Role { get; set; } = RoleTypeId.ClassD;

        public RoleTypeId RoleAppearance { get; set; } = RoleTypeId.ClassD;

        public float Health { get; set; } = 100f;

        public float MaxHealth { get; set; } = 100f;

        public float Ahp { get; set; } = 0f;

        public float HumeShield { get; set; } = 0f;

        public List<UCREffect>? Effects { get; set; } = new()
        {
            new()
        };

        public bool InfiniteStamina { get; set; } = false;

        public bool CanEscape { get; set; } = true;

        public string? RoleAfterEscape { get; set; } = null;

        public Vector3 Scale { get; set; } = new();

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

        public float DamageMultiplier { get; set; } = 1f;

        public SpawnBehaviour? SpawnSettings { get; set; } = new();

        public bool IgnoreSpawnSystem { get; set; } = false;
    }
}