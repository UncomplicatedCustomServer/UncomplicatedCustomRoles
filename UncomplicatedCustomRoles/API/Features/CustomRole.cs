/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features
{
#pragma warning disable CS0618 // Il tipo o il membro è obsoleto
#nullable enable
    public class CustomRole : ICustomRole
    {
        /// <summary>
        /// A more easy-to-use dictionary to store every registered <see cref="ICustomRole"/>
        /// </summary>
        internal static Dictionary<int, ICustomRole> CustomRoles { get; set; } = new();

        /// <summary>
        /// Get a list of every <see cref="ICustomRole"/> registered.
        /// </summary>
        public static List<ICustomRole> List => CustomRoles.Values.ToList();

        /// <summary>
        /// Gets a list of every not loaded custom role.
        /// The data is the Id, the role path, the error type and the error name
        /// </summary>
        internal static List<ErrorCustomRole> NotLoadedRoles { get; } = new();

        /// <summary>
        /// Gets a list of every outdated loaded roles.
        /// The data is the CustomRole, the plugin Version and the role path
        /// </summary>
        internal static List<OutdatedCustomRole> OutdatedRoles { get; } = new();

        /// <summary>
        /// Gets or sets the <see cref="ICustomRole"/> unique Id
        /// </summary>
        public virtual int Id { get; set; } = 1;

        /// <summary>
        /// Gets or sets the name of the custom role.<br></br>
        /// Thisn won't be shown to players, just a thing to help you recognize better your custom roles.
        /// </summary>
        public virtual string Name { get; set; } = "Janitor";

        /// <summary>
        /// Gets or sets whether the <see cref="RoleTypeId"/> name should be hidden in favor of the <see cref="Name"/>
        /// </summary>
        public virtual bool OverrideRoleName { get; set; } = false;

        /// <summary>
        /// Gets or sets the nickname that will be set to the player if not null.
        /// </summary>
        public virtual string? Nickname { get; set; } = "D-%dnumber%";

        /// <summary>
        /// Gets or sets the CustomInfo that will be give to the player.<br></br>
        /// Will be visible only to other players
        /// </summary>
        public virtual string CustomInfo { get; set; } = "Janitor";

        /// <summary>
        /// Gets or sets the badge name
        /// </summary>
        public virtual string BadgeName { get; set; } = "Janitor";

        /// <summary>
        /// Gets or sets the badge color
        /// </summary>
        public virtual string BadgeColor { get; set; } = "pumpkin";

        /// <summary>
        /// Gets or sets the <see cref="RoleTypeId"/> of the player
        /// </summary>
        public virtual RoleTypeId Role { get; set; } = RoleTypeId.ClassD;

        /// <summary>
        /// Gets or sets the <see cref="PlayerRoles.Team"/> of the player
        /// </summary>
        public virtual Team? Team { get; set; } = null;

        /// <summary>
        /// Gets or sets the the Role Appeareance for the player.<br></br>
        /// If it's equal to <see cref="Role"/> then won't be applied
        /// </summary>
        public virtual RoleTypeId RoleAppearance { get; set; } = RoleTypeId.ClassD;

        /// <summary>
        /// Gets or sets the <see cref="Team"/>(s) that will be "friends" with this custom role
        /// </summary>
        public virtual List<Team> IsFriendOf { get; set; } = new();

        /// <summary>
        /// Gets or sets the <see cref="HealthBehaviour"/>
        /// </summary>
        public virtual HealthBehaviour Health { get; set; } = new();

        /// <summary>
        /// Gets or sets the <see cref="AhpBehaviour"/>
        /// </summary>
        public virtual AhpBehaviour Ahp { get; set; } = new();

        /// <summary>
        /// Gets or sets the <see cref="HumeShieldBehaviour"/>
        /// </summary>
        public virtual HumeShieldBehaviour HumeShield { get; set; } = new();

        /// <summary>
        /// Gets or sets the <see cref="Effect"/>
        /// </summary>
        public virtual List<Effect>? Effects { get; set; } = new();

        /// <summary>
        /// Gets or sets the <see cref="StaminaBehaviour"/>
        /// </summary>
        public virtual StaminaBehaviour Stamina { get; set; } = new();

        /// <summary>
        /// Gets or sets the maximum number of candies that can be took by the player without losing hands
        /// </summary>
        public virtual int MaxScp330Candies { get; set; } = 2;

        /// <summary>
        /// Gets or sets whether the player can escape or not
        /// </summary>
        public virtual bool CanEscape { get; set; } = true;

        /// <summary>
        /// Gets or sets the role after escape
        /// </summary>
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

        /// <summary>
        /// Gets or sets the scale of the player
        /// </summary>
        public virtual Vector3 Scale { get; set; } = Vector3.one;

        /// <summary>
        /// Gets or sets the broadcast that will be shown to the player when spawned
        /// </summary>
        public virtual string SpawnBroadcast { get; set; } = "You are a <color=orange><b>Janitor</b></color>!\nClean the Light Containment Zone!";

        /// <summary>
        /// Gets or sets the broadcast duration
        /// </summary>
        public virtual ushort SpawnBroadcastDuration { get; set; } = 5;

        /// <summary>
        /// Gets or sets the hint that will be shown to the player when spawned
        /// </summary>
        public virtual string SpawnHint { get; set; } = "This hint will be shown when you will spawn as a Janitor!";

        /// <summary>
        /// Gets or sets hint duration
        /// </summary>
        public virtual float SpawnHintDuration { get; set; } = 5;

        /// <summary>
        /// Gets or sets the custom inventory limits to override the default ones
        /// </summary>
        public virtual Dictionary<ItemCategory, sbyte> CustomInventoryLimits { get; set; } = new();

        /// <summary>
        /// Gets or sets the inventory of the player
        /// </summary>
        public virtual List<ItemType> Inventory { get; set; } = new()
        {
            ItemType.Flashlight,
            ItemType.KeycardJanitor
        };

        /// <summary>
        /// Gets or sets the custom items inventory of the player
        /// </summary>
        public virtual List<uint> CustomItemsInventory { get; set; } = new();

        /// <summary>
        /// Gets or sets the ammo inventory of the player
        /// </summary>
        public virtual Dictionary<ItemType, ushort> Ammo { get; set; } = new()
        {
            {
                ItemType.Ammo9x19,
                10
            }
        };

        /// <summary>
        /// Gets or sets the damage multiplier.<br></br>
        /// This will increase - keep normal - or decrease the damage that this role will do
        /// </summary>
        public virtual float DamageMultiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets the <see cref="SpawnBehaviour"/>
        /// </summary>
        public virtual SpawnBehaviour? SpawnSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the <see cref="Enums.CustomFlags"/> of the custom role
        /// </summary>
        public virtual List<object>? CustomFlags { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the custom role should be evaluated during normal spawn events or not
        /// </summary>
        public virtual bool IgnoreSpawnSystem { get; set; } = false;

        /// <summary>
        /// Invoked when the custom role is spawned
        /// </summary>
        /// <param name="role"></param>
        public virtual void OnSpawned(SummonedCustomRole role)
        { }

        public override string ToString() => $"{Name} ({Id})";

#nullable disable
        /// <summary>
        /// Try to get a registered <see cref="ICustomRole"/> by it's Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="customRole"></param>
        /// <returns><see cref="true"/> if the operation was successfull.</returns>
        public static bool TryGet(int id, out ICustomRole customRole)
        {
            if (CustomRoles.ContainsKey(id))
            {
                customRole = CustomRoles[id];
                return true;
            }

            customRole = null;
            return false;
        }

        /// <summary>
        /// Get a registered <see cref="ICustomRole"/> by it's Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="ICustomRole"/> with the given Id or <see cref="null"/> if not found.</returns>
        public static ICustomRole Get(int id)
        {
            if (TryGet(id, out ICustomRole customRole)) 
                return customRole; 

            return null;
        }

        /// <summary>
        /// Register a new <see cref="ICustomRole"/> instance.
        /// </summary>
        /// <param name="customRole"></param>
        public static LoadStatusType Register(ICustomRole customRole) => CompatibilityManager.RegisterCustomRole(customRole);

        /// <summary>
        /// Unregister a registered <see cref="ICustomRole"/>.
        /// </summary>
        /// <param name="customRole"></param>
        public static void Unregister(ICustomRole customRole)
        {
            if (CustomRoles.ContainsKey(customRole.Id))
                CustomRoles.Remove(customRole.Id);
        }

        internal static bool Validate(ICustomRole role, out string error)
        {
            error = "Role seems to be null";

            if (role is null)
                return false;

            if (role.SpawnSettings is null)
            {
                error = $"Role has no spawn_settings";
                return false;
            }

            if (role.SpawnSettings.Spawn is SpawnType.ZoneSpawn && role.SpawnSettings.SpawnZones.Count() < 1)
            {
                error = "If the SpawnType is ZoneSpawn the list SpawnZones shouldn't be empty";
                return false;
            }
            else if (role.SpawnSettings.Spawn is SpawnType.RoomsSpawn && role.SpawnSettings.SpawnRooms.Count() < 1)
            {
                error = "If the SpawnType is RoomsSpawn the list SpawnRooms shouldn't be empty";
                return false;
            }
            else if (role.SpawnSettings.Spawn is SpawnType.SpawnPointSpawn && (role.SpawnSettings.SpawnPoints is null || role.SpawnSettings.SpawnPoints.Count == 0))
            {
                error = "If the SpawnType is SpawnPointSpawn the list SpawnPoints shouldn't be empty";
                return false;
            }

            return true;
        }

        internal static LoadStatusType InternalRegister(ICustomRole customRole)
        {
            if (!Validate(customRole, out string _))
                return LoadStatusType.ValidatorError;

            if (!CustomRoles.ContainsKey(customRole.Id))
            {
                CustomRoles.Add(customRole.Id, customRole);

                return LoadStatusType.Success;
            }

            return LoadStatusType.SameId;
        }
    }
}
