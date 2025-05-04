/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.Compatibility.PreviousVersionElements;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.Compatibility.PreviousVersionRoles
{
#nullable enable

    /// <summary>
    /// Custom Role of the version v6.0.0 "Fossuon"
    /// </summary>
    public class FossuonCustomRole : IPreviousVersionRole
    {
        public virtual int Id { get; set; } = 1;

        public virtual string Name { get; set; } = "Janitor";

        public virtual bool OverrideRoleName { get; set; } = false;

        public virtual string? Nickname { get; set; } = "D-%dnumber%";

        public virtual string CustomInfo { get; set; } = "Janitor";

        public virtual string BadgeName { get; set; } = "Janitor";

        public virtual string BadgeColor { get; set; } = "pumpkin";

        public virtual RoleTypeId Role { get; set; } = RoleTypeId.ClassD;

        public virtual Team? Team { get; set; } = null;

        public virtual RoleTypeId RoleAppearance { get; set; } = RoleTypeId.ClassD;

        public virtual List<Team> IsFriendOf { get; set; } = new();

        public virtual FossuonHealthBehaviour Health { get; set; } = new();

        public virtual AhpBehaviour Ahp { get; set; } = new();

        public virtual List<Effect>? Effects { get; set; } = new();

        public virtual StaminaBehaviour Stamina { get; set; } = new();

        public virtual int MaxScp330Candies { get; set; } = 2;

        public virtual bool CanEscape { get; set; } = true;

        public virtual Dictionary<string, string> RoleAfterEscape { get; set; } = new()
        {
            {
                "default",
                "InternalRole Spectator"
            },
            {
                "cuffed by InternalTeam ChaosInsurgency",
                "InternalRole ClassD"
            }
        };

        public virtual Vector3 Scale { get; set; } = Vector3.one;

        public virtual string SpawnBroadcast { get; set; } = "You are a <color=orange><b>Janitor</b></color>!\nClean the Light Containment Zone!";

        public virtual ushort SpawnBroadcastDuration { get; set; } = 5;

        public virtual string SpawnHint { get; set; } = "This hint will be shown when you will spawn as a Janitor!";

        public virtual float SpawnHintDuration { get; set; } = 5;

        public virtual Dictionary<ItemCategory, sbyte> CustomInventoryLimits { get; set; } = new()
        {
            {
                ItemCategory.Medical,
                2
            }
        };

        public virtual List<ItemType> Inventory { get; set; } = new()
        {
            ItemType.Flashlight,
            ItemType.KeycardJanitor
        };

        public virtual List<uint> CustomItemsInventory { get; set; } = new();

        public virtual Dictionary<AmmoType, ushort> Ammo { get; set; } = new()
        {
            {
                AmmoType.Nato9,
                10
            }
        };

        public virtual float DamageMultiplier { get; set; } = 1;

        public virtual SpawnBehaviour? SpawnSettings { get; set; } = new();

        public virtual List<object>? CustomFlags { get; set; } = null;

        public virtual bool IgnoreSpawnSystem { get; set; } = false;

        public CustomRole ToCustomRole()
        {
            return new()
            {
                Id = Id,
                Name = Name,
                OverrideRoleName = OverrideRoleName,
                Nickname = Nickname,
                CustomInfo = CustomInfo,
                BadgeName = BadgeName,
                BadgeColor = BadgeColor,
                Role = Role,
                Team = Team,
                RoleAppearance = RoleAppearance,
                IsFriendOf = IsFriendOf,
                Health = new()
                {
                    Amount = Health.Amount,
                    Maximum = Health.Maximum
                },
                Ahp = Ahp,
                HumeShield = new()
                {
                    Amount = Health.HumeShield,
                    Maximum = Health.HumeShield,
                    RegenerationAmount = Health.HumeShieldRegenerationAmount,
                    RegenerationDelay = Health.HumeShieldRegenerationDelay
                },
                Effects = Effects,
                Stamina = Stamina,
                MaxScp330Candies = MaxScp330Candies,
                CanEscape = CanEscape,
                RoleAfterEscape = RoleAfterEscape,
                Scale = Scale,
                SpawnBroadcast = SpawnBroadcast,
                SpawnBroadcastDuration = SpawnBroadcastDuration,
                SpawnHint = SpawnHint,
                SpawnHintDuration = SpawnHintDuration,
                CustomInventoryLimits = CustomInventoryLimits,
                Inventory = Inventory,
                CustomItemsInventory = CustomItemsInventory,
                Ammo = Ammo,
                DamageMultiplier = DamageMultiplier,
                SpawnSettings = SpawnSettings,
                CustomFlags = CustomFlags,
                IgnoreSpawnSystem = IgnoreSpawnSystem
            };
        }
    }
}
