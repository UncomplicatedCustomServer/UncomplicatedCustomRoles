using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Interfaces
{
#nullable enable
    public interface ICustomRole
    {
        public abstract int Id { get; set; }

        public abstract string Name { get; set; }

        public abstract bool OverrideRoleName { get; set; }

        public abstract string? Nickname { get; set; }

        public abstract string CustomInfo { get; set; }

        public abstract string BadgeName { get; set; }

        public abstract string BadgeColor { get; set; }

        public abstract RoleTypeId Role { get; set; }

        public abstract Team? Team { get; set; }

        public abstract RoleTypeId RoleAppearance { get; set; }

        public abstract List<Team> IsFriendOf { get; set; }

        public abstract HealthBehaviour Health { get; set; }

        public abstract AhpBehaviour Ahp { get; set; }

        public abstract List<Effect>? Effects { get; set; }

        public abstract StaminaBehaviour Stamina { get; set; }

        public abstract int MaxScp330Candies { get; set; }

        public abstract bool CanEscape { get; set; }

        public abstract Dictionary<string, string> RoleAfterEscape { get; set; }

        public abstract Vector3 Scale { get; set; }

        public abstract string SpawnBroadcast { get; set; }

        public abstract ushort SpawnBroadcastDuration { get; set; }

        public abstract string SpawnHint { get; set; }

        public abstract float SpawnHintDuration { get; set; }

        public abstract Dictionary<ItemCategory, sbyte> CustomInventoryLimits { get; set; }

        public abstract List<ItemType> Inventory { get; set; }

        public abstract List<uint> CustomItemsInventory { get; set; }

        public abstract Dictionary<AmmoType, ushort> Ammo { get; set; }

        public abstract float DamageMultiplier { get; set; }

        public abstract SpawnBehaviour? SpawnSettings { get; set; }

        public abstract List<object>? CustomFlags { get; set; }

        public abstract bool IgnoreSpawnSystem { get; set; }
    }
}