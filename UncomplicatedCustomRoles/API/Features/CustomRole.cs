/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Events;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features;
#nullable enable
public class CustomRole : ICustomRole
{
    /// <summary>
    ///     A more easy-to-use dictionary to store every registered <see cref="ICustomRole" />
    /// </summary>
    internal static ConcurrentDictionary<int, ICustomRole> CustomRoles { get; set; } = new();

    /// <summary>
    ///     Get a list of every <see cref="ICustomRole" /> registered.
    /// </summary>
    public static ICollection<ICustomRole> List => CustomRoles.Values;

    /// <summary>
    ///     Gets a list of every not loaded custom role.
    ///     The data is the Id, the role path, the error type and the error name
    /// </summary>
    internal static List<ErrorCustomRole> NotLoadedRoles { get; } = [];

    /// <summary>
    ///     Gets a list of every outdated loaded roles.
    ///     The data is the CustomRole, the plugin Version and the role path
    /// </summary>
    internal static List<OutdatedCustomRole> OutdatedRoles { get; } = [];

    /// <summary>
    ///     Gets or sets the <see cref="ICustomRole" /> unique Id
    /// </summary>
    public virtual int Id { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the name of the custom role.<br></br>
    ///     Thisn won't be shown to players, just a thing to help you recognize better your custom roles.
    /// </summary>
    public virtual string Name { get; set; } = "Janitor";

    /// <summary>
    ///     Gets or sets whether the <see cref="RoleTypeId" /> name should be hidden in favor of the <see cref="Name" />
    /// </summary>
    public virtual bool OverrideRoleName { get; set; } = false;

    /// <summary>
    ///     Gets or sets the nickname that will be set to the player if not null.
    /// </summary>
    public virtual string? Nickname { get; set; } = "D-%dnumber%";

    /// <summary>
    ///     Gets or sets the CustomInfo that will be give to the player.<br></br>
    ///     Will be visible only to other players
    /// </summary>
    public virtual string CustomInfo { get; set; } = "Janitor";

    /// <summary>
    ///     Gets or sets the badge name
    /// </summary>
    public virtual string BadgeName { get; set; } = "Janitor";

    /// <summary>
    ///     Gets or sets the badge color
    /// </summary>
    public virtual string BadgeColor { get; set; } = "pumpkin";

    /// <summary>
    ///     Gets or sets the <see cref="RoleTypeId" /> of the player
    /// </summary>
    public virtual RoleTypeId Role { get; set; } = RoleTypeId.ClassD;

    /// <summary>
    ///     Gets or sets the <see cref="PlayerRoles.Team" /> of the player
    /// </summary>
    public virtual Team? Team { get; set; } = null;

    /// <summary>
    ///     Gets or sets the the Role Appeareance for the player.<br></br>
    ///     If it's equal to <see cref="Role" /> then won't be applied.<br></br>
    ///     Leave it empty to keep the appearance of <see cref="Role" />: anything that is not a usable alive role
    ///     falls back to it when the role is registered.
    /// </summary>
    public virtual RoleTypeId RoleAppearance { get; set; } = RoleTypeId.None;

    /// <summary>
    ///     Gets or sets the <see cref="Team" />(s) that will be "friends" with this custom role
    /// </summary>
    public virtual List<Team> IsFriendOf { get; set; } = [];

    /// <summary>
    ///     Gets or sets the <see cref="HealthBehaviour" />
    /// </summary>
    public virtual HealthBehaviour Health { get; set; } = new();

    /// <summary>
    ///     Gets or sets the <see cref="AhpBehaviour" />
    /// </summary>
    public virtual AhpBehaviour Ahp { get; set; } = new();

    /// <summary>
    ///     Gets or sets the <see cref="HumeShieldBehaviour" />
    /// </summary>
    public virtual HumeShieldBehaviour HumeShield { get; set; } = new();

    /// <summary>
    ///     Gets or sets the <see cref="Effect" />
    /// </summary>
    public virtual List<Effect>? Effects { get; set; } = [];

    /// <summary>
    ///     Gets or sets the <see cref="StaminaBehaviour" />
    /// </summary>
    public virtual StaminaBehaviour Stamina { get; set; } = new();

    /// <summary>
    ///     Gets or sets the maximum number of candies that can be took by the player without losing hands
    /// </summary>
    public virtual int MaxScp330Candies { get; set; } = 2;

    /// <summary>
    ///     Gets or sets whether the player can escape or not
    /// </summary>
    public virtual bool CanEscape { get; set; } = true;

    /// <summary>
    ///     Gets or sets the role after escape
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
    ///     Gets or sets the scale of the player
    /// </summary>
    public virtual Vector3 Scale { get; set; } = Vector3.one;

    /// <summary>
    ///     Gets or sets the broadcast that will be shown to the player when spawned
    /// </summary>
    public virtual string SpawnBroadcast { get; set; } =
        "You are a <color=orange><b>Janitor</b></color>!\nClean the Light Containment Zone!";

    /// <summary>
    ///     Gets or sets the broadcast duration
    /// </summary>
    public virtual ushort SpawnBroadcastDuration { get; set; } = 5;

    /// <summary>
    ///     Gets or sets the hint that will be shown to the player when spawned
    /// </summary>
    public virtual string SpawnHint { get; set; } = "This hint will be shown when you will spawn as a Janitor!";

    /// <summary>
    ///     Gets or sets hint duration
    /// </summary>
    public virtual float SpawnHintDuration { get; set; } = 5;

    /// <summary>
    ///     Gets or sets the custom inventory limits to override the default ones
    /// </summary>
    public virtual Dictionary<ItemCategory, sbyte> CustomInventoryLimits { get; set; } = new();

    /// <summary>
    ///     Gets or sets the inventory of the player
    /// </summary>
    public virtual List<ItemType> Inventory { get; set; } =
    [
        ItemType.Flashlight,
        ItemType.KeycardJanitor
    ];

    /// <summary>
    ///     Gets or sets the custom items inventory of the player
    /// </summary>
    public virtual List<uint> CustomItemsInventory { get; set; } = [];

    /// <summary>
    ///     Gets or sets the ammo inventory of the player
    /// </summary>
    public virtual Dictionary<ItemType, ushort> Ammo { get; set; } = new()
    {
        {
            ItemType.Ammo9x19,
            10
        }
    };

    /// <summary>
    ///     Gets or sets the damage multiplier.<br></br>
    ///     This will increase - keep normal - or decrease the damage that this role will do
    /// </summary>
    public virtual float DamageMultiplier { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the <see cref="SpawnBehaviour" />
    /// </summary>
    public virtual SpawnBehaviour? SpawnSettings { get; set; } = new();

    /// <summary>
    ///     Gets or sets the <see cref="Enums.CustomFlags" /> of the custom role
    /// </summary>
    public virtual List<object>? CustomFlags { get; set; } = null;

    /// <summary>
    ///     Gets or sets whether the custom role should be evaluated during normal spawn events or not
    /// </summary>
    public virtual bool IgnoreSpawnSystem { get; set; } = false;

    /// <summary>
    ///     Invoked when the custom role is spawned
    /// </summary>
    /// <param name="role"></param>
    public virtual void OnSpawned(SummonedCustomRole role)
    {
    }

    /// <summary>
    ///     Invoked when the custom role is removed from the player
    /// </summary>
    /// <param name="role"></param>
    public virtual void OnRemoved(SummonedCustomRole role)
    {
    }

    public override string ToString()
    {
        return $"{Regex.Replace(Name, "<color=.*?>(.*?)</color>", "$1")} ({Id})";
    }

#nullable disable
    /// <summary>
    ///     Try to get a registered <see cref="ICustomRole" /> by it's Id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="customRole"></param>
    /// <returns><see cref="true" /> if the operation was successfull.</returns>
    public static bool TryGet(int id, out ICustomRole customRole)
    {
        return CustomRoles.TryGetValue(id, out customRole);
    }

    /// <summary>
    ///     Try to get a registered <see cref="ICustomRole" /> by it's <see cref="Name" /> (case-insensitive).
    ///     If more roles share the same name the first registered one is returned.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="customRole"></param>
    /// <returns><see cref="true" /> if a role with the given name was found.</returns>
    public static bool TryGet(string name, out ICustomRole customRole)
    {
        customRole = null;

        if (string.IsNullOrEmpty(name))
            return false;

        foreach (var role in CustomRoles.Values)
            if (string.Equals(role.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                customRole = role;
                return true;
            }

        return false;
    }

    /// <summary>
    ///     Try to get the first registered <see cref="ICustomRole" /> of the given type.
    ///     Useful for plugins that register their roles as classes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="customRole"></param>
    /// <returns><see cref="true" /> if a role of the given type was found.</returns>
    public static bool TryGet<T>(out T customRole) where T : class, ICustomRole
    {
        customRole = CustomRoles.Values.OfType<T>().FirstOrDefault();
        return customRole is not null;
    }

    /// <summary>
    ///     Get a registered <see cref="ICustomRole" /> by it's Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The <see cref="ICustomRole" /> with the given Id or <see cref="null" /> if not found.</returns>
    public static ICustomRole Get(int id)
    {
        if (TryGet(id, out var customRole))
            return customRole;

        return null;
    }

    /// <summary>
    ///     Get a registered <see cref="ICustomRole" /> by it's <see cref="Name" /> (case-insensitive)
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The first <see cref="ICustomRole" /> with the given name or <see cref="null" /> if not found.</returns>
    public static ICustomRole Get(string name)
    {
        return TryGet(name, out var customRole) ? customRole : null;
    }

    /// <summary>
    ///     Get the first registered <see cref="ICustomRole" /> of the given type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The first role of the given type or <see cref="null" /> if not found.</returns>
    public static T Get<T>() where T : class, ICustomRole
    {
        return TryGet<T>(out var customRole) ? customRole : null;
    }

    /// <summary>
    ///     Gets whether a <see cref="ICustomRole" /> with the given Id is registered
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IsRegistered(int id)
    {
        return CustomRoles.ContainsKey(id);
    }

    /// <summary>
    ///     Gets the first Id that is not used by any registered <see cref="ICustomRole" />.
    ///     Useful when creating roles at runtime.
    /// </summary>
    /// <param name="start">The Id from which the search starts</param>
    /// <returns></returns>
    public static int GetFirstFreeId(int start = 1)
    {
        return CompatibilityManager.GetFirstFreeId(start);
    }

    /// <summary>
    ///     Register a new <see cref="ICustomRole" /> instance.
    /// </summary>
    /// <param name="customRole"></param>
    public static LoadStatusType Register(ICustomRole customRole)
    {
        return CompatibilityManager.RegisterCustomRole(customRole);
    }

    /// <summary>
    ///     Unregister a registered <see cref="ICustomRole" />.
    /// </summary>
    /// <param name="customRole"></param>
    /// <param name="removeFromPlayers">
    ///     If true every player currently playing this role will lose it (the
    ///     <see cref="SummonedCustomRole" /> instances get destroyed)
    /// </param>
    /// <returns><see cref="true" /> if the role was registered and has been removed.</returns>
    public static bool Unregister(ICustomRole customRole, bool removeFromPlayers = false)
    {
        return customRole is not null && Unregister(customRole.Id, removeFromPlayers);
    }

    /// <summary>
    ///     Unregister a registered <see cref="ICustomRole" /> by it's Id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="removeFromPlayers">
    ///     If true every player currently playing this role will lose it (the
    ///     <see cref="SummonedCustomRole" /> instances get destroyed)
    /// </param>
    /// <returns><see cref="true" /> if the role was registered and has been removed.</returns>
    public static bool Unregister(int id, bool removeFromPlayers = false)
    {
        if (!CustomRoles.TryRemove(id, out var customRole))
            return false;

        if (removeFromPlayers)
            foreach (var summoned in SummonedCustomRole.List.Values.Where(scr => scr.Role.Id == id).ToList())
                summoned.Destroy();

        CustomRoleEvents.OnUnregistered(new CustomRoleUnregisteredEventArgs(customRole));
        return true;
    }

    /// <summary>
    ///     Validate a <see cref="ICustomRole" /> without registering it.
    ///     Useful to check roles that are being built at runtime before calling <see cref="Register(ICustomRole)" />.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="errors">The list of blocking problems - if not empty the role can't be registered</param>
    /// <param name="warnings">The list of non-blocking problems</param>
    /// <returns><see cref="true" /> if the role has no blocking problems.</returns>
    public static bool Validate(ICustomRole role, out List<string> errors, out List<string> warnings)
    {
        RoleValidator.Validate(role, out errors, out warnings);
        return errors.Count == 0;
    }

    internal static bool Validate(ICustomRole role, out string error)
    {
        return RoleValidator.IsValid(role, out error);
    }

    internal static LoadStatusType InternalRegister(ICustomRole customRole)
    {
        FlagMigrator.Migrate(customRole);

        if (customRole.RoleAppearance is RoleTypeId.None ||
            customRole.RoleAppearance.GetTeam() is PlayerRoles.Team.Dead)
            customRole.RoleAppearance = customRole.Role;

        if (Plugin.Instance.Config.EnableValidator)
        {
            RoleValidator.Validate(customRole, out var errors, out var warnings);

            foreach (var warning in warnings)
                LogManager.Warn($"[Role Validator] {customRole}: {warning}");

            if (errors.Count > 0)
                return LoadStatusType.ValidatorError;
        }

        var registeringArgs = new CustomRoleRegisteringEventArgs(customRole);
        CustomRoleEvents.OnRegistering(registeringArgs);
        if (!registeringArgs.IsAllowed)
            return LoadStatusType.Denied;

        if (!CustomRoles.TryAdd(customRole.Id, customRole))
            return LoadStatusType.SameId;

        CustomRoleEvents.OnRegistered(new CustomRoleRegisteredEventArgs(customRole));
        return LoadStatusType.Success;
    }
}