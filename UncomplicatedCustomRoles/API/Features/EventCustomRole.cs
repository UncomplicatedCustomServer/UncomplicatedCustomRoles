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
using System.ComponentModel;
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

        /// <summary>
        /// Invoked when the custom role is spawned
        /// </summary>
        /// <param name="role"></param>
        public virtual void OnSpawned(SummonedCustomRole role)
        { }

        /// <summary>
        /// Called before kicking a <see cref="API.Features.Player"/> from the server.
        /// </summary>
        /// <param name="ev">The <see cref="KickingEventArgs"/> instance.</param>
        public virtual void OnKicking(PlayerKickingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> has been kicked from the server.
        /// </summary>
        /// <param name="ev">The <see cref="KickedEventArgs"/> instance.</param>
        public virtual void OnKicked(PlayerKickedEventArgs ev) { }

        /// <summary>
        /// Called before banning a <see cref="API.Features.Player"/> from the server.
        /// </summary>
        /// <param name="ev">The <see cref="BanningEventArgs"/> instance.</param>
        public virtual void OnBanning(PlayerBanningEventArgs ev) { }

        /// <summary>
        /// Called before a player's danger state changes.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingDangerStateEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnChangingDangerState(object ev) { }

        /// <summary>
        /// Called after a player has been banned from the server.
        /// </summary>
        /// <param name="ev">The <see cref="BannedEventArgs"/> instance.</param>
        public virtual void OnBanned(PlayerBannedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/>  earns an achievement.
        /// </summary>
        /// <param name="ev">The <see cref="EarningAchievementEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnEarningAchievement(object ev) { }

        /// <summary>
        /// Called before using a usable item.
        /// </summary>
        /// <param name="ev">The <see cref="UsingItemEventArgs"/> instance.</param>
        public virtual void OnUsingItem(PlayerUsingItemEventArgs ev) { }

        /// <summary>
        /// Called before completed using of a usable item.
        /// </summary>
        /// <param name="ev">The <see cref="UsingItemEventArgs"/> instance.</param>
        [Obsolete("Only works on EXILED due to the need of a patch, please refer to OnUsedItem")]
        public virtual void OnUsingItemCompleted(object ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> used a <see cref="API.Features.Items.Usable"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="UsedItemEventArgs"/> instance.</param>
        public virtual void OnUsedItem(PlayerUsedItemEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> has stopped the use of a <see cref="API.Features.Items.Usable"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="CancellingItemUseEventArgs"/> instance.</param>
        public virtual void OnCancellingItemUse(PlayerCancellingUsingItemEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> has stopped the use of a <see cref="API.Features.Items.Usable"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="CancelledItemUseEventArgs"/> instance.</param>
        public virtual void OnCancelledItemUse(PlayerCancelledUsingItemEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> interacted with something.
        /// </summary>
        /// <param name="ev">The <see cref="InteractedEventArgs"/> instance.</param>
        [Obsolete("The generic interaction event is not available in LabAPI, please handle every interaction in a separate method.")]
        public virtual void OnInteracted(object ev) { }

        /// <summary>
        /// Called before spawning a <see cref="API.Features.Player"/> ragdoll.
        /// </summary>
        /// <param name="ev">The <see cref="SpawningRagdollEventArgs"/> instance.</param>
        public virtual void OnSpawningRagdoll(PlayerSpawningRagdollEventArgs ev) { }

        /// <summary>
        /// Called after spawning a <see cref="API.Features.Player"/> ragdoll.
        /// </summary>
        /// <param name="ev">The <see cref="SpawnedRagdollEventArgs"/> instance.</param>
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
        /// Called after a <see cref="API.Features.Player"/> has left the server.
        /// </summary>
        /// <param name="ev">The <see cref="LeftEventArgs"/> instance.</param>
        public virtual void OnLeft(PlayerLeftEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> died.
        /// </summary>
        /// <param name="ev">The <see cref="DiedEventArgs"/> instance.</param>
        public virtual void OnDied(PlayerDeathEventArgs ev) { }

        /// <summary>
        /// Called before changing a <see cref="API.Features.Player"/> role.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingRoleEventArgs"/> instance.</param>
        /// <remarks>If <see cref="ChangingRoleEventArgs.IsAllowed"/> is set to <see langword="false"/> when Escape is <see langword="true"/>, tickets will still be given to the escapee's team even though they will 'fail' to escape. Use <see cref="Escaping"/> to block escapes instead.</remarks>
        public virtual void OnChangingRole(PlayerChangingRoleEventArgs ev) { }

        /// <summary>
        /// Called before throwing a grenade.
        /// </summary>
        /// <param name="ev">The <see cref="ThrownProjectileEventArgs"/> instance.</param>
        public virtual void OnThrowingProjectile(PlayerThrowingProjectileEventArgs ev) { }

        /// <summary>
        /// Called after threw a grenade.
        /// </summary>
        /// <param name="ev">The <see cref="ThrownProjectileEventArgs"/> instance.</param>
        public virtual void OnThrewProjectile(PlayerThrewProjectileEventArgs ev) { }

        /// <summary>
        /// Called before receving a throwing request.
        /// </summary>
        /// <param name="ev">The <see cref="ThrowingRequestEventArgs"/> instance.</param>
        [Obsolete("Please refer to OnThrowingItem")]
        public virtual void OnThrowingRequest(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> throws an item.
        /// </summary>
        /// <param name="ev"></param>
        public virtual void OnThrowingItem(PlayerThrowingItemEventArgs ev) { }

        /// <summary>
        /// Called after threw an item.
        /// </summary>
        /// <param name="ev">The <see cref="ThrownProjectileEventArgs"/> instance.</param>
        public virtual void OnThrewItem(PlayerThrewItemEventArgs ev) { }

        /// <summary>
        /// Called before dropping an item.
        /// </summary>
        /// <param name="ev">The <see cref="DroppingItemEventArgs"/> instance.</param>
        public virtual void OnDroppingItem(PlayerDroppingItemEventArgs ev) { }

        /// <summary>
        /// Called after dropping an item.
        /// </summary>
        /// <param name="ev">The <see cref="DroppedItemEventArgs"/> instance.</param>
        public virtual void OnDroppedItem(PlayerDroppedItemEventArgs ev) { }

        /// <summary>
        /// Called before dropping a null item.
        /// </summary>
        /// <param name="ev">The <see cref="DroppingNothingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnDroppingNothing(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> picks up an item.
        /// </summary>
        /// <param name="ev">The <see cref="PickingUpItemEventArgs"/> instance.</param>
        public virtual void OnPickingUpItem(PlayerPickingUpItemEventArgs ev) { }

        /// <summary>
        /// Called before handcuffing a <see cref="API.Features.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="HandcuffingEventArgs"/> instance.</param>
        public virtual void OnHandcuffing(PlayerCuffingEventArgs ev) { }

        /// <summary>
        /// Called after handcuffing a <see cref="API.Features.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="HandcuffingEventArgs"/> instance.</param>
        public virtual void OnHandcuffed(PlayerCuffedEventArgs ev) { }

        /// <summary>
        /// Called before freeing a handcuffed <see cref="API.Features.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="RemovingHandcuffsEventArgs"/> instance.</param>
        public virtual void OnRemovingHandcuffs(PlayerUncuffingEventArgs ev) { }

        /// <summary>
        /// Called after freeing a handcuffed <see cref="API.Features.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="RemovingHandcuffsEventArgs"/> instance.</param>
        public virtual void OnRemovedHandcuffs(PlayerUncuffedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> escapes.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingEventArgs"/> instance.</param>
        public virtual void OnEscaping(PlayerEscapingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> escapes.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingEventArgs"/> instance.</param>
        public virtual void OnEscaped(PlayerEscapedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> begins speaking in the intercom.
        /// </summary>
        /// <param name="ev">The <see cref="IntercomSpeakingEventArgs"/> instance.</param>
        public virtual void OnIntercomSpeaking(PlayerUsingIntercomEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> finished speaking in the intercom.
        /// </summary>
        /// <param name="ev">The <see cref="IntercomSpeakingEventArgs"/> instance.</param>
        public virtual void OnIntercomSpeakingFinished(PlayerUsedIntercomEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> shoots a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="ShotEventArgs"/> instance.</param>
        public virtual void OnShot(PlayerShotWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> shoots a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="ShootingEventArgs"/> instance.</param>
        public virtual void OnShooting(PlayerShootingWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> enters the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringPocketDimensionEventArgs"/> instance.</param>
        public virtual void OnEnteringPocketDimension(PlayerEnteringPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> enters the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringPocketDimensionEventArgs"/> instance.</param>

        public virtual void OnEnteredPocketDimension(PlayerEnteredPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> leaves the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingPocketDimensionEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please see OnLeftPocketDimension")]
        public virtual void OnEscapingPocketDimension(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> leaves the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingPocketDimensionEventArgs"/> instance.</param>
        public virtual void OnLeavingPocketDimension(PlayerLeavingPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> fails to escape the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="FailingEscapePocketDimensionEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please see OnLeftPocketDimension")]
        public virtual void OnFailingEscapePocketDimension(object ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> left the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingPocketDimensionEventArgs"/> instance.</param>
        public virtual void OnLeftPocketDimension(PlayerLeftPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> enters killer collision.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringKillerCollisionEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnEnteringKillerCollision(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> reloads a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="ReloadingWeaponEventArgs"/> instance.</param>
        public virtual void OnReloadingWeapon(PlayerReloadingWeaponEventArgs ev) { }

        /// <summary>
        /// Called before spawning a <see cref="API.Features.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="SpawningEventArgs"/> instance.</param>
        public virtual void OnSpawning(PlayerSpawningEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> has spawned.
        /// </summary>
        /// <param name="ev">The <see cref="SpawnedEventArgs"/> instance.</param>
        public virtual void OnSpawned(PlayerSpawnedEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> held item changes.
        /// </summary>
        /// <param name="ev">The <see cref="ChangedItemEventArgs"/> instance.</param>
        public virtual void OnChangedItem(PlayerChangedItemEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> held item changes.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingItemEventArgs"/> instance.</param>
        public virtual void OnChangingItem(PlayerChangingItemEventArgs ev) { }

        /// <summary>
        /// Called before changing a <see cref="API.Features.Player"/> group.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingGroupEventArgs"/> instance.</param>
        public virtual void OnChangingGroup(PlayerGroupChangingEventArgs ev) { }

        /// <summary>
        /// Called after changing a <see cref="API.Features.Player"/> group.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingGroupEventArgs"/> instance.</param>
        public virtual void OnChangedGroup(PlayerGroupChangedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with an elevator.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingElevatorEventArgs"/> instance.</param>
        public virtual void OnInteractingElevator(PlayerInteractingElevatorEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> interacts with an elevator.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingElevatorEventArgs"/> instance.</param>
        public virtual void OnInteractedElevator(PlayerInteractedElevatorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with a locker.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractingLocker(PlayerInteractingLockerEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> interacts with a locker.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractedLocker(PlayerInteractedLockerEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with a generator.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractingGenerator(PlayerInteractingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> interacts with a generator.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractedGenerator(PlayerInteractedGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with a door.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractingDoor(PlayerInteractingDoorEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> interacts with a door.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractedDoor(PlayerInteractedDoorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with SCP-330.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractingScp330(PlayerInteractingScp330EventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> interacts with SCP-330.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractedScp330(PlayerInteractedScp330EventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with a door.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractingShootingTarget(PlayerInteractingShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> interacts with a door.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractedShootingTarget(PlayerInteractedShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> triggers a tesla.
        /// </summary>
        /// <param name="ev">The <see cref="TriggeringTeslaEventArgs"/> instance.</param>
        public virtual void OnTriggeringTesla(PlayerTriggeringTeslaEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> triggers a tesla.
        /// </summary>
        /// <param name="ev">The <see cref="TriggeringTeslaEventArgs"/> instance.</param>
        public virtual void OnTriggeredTesla(PlayerTriggeredTeslaEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> receives a status effect.
        /// </summary>
        /// <param name="ev">The <see cref="ReceivingEffectEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please refer to OnUpdatedEffect")]
        public virtual void OnReceivingEffect(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> receives a status effect.
        /// </summary>
        /// <param name="ev"></param>
        public virtual void OnUpdatingEffect(PlayerEffectUpdatingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> receives a status effect.
        /// </summary>
        /// <param name="ev"></param>
        public virtual void OnUpdatedEffect(PlayerEffectUpdatedEventArgs ev) { }

        /// <summary>
        /// Called before a user's radio battery charge is changed.
        /// </summary>
        /// <param name="ev">The <see cref="UsingRadioBatteryEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please refer to OnUsingRadio")]
        public virtual void OnUsingRadioBattery(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> uses a Radio.
        /// </summary>
        /// <param name="ev"></param>
        public virtual void OnUsingRadio(PlayerUsingRadioEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> MicroHID state is changed.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingMicroHIDStateEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnChangingMicroHIDState(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> MicroHID energy is changed.
        /// </summary>
        /// <param name="ev">The <see cref="UsingMicroHIDEnergyEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnUsingMicroHIDEnergy(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> damages a shooting target.
        /// </summary>
        /// <param name="ev">The <see cref="DamagingShootingTargetEventArgs"/> instance.</param>
        public virtual void OnDamagingShootingTarget(PlayerDamagingShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> damages a shooting target.
        /// </summary>
        /// <param name="ev">The <see cref="DamagingShootingTargetEventArgs"/> instance.</param>
        public virtual void OnDamagedShootingTarget(PlayerDamagedShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> flips a coin.
        /// </summary>
        /// <param name="ev">The <see cref="FlippingCoinEventArgs"/> instance.</param>
        public virtual void OnFlippingCoin(PlayerFlippingCoinEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> flips a coin.
        /// </summary>
        /// <param name="ev">The <see cref="FlippingCoinEventArgs"/> instance.</param>
        public virtual void OnFlippedCoin(PlayerFlippedCoinEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> toggles the flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingFlashlightEventArgs"/> instance.</param>
        public virtual void OnTogglingFlashlight(PlayerTogglingFlashlightEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> toggles the flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingFlashlightEventArgs"/> instance.</param>
        public virtual void OnToggledFlashlight(PlayerToggledFlashlightEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> unloads a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="UnloadingWeaponEventArgs"/> instance.</param>
        public virtual void OnUnloadingWeapon(PlayerUnloadingWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> unloads a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="UnloadingWeaponEventArgs"/> instance.</param>
        public virtual void OnUnloadedWeapon(PlayerUnloadedWeaponEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> triggers an aim action.
        /// </summary>
        /// <param name="ev">The <see cref="AimingDownSightEventArgs"/> instance.</param>
        public virtual void OnAimingDownSight(PlayerAimedWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> toggles the weapon's flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingWeaponFlashlightEventArgs"/> instance.</param>
        public virtual void OnTogglingWeaponFlashlight(PlayerTogglingWeaponFlashlightEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> toggles the weapon's flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingWeaponFlashlightEventArgs"/> instance.</param>
        public virtual void OnToggledWeaponFlashlight(PlayerToggledWeaponFlashlightEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> dryfires a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="DryfiringWeaponEventArgs"/> instance.</param>
        public virtual void OnDryfiringWeapon(PlayerDryFiringWeaponEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> dryfires a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="DryfiringWeaponEventArgs"/> instance.</param>
        public virtual void OnDryfiredWeapon(PlayerDryFiredWeaponEventArgs ev) { }

        /// <summary>
        /// Invoked after a <see cref="API.Features.Player"/> presses the voicechat key.
        /// </summary>
        /// <param name="ev">The <see cref="VoiceChattingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please refer to OnSendingVoiceMessage")]
        public virtual void OnVoiceChatting(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> sends a Voice Message.
        /// </summary>
        /// <param name="ev"></param>
        public virtual void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> receives a Voice Message.
        /// </summary>
        /// <param name="ev"></param>
        public virtual void OnReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> makes noise.
        /// </summary>
        /// <param name="ev">The <see cref="MakingNoiseEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnMakingNoise(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> jumps.
        /// </summary>
        /// <param name="ev">The <see cref="JumpingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnJumping(object ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> lands.
        /// </summary>
        /// <param name="ev">The <see cref="LandingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnLanding(object ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> presses the transmission key.
        /// </summary>
        /// <param name="ev">The <see cref="TransmittingEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI, please refer to OnUsingRadio")]
        public virtual void OnTransmitting(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> changes move state.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingMoveStateEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnChangingMoveState(object ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> changes spectated player.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingSpectatedPlayerEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnChangingSpectatedPlayer(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> toggles the NoClip mode.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingNoClipEventArgs"/> instance.</param>
        public virtual void OnTogglingNoClip(PlayerTogglingNoclipEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> toggles the NoClip mode.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingNoClipEventArgs"/> instance.</param>
        public virtual void OnToggledNoClip(PlayerToggledNoclipEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> toggles overwatch.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingOverwatchEventArgs"/> instance.</param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnTogglingOverwatch(object ev) { }

        /// <summary>
        /// Called before turning the radio on/off.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingRadioEventArgs"/> instance.</param>
        public virtual void OnTogglingRadio(PlayerTogglingRadioEventArgs ev) { }

        /// <summary>
        /// Called after turning the radio on/off.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingRadioEventArgs"/> instance.</param>
        public virtual void OnToggledRadio(PlayerToggledRadioEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> searches a Pickup.
        /// </summary>
        /// <param name="ev">The <see cref="SearchingPickupEventArgs"/> instance.</param>
        public virtual void OnSearchPickupRequest(PlayerSearchingPickupEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> searches a Pickup.
        /// </summary>
        /// <param name="ev">The <see cref="SearchingPickupEventArgs"/> instance.</param>
        public virtual void OnSearchedPickupRequest(PlayerSearchedPickupEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> sends a message inside the admin chat.
        /// </summary>
        /// <param name="ev">The <see cref="SendingAdminChatMessageEventsArgs"/> instance.</param>
        public virtual void OnSendingAdminChatMessage(SendingAdminChatEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> sent a message inside the admin chat.
        /// </summary>
        /// <param name="ev">The <see cref="SendingAdminChatMessageEventsArgs"/> instance.</param>
        public virtual void OnSentAdminChatMessage(SentAdminChatEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="T:Exiled.API.Features.Player" /> has an item added to their inventory.
        /// </summary>
        /// <param name="ev">The <see cref="ItemAddedEventArgs"/> event handler. </param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnItemAdded(object ev) { }

        /// <summary>
        /// Called after a <see cref="T:Exiled.API.Features.Player" /> has an item removed from their inventory.
        /// </summary>
        /// <param name="ev">The <see cref="ItemRemovedEventArgs"/> event handler. </param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnItemRemoved(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> enters in an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringEnvironmentalHazardEventArgs"/> instance. </param>
        public virtual void OnEnteringEnvironmentalHazard(PlayerEnteringHazardEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> enters in an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringEnvironmentalHazardEventArgs"/> instance. </param>
        public virtual void OnEnteredEnvironmentalHazard(PlayerEnteredHazardEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="API.Features.Player"/> stays on an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="StayingOnEnvironmentalHazardEventArgs"/> instance. </param>
        public virtual void OnStayingOnEnvironmentalHazard(PlayersStayingInHazardEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> exits from an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="ExitingEnvironmentalHazardEventArgs"/> instance. </param>
        public virtual void OnExitingEnvironmentalHazard(PlayerLeavingHazardEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> exited from an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="ExitingEnvironmentalHazardEventArgs"/> instance. </param>
        public virtual void OnExitedEnvironmentalHazard(PlayerLeftHazardEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> damage a window.
        /// </summary>
        /// <param name="ev">The <see cref="DamagingWindowEventArgs"/> instance. </param>
        public virtual void OnPlayerDamageWindow(PlayerDamagingWindowEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> unlocks a generator.
        /// </summary>
        /// <param name="ev">The <see cref="UnlockingGeneratorEventArgs"/> instance. </param>
        public virtual void OnUnlockingGenerator(PlayerUnlockingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> opens a generator.
        /// </summary>
        /// <param name="ev">The <see cref="OpeningGeneratorEventArgs"/> instance. </param>
        public virtual void OnOpeningGenerator(PlayerOpeningGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> closes a generator.
        /// </summary>
        /// <param name="ev">The <see cref="ClosingGeneratorEventArgs"/> instance. </param>
        public virtual void OnClosingGenerator(PlayerClosingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> turns on the generator by switching lever.
        /// </summary>
        /// <param name="ev">The <see cref="ActivatingGeneratorEventArgs"/> instance. </param>
        public virtual void OnActivatingGenerator(PlayerActivatingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> turns off the generator by switching lever.
        /// </summary>
        /// <param name="ev">The <see cref="StoppingGeneratorEventArgs"/> instance. </param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnStoppingGenerator(object ev) { }

        /// <summary>
        /// Called before dropping ammo.
        /// </summary>
        /// <param name="ev">The <see cref="DroppingAmmoEventArgs"/> instance. </param>
        public virtual void OnDroppingAmmo(PlayerDroppingAmmoEventArgs ev) { }

        /// <summary>
        /// Called after dropping ammo.
        /// </summary>
        /// <param name="ev">The <see cref="DroppedAmmoEventArgs"/> instance. </param>
        public virtual void OnDroppedAmmo(PlayerDroppedAmmoEventArgs ev) { }

        /// <summary>
        /// Called before being muted.
        /// </summary>
        /// <param name="ev">The <see cref="IssuingMuteEventArgs"/> instance. </param>
        public virtual void OnIssuingMute(PlayerMutingEventArgs ev) { }

        /// <summary>
        /// Called after being muted.
        /// </summary>
        /// <param name="ev">The <see cref="IssuingMuteEventArgs"/> instance. </param>
        public virtual void OnIssuedMute(PlayerMutedEventArgs ev) { }

        /// <summary>
        /// Called before being unmuted.
        /// </summary>
        /// <param name="ev">The <see cref="RevokingMuteEventArgs"/> instance. </param>
        public virtual void OnRevokingMute(PlayerUnmutingEventArgs ev) { }

        /// <summary>
        /// Called after being unmuted.
        /// </summary>
        /// <param name="ev">The <see cref="RevokingMuteEventArgs"/> instance. </param>
        public virtual void OnRevokedMute(PlayerUnmutedEventArgs ev) { }

        /// <summary>
        /// Called before a user's radio preset is changed.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingRadioPresetEventArgs"/> instance. </param>
        public virtual void OnChangingRadioPreset(PlayerChangingRadioRangeEventArgs ev) { }

        /// <summary>
        /// Called before hurting a player.
        /// </summary>
        /// <param name="ev">The <see cref="HurtingEventArgs"/> instance. </param>
        public virtual void OnHurting(PlayerHurtingEventArgs ev) { }

        /// <summary>
        /// Called ater a <see cref="API.Features.Player"/> being hurt.
        /// </summary>
        /// <param name="ev">The <see cref="HurtingEventArgs"/> instance. </param>
        public virtual void OnHurt(PlayerHurtEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> is healed.
        /// </summary>
        /// <param name="ev">The <see cref="HealingEventArgs"/> instance. </param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnHealing(object ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> is healed.
        /// </summary>
        /// <param name="ev">The <see cref="HealedEventArgs"/> instance. </param>
        [Obsolete("Not available on LabAPI")]
        public virtual void OnHealed(object ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> dies.
        /// </summary>
        /// <param name="ev">The <see cref="DyingEventArgs"/> instance. </param>
        public virtual void OnDying(PlayerDyingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="Player"/>'s custom display name is changed.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingNicknameEventArgs"/> instance.</param>
        public virtual void OnChangingNickname(PlayerChangingNicknameEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="Player"/>'s custom display name is changed.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingNicknameEventArgs"/> instance.</param>
        public virtual void OnChangedNickname(PlayerChangedNicknameEventArgs ev) { }
    }
}