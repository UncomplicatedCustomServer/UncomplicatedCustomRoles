using Exiled.API.Enums;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.API.Interfaces;
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
        /// Gets or sets the <see cref="IUCREffect"/>s
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
        public virtual Dictionary<ItemCategory, sbyte> CustomInventoryLimits { get; set; } = new()
        {
            {
                ItemCategory.Medical,
                2
            }
        };

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
        public virtual Dictionary<AmmoType, ushort> Ammo { get; set; } = new()
        {
            {
                AmmoType.Nato9,
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
        public virtual CustomFlags? CustomFlags { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the custom role should be evaluated during normal spawn events or not
        /// </summary>
        public virtual bool IgnoreSpawnSystem { get; set; } = false;

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
        /// Register a new <see cref="ICustomRole"/>.
        /// Required only if you want the custom role to be evaluated from UCR.
        /// </summary>
        /// <param name="customRole"></param>
        public static void Register(ICustomRole Role, bool notLoadIfLoaded = false)
        {
            if (!Validate(Role))
            {
                LogManager.Warn($"Failed to register the UCR role with the ID {Role.Id} due to the validator check!");

                return;
            }

            if (!CustomRoles.ContainsKey(Role.Id))
            {
                CustomRoles.Add(Role.Id, Role);

                if (Plugin.Instance.Config.EnableBasicLogs)
                    LogManager.Info($"Successfully registered the UCR role with the ID {Role.Id} and {Role.Name} as name!");

                return;
            }

            if (notLoadIfLoaded)
            {
                LogManager.Debug($"Can't load role {Role.Id} {Role.Name} due to plugin settings!\nPlease reach UCS support for UCR!");
                return;
            }

            LogManager.Warn($"Failed to register the UCR role with the ID {Role.Id}: The problem can be the following: ERR_ID_ALREADY_HERE!\nTrying to assign a new one...");

            int NewId = GetFirstFreeID(Role.Id);

            LogManager.Info($"Custom Role {Role.Name} with the old Id {Role.Id} will be registered with the following Id: {NewId}");

            Role.Id = NewId;

            Register(Role, true);
        }

        /// <summary>
        /// Validate a <see cref="ICustomRole"/>
        /// </summary>
        /// <param name="Role"></param>
        /// <returns></returns>
        [Obsolete("This method should not be used as was intended for the first versions of UCR and now the plugin can handle also things that are reported as errors here!", false)]
        public static bool Validate(ICustomRole Role)
        {
            if (Role is null)
                return false;

            if (Role.SpawnSettings is null)
            {
                LogManager.Warn($"Is kinda useless registering a role with no spawn_settings.\nFound (or not found) in role: {Role.Name} ({Role.Id})");
                return false;
            }

            if (Role.SpawnSettings.Spawn == SpawnType.ZoneSpawn && Role.SpawnSettings.SpawnZones.Count() < 1)
            {
                LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the ZoneSpawn as SpawnType the List SpawnZones can't be empty!");
                return false;
            }
            else if (Role.SpawnSettings.Spawn == SpawnType.RoomsSpawn && Role.SpawnSettings.SpawnRooms.Count() < 1)
            {
                LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the RoomSpawn as SpawnType the List SpawnRooms can't be empty!");
                return false;
            }
            else if (Role.SpawnSettings.Spawn == SpawnType.SpawnPointSpawn && (Role.SpawnSettings.SpawnPoints is null || Role.SpawnSettings.SpawnPoints.Count == 0))
            {
                LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the SpawnPointSpawn as SpawnType the SpawnPoint can't be empty or null!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the first free id to register a new custom role
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static int GetFirstFreeID(int Id)
        {
            while (CustomRoles.ContainsKey(Id))
                Id++;

            return Id;
        }

        /// <summary>
        /// Unregister a registered <see cref="ICustomRole"/>.
        /// </summary>
        /// <param name="customRole"></param>
        public static void Unregister(ICustomRole customRole)
        {
            if (CustomRoles.ContainsKey(customRole.Id))
                CustomRoles.Remove(customRole.Id);
        }
    }
}
