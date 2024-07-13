using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Interfaces;
using UnityEngine;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;

namespace UncomplicatedCustomRoles.Elements
{
#nullable enable
    public class CustomRole : ICustomRole
    {
        public int Id { get; set; } = 1;

        public string Name { get; set; } = "Janitor";

        public string? Nickname { get; set; } = "D-%dnumber%";

        public string CustomInfo { get; set; } = "Janitor";

        public string BadgeName { get; set; } = "Janitor";

        public string BadgeColor { get; set; } = "pumpkin";

        public RoleTypeId Role { get; set; } = RoleTypeId.ClassD;

        public RoleTypeId RoleAppearance { get; set; } = RoleTypeId.ClassD;

        public Team? IsFriendOf { get; set; } = null;

        public HealthBehaviour Health { get; set; } = new();

        public AhpBehaviour Ahp { get; set; } = new();

        public List<UCREffect>? Effects { get; set; } = new();

        public StaminaBehaviour Stamina { get; set; } = new();

        public int MaxScp330Candies { get; set; } = 2;

        public bool CanEscape { get; set; } = true;

        public string? RoleAfterEscape { get; set; } = "IR:Spectator,IR:NtfCaptain";

        public Vector3 Scale { get; set; } = Vector3.one;

        public string SpawnBroadcast { get; set; } = "You are a <color=orange><b>Janitor</b></color>!\nClean the Light Containment Zone!";

        public ushort SpawnBroadcastDuration { get; set; } = 5;

        public string SpawnHint { get; set; } = "This hint will be shown when you'll spawn as a Janitor!";

        public float SpawnHintDuration { get; set; } = 5;

        public List<ItemType> Inventory { get; set; } = new()
        {
            ItemType.Flashlight,
            ItemType.KeycardJanitor
        };

        public List<uint> CustomItemsInventory { get; set; } = new();

        public Dictionary<AmmoType, ushort> Ammo { get; set; } = new()
        {
            {
                AmmoType.Nato9,
                10
            }
        };

        public float DamageMultiplier { get; set; } = 1;

        public MovementBehaviour? Movement { get; set; } = new();

        public SpawnBehaviour? SpawnSettings { get; set; } = new();

        public bool IgnoreSpawnSystem { get; set; } = false;

        public bool HasTeam(Team team)
        {
            if (IsFriendOf is not null)
                return (IsFriendOf & team) == team;

            return false;
        }
    }
}