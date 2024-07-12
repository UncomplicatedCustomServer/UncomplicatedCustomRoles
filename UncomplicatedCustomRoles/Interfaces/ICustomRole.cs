using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.Interfaces
{
#nullable enable
    public interface ICustomRole
    {
        public abstract int Id { get; set; }

        public abstract string Name { get; set; }

        public abstract string? Nickname { get; set; }

        public abstract string CustomInfo { get; set; }

        public abstract string BadgeName { get; set; }

        public abstract string BadgeColor { get; set; }

        public abstract RoleTypeId Role { get; set; }

        public abstract RoleTypeId RoleAppearance { get; set; }

        public abstract float Health { get; set; }

        public abstract float MaxHealth { get; set; }

        public abstract float Ahp { get; set; }

        public abstract float HumeShield { get; set; }

        public abstract List<UCREffect>? Effects { get; set; }

        public abstract bool InfiniteStamina { get; set; }

        public abstract bool CanEscape { get; set; }

        public abstract string? RoleAfterEscape { get; set; }

        public abstract Vector3 Scale { get; set; }

        public abstract string SpawnBroadcast { get; set; }

        public abstract ushort SpawnBroadcastDuration { get; set; }

        public abstract string SpawnHint { get; set; }

        public abstract float SpawnHintDuration { get; set; }

        public abstract List<ItemType> Inventory { get; set; }

        public abstract List<uint> CustomItemsInventory { get; set; }

        public abstract Dictionary<AmmoType, ushort> Ammo { get; set; }

        public abstract float DamageMultiplier { get; set; }

        public abstract SpawnBehaviour? SpawnSettings { get; set; }

        public abstract bool IgnoreSpawnSystem { get; set; }
    }
}