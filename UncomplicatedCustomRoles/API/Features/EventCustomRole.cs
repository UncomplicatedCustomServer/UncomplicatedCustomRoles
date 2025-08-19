/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using PlayerRoles;
using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.ObjectiveEvents;
using LabApi.Events.Arguments.Scp127Events;
using LabApi.Events.Arguments.Scp3114Events;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features
{
#nullable enable
    public class EventCustomRole : ICustomRole
    {
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

        public override string ToString() => $"{Name} ({Id})";

        /// <summary>
        /// Invoked when the Custom Role is spawned
        /// </summary>
        /// <param name="role"></param>
        public virtual void OnSpawned(SummonedCustomRole role)
        { }
        
        /// <summary>
        /// Invoked when the Custom Role is spawned
        /// </summary>
        /// <param name="role"></param>
        public virtual void OnRemoved(SummonedCustomRole role)
        { }

        /// <summary>
        /// Called before kicking a <see cref="LabApi.Features.Wrappers.Player"/> from the server.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerKickingEventArgs"/> instance.</param>
        public virtual void OnKicking(PlayerKickingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> has been kicked from the server.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerKickedEventArgs"/> instance.</param>
        public virtual void OnKicked(PlayerKickedEventArgs ev) { }

        /// <summary>
        /// Called before banning a <see cref="LabApi.Features.Wrappers.Player"/> from the server.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerBanningEventArgs"/> instance.</param>
        public virtual void OnBanning(PlayerBanningEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> danger state changes.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingDangerStateEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnChangingDangerState(object ev) { }

        /// <summary>
        /// Called after a player has been banned from the server.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerBannedEventArgs"/> instance.</param>
        public virtual void OnBanned(PlayerBannedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/>  earns an achievement.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerReceivedAchievementEventArgs"/> instance.</param>
        public virtual void OnReceivedAchievement(PlayerReceivedAchievementEventArgs ev) { }

        /// <summary>
        /// Called before using a usable item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUsingItemEventArgs"/> instance.</param>
        public virtual void OnUsingItem(PlayerUsingItemEventArgs ev) { }

        /// <summary>
        /// Called before completed using of a usable item.
        /// </summary>
        /// <param name="ev">The <see cref="UsingItemEventArgs"/> instance.</param>
        [Obsolete("Only works on EXILED due to the need of a patch, please refer to OnUsedItem")]
        public virtual void OnUsingItemCompleted(object ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> used a <see cref="LabApi.Features.Wrappers.UsableItem"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUsedItemEventArgs"/> instance.</param>
        public virtual void OnUsedItem(PlayerUsedItemEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> has stopped the use of a <see cref="API.Features.Items.Usable"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerCancellingUsingItemEventArgs"/> instance.</param>
        public virtual void OnCancellingItemUse(PlayerCancellingUsingItemEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> has stopped the use of a <see cref="API.Features.Items.Usable"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerCancelledUsingItemEventArgs"/> instance.</param>
        public virtual void OnCancelledItemUse(PlayerCancelledUsingItemEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> interacted with something.
        /// </summary>
        /// <param name="ev">The <see cref="InteractedEventArgs"/> instance.</param>
        [Obsolete("The generic interaction event is not available in LabAPI, please handle every interaction in a separate method.")]
        public virtual void OnInteracted(object ev) { }

        /// <summary>
        /// Called before spawning a <see cref="LabApi.Features.Wrappers.Player"/> ragdoll.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSpawningRagdollEventArgs"/> instance.</param>
        public virtual void OnSpawningRagdoll(PlayerSpawningRagdollEventArgs ev) { }

        /// <summary>
        /// Called after spawning a <see cref="LabApi.Features.Wrappers.Player"/> ragdoll.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSpawnedRagdollEventArgs"/> instance.</param>
        public virtual void OnSpawnedRagdoll(PlayerSpawnedRagdollEventArgs ev) { }

        /// <summary>
        /// Called before activating the warhead panel.
        /// </summary>
        /// <param name="ev">The <see cref="ActivatingWarheadPanelEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnActivatingWarheadPanel(object ev) { }

        /// <summary>
        /// Called before activating a workstation.
        /// </summary>
        /// <param name="ev">The <see cref="ActivatingWorkstation"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnActivatingWorkstation(object ev) { }

        /// <summary>
        /// Called before deactivating a workstation.
        /// </summary>
        /// <param name="ev">The <see cref="DeactivatingWorkstationEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnDeactivatingWorkstation(object ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> has left the server.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerLeftEventArgs"/> instance.</param>
        public virtual void OnLeft(PlayerLeftEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> died.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDeathEventArgs"/> instance.</param>
        public virtual void OnDied(PlayerDeathEventArgs ev) { }

        /// <summary>
        /// Called before changing a <see cref="LabApi.Features.Wrappers.Player"/> role.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangingRoleEventArgs"/> instance.</param>
        /// <remarks>If <see cref="PlayerChangingRoleEventArgs.IsAllowed"/> is set to <see langword="false"/> when Escape is <see langword="true"/>, awards will still be given to the escapee's team even though they will 'fail' to escape. Use <see cref="OnEscaping"/> to block escapes instead.</remarks>
        public virtual void OnChangingRole(PlayerChangingRoleEventArgs ev) { }

        /// <summary>
        /// Called before throwing a grenade.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerThrowingProjectileEventArgs"/> instance.</param>
        public virtual void OnThrowingProjectile(PlayerThrowingProjectileEventArgs ev) { }

        /// <summary>
        /// Called after threw a grenade.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerThrewProjectileEventArgs"/> instance.</param>
        public virtual void OnThrewProjectile(PlayerThrewProjectileEventArgs ev) { }

        /// <summary>
        /// Called before receving a throwing request.
        /// </summary>
        /// <param name="ev">The <see cref="ThrowingRequestEventArgs"/> instance.</param>
        [Obsolete("Please refer to OnThrowingItem")]
        public virtual void OnThrowingRequest(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> throws an item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerThrowingItemEventArgs"/> instance.</param>
        public virtual void OnThrowingItem(PlayerThrowingItemEventArgs ev) { }

        /// <summary>
        /// Called after threw an item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerThrewItemEventArgs"/> instance.</param>
        public virtual void OnThrewItem(PlayerThrewItemEventArgs ev) { }

        /// <summary>
        /// Called before dropping an item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDroppingItemEventArgs"/> instance.</param>
        public virtual void OnDroppingItem(PlayerDroppingItemEventArgs ev) { }

        /// <summary>
        /// Called after dropping an item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDroppedItemEventArgs"/> instance.</param>
        public virtual void OnDroppedItem(PlayerDroppedItemEventArgs ev) { }

        /// <summary>
        /// Called before dropping a null item.
        /// </summary>
        /// <param name="ev">The <see cref="DroppingNothingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnDroppingNothing(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> picks up an item.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerPickingUpItemEventArgs"/> instance.</param>
        public virtual void OnPickingUpItem(PlayerPickingUpItemEventArgs ev) { }

        /// <summary>
        /// Called before handcuffing a <see cref="LabApi.Features.Wrappers.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerCuffedEventArgs"/> instance.</param>
        public virtual void OnHandcuffing(PlayerCuffingEventArgs ev) { }

        /// <summary>
        /// Called after handcuffing a <see cref="LabApi.Features.Wrappers.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerCuffedEventArgs"/> instance.</param>
        public virtual void OnHandcuffed(PlayerCuffedEventArgs ev) { }

        /// <summary>
        /// Called before freeing a handcuffed <see cref="LabApi.Features.Wrappers.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUncuffingEventArgs"/> instance.</param>
        public virtual void OnRemovingHandcuffs(PlayerUncuffingEventArgs ev) { }

        /// <summary>
        /// Called after freeing a handcuffed <see cref="LabApi.Features.Wrappers.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUncuffedEventArgs"/> instance.</param>
        public virtual void OnRemovedHandcuffs(PlayerUncuffedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> escapes.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerEscapingEventArgs"/> instance.</param>
        public virtual void OnEscaping(PlayerEscapingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> escapes.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerEscapedEventArgs"/> instance.</param>
        public virtual void OnEscaped(PlayerEscapedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> begins speaking in the intercom.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUsingIntercomEventArgs"/> instance.</param>
        public virtual void OnIntercomSpeaking(PlayerUsingIntercomEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> finished speaking in the intercom.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUsedIntercomEventArgs"/> instance.</param>
        public virtual void OnIntercomSpeakingFinished(PlayerUsedIntercomEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> shoots a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerShootingWeaponEventArgs"/> instance.</param>
        public virtual void OnShot(PlayerShotWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> shoots a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerShootingWeaponEventArgs"/> instance.</param>
        public virtual void OnShooting(PlayerShootingWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> enters the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerEnteringPocketDimensionEventArgs"/> instance.</param>
        public virtual void OnEnteringPocketDimension(PlayerEnteringPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> enters the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerEnteredPocketDimensionEventArgs"/> instance.</param>

        public virtual void OnEnteredPocketDimension(PlayerEnteredPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> leaves the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingPocketDimensionEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please see OnLeftPocketDimension")]
        public virtual void OnEscapingPocketDimension(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> leaves the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerLeavingPocketDimensionEventArgs"/> instance.</param>
        public virtual void OnLeavingPocketDimension(PlayerLeavingPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> fails to escape the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="FailingEscapePocketDimensionEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please see OnLeftPocketDimension")]
        public virtual void OnFailingEscapePocketDimension(object ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> left the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerLeftPocketDimensionEventArgs"/> instance.</param>
        public virtual void OnLeftPocketDimension(PlayerLeftPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> enters killer collision.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringKillerCollisionEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnEnteringKillerCollision(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> reloads a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerReloadingWeaponEventArgs"/> instance.</param>
        public virtual void OnReloadingWeapon(PlayerReloadingWeaponEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> held item changes.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangedItemEventArgs"/> instance.</param>
        public virtual void OnChangedItem(PlayerChangedItemEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> held item changes.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangingItemEventArgs"/> instance.</param>
        public virtual void OnChangingItem(PlayerChangingItemEventArgs ev) { }

        /// <summary>
        /// Called before changing a <see cref="LabApi.Features.Wrappers.Player"/> group.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerGroupChangingEventArgs"/> instance.</param>
        public virtual void OnChangingGroup(PlayerGroupChangingEventArgs ev) { }

        /// <summary>
        /// Called after changing a <see cref="LabApi.Features.Wrappers.Player"/> group.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerGroupChangedEventArgs"/> instance.</param>
        public virtual void OnChangedGroup(PlayerGroupChangedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> interacts with an elevator.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractingElevatorEventArgs"/> instance.</param>
        public virtual void OnInteractingElevator(PlayerInteractingElevatorEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> interacts with an elevator.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractedElevatorEventArgs"/> instance.</param>
        public virtual void OnInteractedElevator(PlayerInteractedElevatorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a locker.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractingLocker(PlayerInteractingLockerEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a locker.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractedLockerEventArgs"/> instance.</param>
        public virtual void OnInteractedLocker(PlayerInteractedLockerEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a generator.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractingGeneratorEventArgs"/> instance.</param>
        public virtual void OnInteractingGenerator(PlayerInteractingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a generator.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractedGeneratorEventArgs"/> instance.</param>
        public virtual void OnInteractedGenerator(PlayerInteractedGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a door.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractingDoorEventArgs"/> instance.</param>
        public virtual void OnInteractingDoor(PlayerInteractingDoorEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a door.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractedDoorEventArgs"/> instance.</param>
        public virtual void OnInteractedDoor(PlayerInteractedDoorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> interacts with SCP-330.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractingScp330EventArgs"/> instance.</param>
        public virtual void OnInteractingScp330(PlayerInteractingScp330EventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> interacts with SCP-330.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractedScp330EventArgs"/> instance.</param>
        public virtual void OnInteractedScp330(PlayerInteractedScp330EventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a shooting target.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractingShootingTargetEventArgs"/> instance.</param>
        public virtual void OnInteractingShootingTarget(PlayerInteractingShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a shooting target.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractedShootingTargetEventArgs"/> instance.</param>
        public virtual void OnInteractedShootingTarget(PlayerInteractedShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> triggers a tesla.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerTriggeringTeslaEventArgs"/> instance.</param>
        public virtual void OnTriggeringTesla(PlayerTriggeringTeslaEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> triggers a tesla.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerTriggeredTeslaEventArgs"/> instance.</param>
        public virtual void OnTriggeredTesla(PlayerTriggeredTeslaEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> receives a status effect.
        /// </summary>
        /// <param name="ev">The <see cref="ReceivingEffectEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please refer to OnUpdatedEffect")]
        public virtual void OnReceivingEffect(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> receives a status effect.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerEffectUpdatingEventArgs"/> instance.</param>
        public virtual void OnUpdatingEffect(PlayerEffectUpdatingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> receives a status effect.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerEffectUpdatedEventArgs"/> instance.</param>
        public virtual void OnUpdatedEffect(PlayerEffectUpdatedEventArgs ev) { }

        /// <summary>
        /// Called before a user's radio battery charge is changed.
        /// </summary>
        /// <param name="ev">The <see cref="UsingRadioBatteryEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please refer to OnUsingRadio")]
        public virtual void OnUsingRadioBattery(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> uses a Radio.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUsingRadioEventArgs"/> instance.</param>
        public virtual void OnUsingRadio(PlayerUsingRadioEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> MicroHID state is changed.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingMicroHIDStateEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnChangingMicroHIDState(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> MicroHID energy is changed.
        /// </summary>
        /// <param name="ev">The <see cref="UsingMicroHIDEnergyEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnUsingMicroHIDEnergy(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> damages a shooting target.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDamagingShootingTargetEventArgs"/> instance.</param>
        public virtual void OnDamagingShootingTarget(PlayerDamagingShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> damages a shooting target.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDamagedShootingTargetEventArgs"/> instance.</param>
        public virtual void OnDamagedShootingTarget(PlayerDamagedShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> flips a coin.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerFlippingCoinEventArgs"/> instance.</param>
        public virtual void OnFlippingCoin(PlayerFlippingCoinEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> flips a coin.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerFlippedCoinEventArgs"/> instance.</param>
        public virtual void OnFlippedCoin(PlayerFlippedCoinEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> toggles the flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerTogglingFlashlightEventArgs"/> instance.</param>
        public virtual void OnTogglingFlashlight(PlayerTogglingFlashlightEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> toggles the flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerToggledFlashlightEventArgs"/> instance.</param>
        public virtual void OnToggledFlashlight(PlayerToggledFlashlightEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> unloads a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUnloadingWeaponEventArgs"/> instance.</param>
        public virtual void OnUnloadingWeapon(PlayerUnloadingWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> unloads a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUnloadedWeaponEventArgs"/> instance.</param>
        public virtual void OnUnloadedWeapon(PlayerUnloadedWeaponEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> triggers an aim action.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerAimedWeaponEventArgs"/> instance.</param>
        public virtual void OnAimingDownSight(PlayerAimedWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> toggles the weapon's flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerTogglingWeaponFlashlightEventArgs"/> instance.</param>
        public virtual void OnTogglingWeaponFlashlight(PlayerTogglingWeaponFlashlightEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> toggles the weapon's flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerToggledWeaponFlashlightEventArgs"/> instance.</param>
        public virtual void OnToggledWeaponFlashlight(PlayerToggledWeaponFlashlightEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> dryfires a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDryFiringWeaponEventArgs"/> instance.</param>
        public virtual void OnDryfiringWeapon(PlayerDryFiringWeaponEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> dryfires a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDryFiredWeaponEventArgs"/> instance.</param>
        public virtual void OnDryfiredWeapon(PlayerDryFiredWeaponEventArgs ev) { }

        /// <summary>
        /// Invoked after a <see cref="LabApi.Features.Wrappers.Player"/> presses the voicechat key.
        /// </summary>
        /// <param name="ev">The <see cref="VoiceChattingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please refer to OnSendingVoiceMessage")]
        public virtual void OnVoiceChatting(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> sends a Voice Message.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSendingVoiceMessageEventArgs"/> instance.</param>
        public virtual void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> receives a Voice Message.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerReceivingVoiceMessageEventArgs"/> instance.</param>
        public virtual void OnReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> makes noise.
        /// </summary>
        /// <param name="ev">The <see cref="MakingNoiseEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnMakingNoise(object ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> lands.
        /// </summary>
        /// <param name="ev">The <see cref="LandingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnLanding(object ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> presses the transmission key.
        /// </summary>
        /// <param name="ev">The <see cref="TransmittingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please refer to OnUsingRadio")]
        public virtual void OnTransmitting(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> changes move state.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerMovementStateChangedEventArgs"/> instance.</param>
        public virtual void OnMovementStateChanged(PlayerMovementStateChangedEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> changes spectated player.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangedSpectatorEventArgs"/> instance.</param>
        public virtual void OnChangedSpectator(PlayerChangedSpectatorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> toggles the NoClip mode.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerTogglingNoclipEventArgs"/> instance.</param>
        public virtual void OnTogglingNoClip(PlayerTogglingNoclipEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> toggles the NoClip mode.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerToggledNoclipEventArgs"/> instance.</param>
        public virtual void OnToggledNoClip(PlayerToggledNoclipEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> toggles overwatch.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingOverwatchEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnTogglingOverwatch(object ev) { }

        /// <summary>
        /// Called before turning the radio on/off.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerTogglingRadioEventArgs"/> instance.</param>
        public virtual void OnTogglingRadio(PlayerTogglingRadioEventArgs ev) { }

        /// <summary>
        /// Called after turning the radio on/off.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerToggledRadioEventArgs"/> instance.</param>
        public virtual void OnToggledRadio(PlayerToggledRadioEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> searches a Pickup.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSearchingPickupEventArgs"/> instance.</param>
        public virtual void OnSearchPickupRequest(PlayerSearchingPickupEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> searches a Pickup.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSearchedPickupEventArgs"/> instance.</param>
        public virtual void OnSearchedPickupRequest(PlayerSearchedPickupEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> sends a message inside the admin chat.
        /// </summary>
        /// <param name="ev">The <see cref="SendingAdminChatEventArgs"/> instance.</param>
        public virtual void OnSendingAdminChatMessage(SendingAdminChatEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> sent a message inside the admin chat.
        /// </summary>
        /// <param name="ev">The <see cref="SentAdminChatEventArgs"/> instance.</param>
        public virtual void OnSentAdminChatMessage(SentAdminChatEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="T:Exiled.LabApi.Features.Wrappers.Player" /> has an item added to their inventory.
        /// </summary>
        /// <param name="ev">The <see cref="PickupCreatedEventArgs"/> event handler. </param>
        public virtual void OnPickupCreated(PickupCreatedEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="T:Exiled.LabApi.Features.Wrappers.Player" /> has an item removed from their inventory.
        /// </summary>
        /// <param name="ev">The <see cref="PickupDestroyedEventArgs"/> event handler. </param>
        public virtual void OnPickupDestroyed(PickupDestroyedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> enters in an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerEnteringHazardEventArgs"/> instance. </param>
        public virtual void OnEnteringEnvironmentalHazard(PlayerEnteringHazardEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> enters in an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerEnteredHazardEventArgs"/> instance. </param>
        public virtual void OnEnteredEnvironmentalHazard(PlayerEnteredHazardEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> stays on an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="PlayersStayingInHazardEventArgs"/> instance. </param>
        public virtual void OnStayingOnEnvironmentalHazard(PlayersStayingInHazardEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> exits from an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerLeavingHazardEventArgs"/> instance. </param>
        public virtual void OnExitingEnvironmentalHazard(PlayerLeavingHazardEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> exited from an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerLeftHazardEventArgs"/> instance. </param>
        public virtual void OnExitedEnvironmentalHazard(PlayerLeftHazardEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> damage a window.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDamagingWindowEventArgs"/> instance. </param>
        public virtual void OnPlayerDamageWindow(PlayerDamagingWindowEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> unlocks a generator.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUnlockingGeneratorEventArgs"/> instance. </param>
        public virtual void OnUnlockingGenerator(PlayerUnlockingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> opens a generator.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerOpeningGeneratorEventArgs"/> instance. </param>
        public virtual void OnOpeningGenerator(PlayerOpeningGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> closes a generator.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerClosingGeneratorEventArgs"/> instance. </param>
        public virtual void OnClosingGenerator(PlayerClosingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> turns on the generator by switching lever.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerActivatingGeneratorEventArgs"/> instance. </param>
        public virtual void OnActivatingGenerator(PlayerActivatingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before dropping ammo.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDroppingAmmoEventArgs"/> instance. </param>
        public virtual void OnDroppingAmmo(PlayerDroppingAmmoEventArgs ev) { }

        /// <summary>
        /// Called after dropping ammo.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDroppedAmmoEventArgs"/> instance. </param>
        public virtual void OnDroppedAmmo(PlayerDroppedAmmoEventArgs ev) { }

        /// <summary>
        /// Called before being muted.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerMutingEventArgs"/> instance. </param>
        public virtual void OnIssuingMute(PlayerMutingEventArgs ev) { }

        /// <summary>
        /// Called after being muted.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerMutedEventArgs"/> instance. </param>
        public virtual void OnIssuedMute(PlayerMutedEventArgs ev) { }

        /// <summary>
        /// Called before being unmuted.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUnmutingEventArgs"/> instance. </param>
        public virtual void OnRevokingMute(PlayerUnmutingEventArgs ev) { }

        /// <summary>
        /// Called after being unmuted.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUnmutedEventArgs"/> instance. </param>
        public virtual void OnRevokedMute(PlayerUnmutedEventArgs ev) { }

        /// <summary>
        /// Called before a user's radio preset is changed.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangingRadioRangeEventArgs"/> instance. </param>
        public virtual void OnChangingRadioPreset(PlayerChangingRadioRangeEventArgs ev) { }

        /// <summary>
        /// Called before hurting a player.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerHurtingEventArgs"/> instance. </param>
        public virtual void OnHurting(PlayerHurtingEventArgs ev) { }

        /// <summary>
        /// Called ater a <see cref="LabApi.Features.Wrappers.Player"/> being hurt.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerHurtEventArgs"/> instance. </param>
        public virtual void OnHurt(PlayerHurtEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> is healed.
        /// </summary>
        /// <param name="ev">The <see cref="HealingEventArgs"/> instance. </param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnHealing(object ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/> is healed.
        /// </summary>
        /// <param name="ev">The <see cref="HealedEventArgs"/> instance. </param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnHealed(object ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/> dies.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDyingEventArgs"/> instance. </param>
        public virtual void OnDying(PlayerDyingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="LabApi.Features.Wrappers.Player"/>s custom display name is changed.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangingNicknameEventArgs"/> instance.</param>
        public virtual void OnChangingNickname(PlayerChangingNicknameEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="LabApi.Features.Wrappers.Player"/>s custom display name is changed.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangedNicknameEventArgs"/> instance.</param>
        public virtual void OnChangedNickname(PlayerChangedNicknameEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> jumps.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerJumpedEventArgs"/> instance.</param>
        public virtual void OnPlayerJumped(PlayerJumpedEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> movement state changes.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerMovementStateChangedEventArgs"/> instance.</param>
        public virtual void OnPlayerMovementStateChanged(PlayerMovementStateChangedEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is changing attachments.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangingAttachmentsEventArgs"/> instance.</param>
        public virtual void OnPlayerChangingAttachments(PlayerChangingAttachmentsEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> has changed attachments.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangedAttachmentsEventArgs"/> instance.</param>
        public virtual void OnPlayerChangedAttachments(PlayerChangedAttachmentsEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is sending attachments preferences.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSendingAttachmentsPrefsEventArgs"/> instance.</param>
        public virtual void OnPlayerSendingAttachmentsPrefs(PlayerSendingAttachmentsPrefsEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> has sent attachments preferences.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSentAttachmentsPrefsEventArgs"/> instance.</param>
        public virtual void OnPlayerSentAttachmentsPrefs(PlayerSentAttachmentsPrefsEventArgs ev) { }

        /// <summary>
        /// Called when the server elevator sequence changes.
        /// </summary>
        /// <param name="ev">The <see cref="ElevatorSequenceChangedEventArgs"/> instance.</param>
        public virtual void OnServerElevatorSequenceChanged(ElevatorSequenceChangedEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> interacts with a warhead lever.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractingWarheadLeverEventArgs"/> instance.</param>
        public virtual void OnPlayerInteractingWarheadLever(PlayerInteractingWarheadLeverEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> has interacted with a warhead lever.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInteractedWarheadLeverEventArgs"/> instance.</param>
        public virtual void OnPlayerInteractedWarheadLever(PlayerInteractedWarheadLeverEventArgs ev) { }

        /// <summary>
        /// Gets called when <see cref="LabApi.Features.Wrappers.Player"/> detects enemy player using SCP-1344.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerDetectedByScp1344EventArgs"/> instance.</param>
        public virtual void OnDetectedByScp1344(PlayerDetectedByScp1344EventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 is disguising.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114DisguisingEventArgs"/> instance.</param>
        public virtual void OnScp3114Disguising(Scp3114DisguisingEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 has disguised.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114DisguisedEventArgs"/> instance.</param>
        public virtual void OnScp3114Disguised(Scp3114DisguisedEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 is revealing.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114RevealingEventArgs"/> instance.</param>
        public virtual void OnScp3114Revealing(Scp3114RevealingEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 has revealed.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114RevealedEventArgs"/> instance.</param>
        public virtual void OnScp3114Revealed(Scp3114RevealedEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 starts dancing.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114StartingDanceEventArgs"/> instance.</param>
        public virtual void OnScp3114StartingDancing(Scp3114StartingDanceEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 has started dancing.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114StartedDanceEventArgs"/> instance.</param>
        public virtual void OnScp3114StartedDancing(Scp3114StartedDanceEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is spinning a revolver.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSpinningRevolverEventArgs"/> instance.</param>
        public virtual void OnPlayerSpinningRevolver(PlayerSpinningRevolverEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> has spun a revolver.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerSpinnedRevolverEventArgs"/> instance.</param>
        public virtual void OnPlayerSpunRevolver(PlayerSpinnedRevolverEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> toggles disruptor mode.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerToggledDisruptorFiringModeEventArgs"/> instance.</param>
        public virtual void OnPlayerToggledDisruptorFiringMode(PlayerToggledDisruptorFiringModeEventArgs ev) { }

        /// <summary>
        /// Called when SCP-127 gains experience.
        /// </summary>
        /// <param name="ev">The <see cref="Scp127GainExperienceEventArgs"/> instance.</param>
        public virtual void OnGainingExp(Scp127GainExperienceEventArgs ev) { }

        /// <summary>
        /// Called when SCP-127 has gained experience.
        /// </summary>
        /// <param name="ev">The <see cref="Scp127GainExperienceEventArgs"/> instance.</param>
        public virtual void OnGainedExp(Scp127GainExperienceEventArgs ev) { }

        /// <summary>
        /// Called when SCP-127 is levelling up.
        /// </summary>
        /// <param name="ev">The <see cref="Scp127LevellingUpEventArgs"/> instance.</param>
        public virtual void OnLevellingUp(Scp127LevellingUpEventArgs ev) { }

        /// <summary>
        /// Called when SCP-127 has levelled up.
        /// </summary>
        /// <param name="ev">The <see cref="Scp127LevelUpEventArgs"/> instance.</param>
        public virtual void OnLevelUp(Scp127LevelUpEventArgs ev) { }

        /// <summary>
        /// Called when SCP-127 is talking.
        /// </summary>
        /// <param name="ev">The <see cref="Scp127TalkingEventArgs"/> instance.</param>
        public virtual void OnTalking(Scp127TalkingEventArgs ev) { }

        /// <summary>
        /// Called when SCP-127 has talked.
        /// </summary>
        /// <param name="ev">The <see cref="Scp127TalkedEventArgs"/> instance.</param>
        public virtual void OnTalked(Scp127TalkedEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> badge visibility is changing.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangingBadgeVisibilityEventArgs"/> instance.</param>
        public virtual void OnChangingBadgeVisibility(PlayerChangingBadgeVisibilityEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> badge visibility has changed.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerChangedBadgeVisibilityEventArgs"/> instance.</param>
        public virtual void OnChangedBadgeVisibility(PlayerChangedBadgeVisibilityEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is processing a Jailbird message.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerProcessingJailbirdMessageEventArgs"/> instance.</param>
        public virtual void OnProcessingJailbirdMessage(PlayerProcessingJailbirdMessageEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> has processed a Jailbird message.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerProcessedJailbirdMessageEventArgs"/> instance.</param>
        public virtual void OnProcessedJailbirdMessage(PlayerProcessedJailbirdMessageEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is completing item use.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUsingItemEventArgs"/> instance.</param>
        public virtual void OnUsingItemCompleting(PlayerUsingItemEventArgs ev) { }
        
        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is completing item use.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerUsedItemEventArgs"/> instance.</param>
        public virtual void OnUsedItemCompleting(PlayerUsedItemEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 strangle is aborting.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114StrangleAbortingEventArgs"/> instance.</param>
        public virtual void OnStrangleAborting(Scp3114StrangleAbortingEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 strangle has aborted.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114StrangleAbortedEventArgs"/> instance.</param>
        public virtual void OnStrangleAborted(Scp3114StrangleAbortedEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 strangle is starting.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114StrangleStartingEventArgs"/> instance.</param>
        public virtual void OnStrangleStarting(Scp3114StrangleStartingEventArgs ev) { }

        /// <summary>
        /// Called when SCP-3114 strangle has started.
        /// </summary>
        /// <param name="ev">The <see cref="Scp3114StrangleStartedEventArgs"/> instance.</param>
        public virtual void OnStrangleStarted(Scp3114StrangleStartedEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is inspecting a keycard.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInspectingKeycardEventArgs"/> instance.</param>
        public virtual void OnInspectingKeycard(PlayerInspectingKeycardEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> has inspected a keycard.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerInspectedKeycardEventArgs"/> instance.</param>
        public virtual void OnInspectedKeycard(PlayerInspectedKeycardEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/>' room has changed.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRoomChangedEventArgs"/> instance.</param>
        public virtual void OnRoomChanged(PlayerRoomChangedEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/>' zone has changed.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerZoneChangedEventArgs"/> instance.</param>
        public virtual void OnZoneChanged(PlayerZoneChangedEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is added to the RA player list.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRaPlayerListAddedPlayerEventArgs"/> instance.</param>
        public virtual void OnRaPlayerListAddedPlayer(PlayerRaPlayerListAddedPlayerEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is being added to the RA player list.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRaPlayerListAddingPlayerEventArgs"/> instance.</param>
        public virtual void OnRaPlayerListAddingPlayer(PlayerRaPlayerListAddingPlayerEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> requests custom RA info.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRequestedCustomRaInfoEventArgs"/> instance.</param>
        public virtual void OnRequestedCustomRaInfo(PlayerRequestedCustomRaInfoEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> requests RA player info.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRequestedRaPlayerInfoEventArgs"/> instance.</param>
        public virtual void OnRequestedRaPlayerInfo(PlayerRequestedRaPlayerInfoEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is requesting RA player info.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRequestingRaPlayerInfoEventArgs"/> instance.</param>
        public virtual void OnRequestingRaPlayerInfo(PlayerRequestingRaPlayerInfoEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> requests the RA player list.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRequestedRaPlayerListEventArgs"/> instance.</param>
        public virtual void OnRequestedRaPlayerList(PlayerRequestedRaPlayerListEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is requesting the RA player list.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRequestingRaPlayerListEventArgs"/> instance.</param>
        public virtual void OnRequestingRaPlayerList(PlayerRequestingRaPlayerListEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> requests RA players info.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRequestedRaPlayersInfoEventArgs"/> instance.</param>
        public virtual void OnRequestedRaPlayersInfo(PlayerRequestedRaPlayersInfoEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="LabApi.Features.Wrappers.Player"/> is requesting RA players info.
        /// </summary>
        /// <param name="ev">The <see cref="PlayerRequestingRaPlayersInfoEventArgs"/> instance.</param>
        public virtual void OnRequestingRaPlayersInfo(PlayerRequestingRaPlayersInfoEventArgs ev) { }

        /// <summary>
        /// Called when an objective is completing.
        /// </summary>
        /// <param name="ev">The <see cref="ObjectiveCompletingBaseEventArgs"/> instance.</param>
        public virtual void OnCompleting(ObjectiveCompletingBaseEventArgs ev) { }

        /// <summary>
        /// Called when an objective is completed.
        /// </summary>
        /// <param name="ev">The <see cref="ObjectiveCompletedBaseEventArgs"/> instance.</param>
        public virtual void OnCompleted(ObjectiveCompletedBaseEventArgs ev) { }

        /// <summary>
        /// Called when activating generator objective is completing.
        /// </summary>
        /// <param name="ev">The <see cref="GeneratorActivatingEventArgs"/> instance.</param>
        public virtual void OnActivatingGeneratorCompleting(GeneratorActivatingEventArgs ev) { }

        /// <summary>
        /// Called when activating generator objective is completed.
        /// </summary>
        /// <param name="ev">The <see cref="GeneratorActivatedEventArgs"/> instance.</param>
        public virtual void OnActivatedGeneratorCompleted(GeneratorActivatedEventArgs ev) { }

        /// <summary>
        /// Called when damaging SCP objective is completing.
        /// </summary>
        /// <param name="ev">The <see cref="ScpDamagingObjectiveEventArgs"/> instance.</param>
        public virtual void OnDamagingScpCompleting(ScpDamagingObjectiveEventArgs ev) { }

        /// <summary>
        /// Called when damaging SCP objective is completed.
        /// </summary>
        /// <param name="ev">The <see cref="ScpDamagedObjectiveEventArgs"/> instance.</param>
        public virtual void OnDamagedScpCompleted(ScpDamagedObjectiveEventArgs ev) { }

        /// <summary>
        /// Called when escaping objective is completing.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingObjectiveEventArgs"/> instance.</param>
        public virtual void OnEscapingCompleting(EscapingObjectiveEventArgs ev) { }

        /// <summary>
        /// Called when escaping objective is completed.
        /// </summary>
        /// <param name="ev">The <see cref="EscapedObjectiveEventArgs"/> instance.</param>
        public virtual void OnEscapedCompleted(EscapedObjectiveEventArgs ev) { }

        /// <summary>
        /// Called when killing enemy objective is completing.
        /// </summary>
        /// <param name="ev">The <see cref="EnemyKillingObjectiveEventArgs"/> instance.</param>
        public virtual void OnKillingEnemyCompleting(EnemyKillingObjectiveEventArgs ev) { }

        /// <summary>
        /// Called when killing enemy objective is completed.
        /// </summary>
        /// <param name="ev">The <see cref="EnemyKilledObjectiveEventArgs"/> instance.</param>
        public virtual void OnKilledEnemyCompleted(EnemyKilledObjectiveEventArgs ev) { }

        /// <summary>
        /// Called when picking SCP item objective is completing.
        /// </summary>
        /// <param name="ev">The <see cref="ScpItemPickingObjectiveEventArgs"/> instance.</param>
        public virtual void OnPickingScpItemCompleting(ScpItemPickingObjectiveEventArgs ev) { }

        /// <summary>
        /// Called when picking SCP item objective is completed.
        /// </summary>
        /// <param name="ev">The <see cref="ScpItemPickingObjectiveEventArgs"/> instance.</param>
        public virtual void OnPickedScpItemCompleted(ScpItemPickedObjectiveEventArgs ev) { }
    }
}

