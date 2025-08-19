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
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System.Collections.Generic;
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
        public virtual List<object>? CustomFlags { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the custom role should be evaluated during normal spawn events or not
        /// </summary>
        public virtual bool IgnoreSpawnSystem { get; set; } = false;

        /// <summary>
        /// Invoked when the Custom Role is spawned
        /// </summary>
        /// <param name="role"></param>
        public virtual void OnSpawned(SummonedCustomRole role)
        { }

        /// <summary>
        /// Invoked when the Custom Role is removed
        /// </summary>
        /// <param name="role"></param>
        public virtual void OnRemoved(SummonedCustomRole role)
        { }

        /// <summary>
        /// Called before kicking a <see cref="API.Features.Player"/> from the server.
        /// </summary>
        /// <param name="ev">The <see cref="KickingEventArgs"/> instance.</param>
        public virtual void OnKicking(KickingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> has been kicked from the server.
        /// </summary>
        /// <param name="ev">The <see cref="KickedEventArgs"/> instance.</param>
        public virtual void OnKicked(KickedEventArgs ev) { }

        /// <summary>
        /// Called before banning a <see cref="API.Features.Player"/> from the server.
        /// </summary>
        /// <param name="ev">The <see cref="BanningEventArgs"/> instance.</param>
        public virtual void OnBanning(BanningEventArgs ev) { }

        /// <summary>
        /// Called before a player's danger state changes.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingDangerStateEventArgs"/> instance.</param>
        public virtual void OnChangingDangerState(ChangingDangerStateEventArgs ev) { }

        /// <summary>
        /// Called after a player has been banned from the server.
        /// </summary>
        /// <param name="ev">The <see cref="BannedEventArgs"/> instance.</param>
        public virtual void OnBanned(BannedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/>  earns an achievement.
        /// </summary>
        /// <param name="ev">The <see cref="EarningAchievementEventArgs"/> instance.</param>
        public virtual void OnEarningAchievement(EarningAchievementEventArgs ev) { }

        /// <summary>
        /// Called before using a usable item.
        /// </summary>
        /// <param name="ev">The <see cref="UsingItemEventArgs"/> instance.</param>
        public virtual void OnUsingItem(UsingItemEventArgs ev) { }

        /// <summary>
        /// Called before completed using of a usable item.
        /// </summary>
        /// <param name="ev">The <see cref="UsingItemEventArgs"/> instance.</param>
        public virtual void OnUsingItemCompleted(UsingItemCompletedEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> used a <see cref="API.Features.Items.Usable"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="UsedItemEventArgs"/> instance.</param>
        public virtual void OnUsedItem(UsedItemEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> has stopped the use of a <see cref="API.Features.Items.Usable"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="CancellingItemUseEventArgs"/> instance.</param>
        public virtual void OnCancellingItemUse(CancellingItemUseEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> has stopped the use of a <see cref="API.Features.Items.Usable"/> item.
        /// </summary>
        /// <param name="ev">The <see cref="CancelledItemUseEventArgs"/> instance.</param>
        public virtual void OnCancelledItemUse(CancelledItemUseEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> interacted with something.
        /// </summary>
        /// <param name="ev">The <see cref="InteractedEventArgs"/> instance.</param>
        public virtual void OnInteracted(InteractedEventArgs ev) { }

        /// <summary>
        /// Called before spawning a <see cref="API.Features.Player"/> ragdoll.
        /// </summary>
        /// <param name="ev">The <see cref="SpawningRagdollEventArgs"/> instance.</param>
        public virtual void OnSpawningRagdoll(SpawningRagdollEventArgs ev) { }

        /// <summary>
        /// Called after spawning a <see cref="API.Features.Player"/> ragdoll.
        /// </summary>
        /// <param name="ev">The <see cref="SpawnedRagdollEventArgs"/> instance.</param>
        public virtual void OnSpawnedRagdoll(SpawnedRagdollEventArgs ev) { }

        /// <summary>
        /// Called before activating the warhead panel.
        /// </summary>
        /// <param name="ev">The <see cref="ActivatingWarheadPanelEventArgs"/> instance.</param>
        public virtual void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev) { }

        /// <summary>
        /// Called before activating a workstation.
        /// </summary>
        /// <param name="ev">The <see cref="ActivatingWorkstation"/> instance.</param>
        public virtual void OnActivatingWorkstation(ActivatingWorkstationEventArgs ev) { }

        /// <summary>
        /// Called before deactivating a workstation.
        /// </summary>
        /// <param name="ev">The <see cref="DeactivatingWorkstationEventArgs"/> instance.</param>
        public virtual void OnDeactivatingWorkstation(DeactivatingWorkstationEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> has left the server.
        /// </summary>
        /// <param name="ev">The <see cref="LeftEventArgs"/> instance.</param>
        public virtual void OnLeft(LeftEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> died.
        /// </summary>
        /// <param name="ev">The <see cref="DiedEventArgs"/> instance.</param>
        public virtual void OnDied(DiedEventArgs ev) { }

        /// <summary>
        /// Called before changing a <see cref="API.Features.Player"/> role.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingRoleEventArgs"/> instance.</param>
        /// <remarks>If <see cref="ChangingRoleEventArgs.IsAllowed"/> is set to <see langword="false"/> when Escape is <see langword="true"/>, tickets will still be given to the escapee's team even though they will 'fail' to escape. Use <see cref="Escaping"/> to block escapes instead.</remarks>
        public virtual void OnChangingRole(ChangingRoleEventArgs ev) { }

        /// <summary>
        /// Called before throwing a grenade.
        /// </summary>
        /// <param name="ev">The <see cref="ThrownProjectileEventArgs"/> instance.</param>
        public virtual void OnThrowingProjectile(ThrownProjectileEventArgs ev) { }

        /// <summary>
        /// Called before receving a throwing request.
        /// </summary>
        /// <param name="ev">The <see cref="ThrowingRequestEventArgs"/> instance.</param>
        public virtual void OnThrowingRequest(ThrowingRequestEventArgs ev) { }

        /// <summary>
        /// Called before dropping an item.
        /// </summary>
        /// <param name="ev">The <see cref="DroppingItemEventArgs"/> instance.</param>
        public virtual void OnDroppingItem(DroppingItemEventArgs ev) { }

        /// <summary>
        /// Called after dropping an item.
        /// </summary>
        /// <param name="ev">The <see cref="DroppedItemEventArgs"/> instance.</param>
        public virtual void OnDroppedItem(DroppedItemEventArgs ev) { }

        /// <summary>
        /// Called before dropping a null item.
        /// </summary>
        /// <param name="ev">The <see cref="DroppingNothingEventArgs"/> instance.</param>
        public virtual void OnDroppingNothing(DroppingNothingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> picks up an item.
        /// </summary>
        /// <param name="ev">The <see cref="PickingUpItemEventArgs"/> instance.</param>
        public virtual void OnPickingUpItem(PickingUpItemEventArgs ev) { }

        /// <summary>
        /// Called before handcuffing a <see cref="API.Features.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="HandcuffingEventArgs"/> instance.</param>
        public virtual void OnHandcuffing(HandcuffingEventArgs ev) { }

        /// <summary>
        /// Called before freeing a handcuffed <see cref="API.Features.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="RemovingHandcuffsEventArgs"/> instance.</param>
        public virtual void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> escapes.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingEventArgs"/> instance.</param>
        public virtual void OnEscaping(EscapingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> begins speaking to the intercom.
        /// </summary>
        /// <param name="ev">The <see cref="IntercomSpeakingEventArgs"/> instance.</param>
        public virtual void OnIntercomSpeaking(IntercomSpeakingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> shoots a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="ShotEventArgs"/> instance.</param>
        public virtual void OnShot(ShotEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> shoots a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="ShootingEventArgs"/> instance.</param>
        public virtual void OnShooting(ShootingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> enters the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringPocketDimensionEventArgs"/> instance.</param>
        public virtual void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> escapes the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="EscapingPocketDimensionEventArgs"/> instance.</param>
        public virtual void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> fails to escape the pocket dimension.
        /// </summary>
        /// <param name="ev">The <see cref="FailingEscapePocketDimensionEventArgs"/> instance.</param>
        public virtual void OnFailingEscapePocketDimension(FailingEscapePocketDimensionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> enters killer collision.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringKillerCollisionEventArgs"/> instance.</param>
        public virtual void OnEnteringKillerCollision(EnteringKillerCollisionEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> reloads a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="ReloadingWeaponEventArgs"/> instance.</param>
        public virtual void OnReloadingWeapon(ReloadingWeaponEventArgs ev) { }

        /// <summary>
        /// Called before spawning a <see cref="API.Features.Player"/>.
        /// </summary>
        /// <param name="ev">The <see cref="SpawningEventArgs"/> instance.</param>
        public virtual void OnSpawning(SpawningEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> has spawned.
        /// </summary>
        /// <param name="ev">The <see cref="SpawnedEventArgs"/> instance.</param>
        public virtual void OnSpawned(SpawnedEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> held item changes.
        /// </summary>
        /// <param name="ev">The <see cref="ChangedItemEventArgs"/> instance.</param>
        public virtual void OnChangedItem(ChangedItemEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> held item changes.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingItemEventArgs"/> instance.</param>
        public virtual void OnChangingItem(ChangingItemEventArgs ev) { }

        /// <summary>
        /// Called before changing a <see cref="API.Features.Player"/> group.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingGroupEventArgs"/> instance.</param>
        public virtual void OnChangingGroup(ChangingGroupEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with an elevator.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingElevatorEventArgs"/> instance.</param>
        public virtual void OnInteractingElevator(InteractingElevatorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with a locker.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingLockerEventArgs"/> instance.</param>
        public virtual void OnInteractingLocker(InteractingLockerEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> triggers a tesla.
        /// </summary>
        /// <param name="ev">The <see cref="TriggeringTeslaEventArgs"/> instance.</param>
        public virtual void OnTriggeringTesla(TriggeringTeslaEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> receives a status effect.
        /// </summary>
        /// <param name="ev">The <see cref="ReceivingEffectEventArgs"/> instance.</param>
        public virtual void OnReceivingEffect(ReceivingEffectEventArgs ev) { }

        /// <summary>
        /// Called before a user's radio battery charge is changed.
        /// </summary>
        /// <param name="ev">The <see cref="UsingRadioBatteryEventArgs"/> instance.</param>
        public virtual void OnUsingRadioBattery(UsingRadioBatteryEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> MicroHID state is changed.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingMicroHIDStateEventArgs"/> instance.</param>
        public virtual void OnChangingMicroHIDState(ChangingMicroHIDStateEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> MicroHID energy is changed.
        /// </summary>
        /// <param name="ev">The <see cref="UsingMicroHIDEnergyEventArgs"/> instance.</param>
        public virtual void OnUsingMicroHIDEnergy(UsingMicroHIDEnergyEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with a shooting target.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingShootingTargetEventArgs"/> instance.</param>
        public virtual void OnInteractingShootingTarget(InteractingShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> damages a shooting target.
        /// </summary>
        /// <param name="ev">The <see cref="DamagingShootingTargetEventArgs"/> instance.</param>
        public virtual void OnDamagingShootingTarget(DamagingShootingTargetEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> flips a coin.
        /// </summary>
        /// <param name="ev">The <see cref="FlippingCoinEventArgs"/> instance.</param>
        public virtual void OnFlippingCoin(FlippingCoinEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> toggles the flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingFlashlightEventArgs"/> instance.</param>
        public virtual void OnTogglingFlashlight(TogglingFlashlightEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> unloads a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="UnloadingWeaponEventArgs"/> instance.</param>
        public virtual void OnUnloadingWeapon(UnloadingWeaponEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> triggers an aim action.
        /// </summary>
        /// <param name="ev">The <see cref="AimingDownSightEventArgs"/> instance.</param>
        public virtual void OnAimingDownSight(AimingDownSightEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> toggles the weapon's flashlight.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingWeaponFlashlightEventArgs"/> instance.</param>
        public virtual void OnTogglingWeaponFlashlight(TogglingWeaponFlashlightEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> dryfires a weapon.
        /// </summary>
        /// <param name="ev">The <see cref="DryfiringWeaponEventArgs"/> instance.</param>
        public virtual void OnDryfiringWeapon(DryfiringWeaponEventArgs ev) { }

        /// <summary>
        /// Invoked after a <see cref="API.Features.Player"/> presses the voicechat key.
        /// </summary>
        /// <param name="ev">The <see cref="VoiceChattingEventArgs"/> instance.</param>
        public virtual void OnVoiceChatting(VoiceChattingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> makes noise.
        /// </summary>
        /// <param name="ev">The <see cref="MakingNoiseEventArgs"/> instance.</param>
        public virtual void OnMakingNoise(MakingNoiseEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> jumps.
        /// </summary>
        /// <param name="ev">The <see cref="JumpingEventArgs"/> instance.</param>
        public virtual void OnJumping(JumpingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> lands.
        /// </summary>
        /// <param name="ev">The <see cref="LandingEventArgs"/> instance.</param>
        public virtual void OnLanding(LandingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> presses the transmission key.
        /// </summary>
        /// <param name="ev">The <see cref="TransmittingEventArgs"/> instance.</param>
        public virtual void OnTransmitting(TransmittingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> changes move state.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingMoveStateEventArgs"/> instance.</param>
        public virtual void OnChangingMoveState(ChangingMoveStateEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> changes spectated player.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingSpectatedPlayerEventArgs"/> instance.</param>
        public virtual void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> toggles the NoClip mode.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingNoClipEventArgs"/> instance.</param>
        public virtual void OnTogglingNoClip(TogglingNoClipEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> toggles overwatch.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingOverwatchEventArgs"/> instance.</param>
        public virtual void OnTogglingOverwatch(TogglingOverwatchEventArgs ev) { }

        /// <summary>
        /// Called before turning the radio on/off.
        /// </summary>
        /// <param name="ev">The <see cref="TogglingRadioEventArgs"/> instance.</param>
        public virtual void OnTogglingRadio(TogglingRadioEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> searches a Pickup.
        /// </summary>
        /// <param name="ev">The <see cref="SearchingPickupEventArgs"/> instance.</param>
        public virtual void OnSearchPickupRequest(SearchingPickupEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> searches a Pickup.
        /// </summary>
        /// <param name="ev">The <see cref="SendingAdminChatMessageEventsArgs"/> instance.</param>
        public virtual void OnSendingAdminChatMessage(SendingAdminChatMessageEventsArgs ev) { }

        /// <summary>
        /// Called after a <see cref="T:Exiled.API.Features.Player" /> has an item added to their inventory.
        /// </summary>
        /// <param name="ev">The <see cref="ItemAddedEventArgs"/> event handler. </param>
        public virtual void OnItemAdded(ItemAddedEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="T:Exiled.API.Features.Player" /> has an item removed from their inventory.
        /// </summary>
        /// <param name="ev">The <see cref="ItemRemovedEventArgs"/> event handler. </param>
        public virtual void OnItemRemoved(ItemRemovedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> enters in an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="EnteringEnvironmentalHazardEventArgs"/> instance. </param>
        public virtual void OnEnteringEnvironmentalHazard(EnteringEnvironmentalHazardEventArgs ev) { }

        /// <summary>
        /// Called when a <see cref="API.Features.Player"/> stays on an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="StayingOnEnvironmentalHazardEventArgs"/> instance. </param>
        public virtual void OnStayingOnEnvironmentalHazard(StayingOnEnvironmentalHazardEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> exits from an environmental hazard.
        /// </summary>
        /// <param name="ev">The <see cref="ExitingEnvironmentalHazardEventArgs"/> instance. </param>
        public virtual void OnExitingEnvironmentalHazard(ExitingEnvironmentalHazardEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> damage a window.
        /// </summary>
        /// <param name="ev">The <see cref="DamagingWindowEventArgs"/> instance. </param>
        public virtual void OnPlayerDamageWindow(DamagingWindowEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> unlocks a generator.
        /// </summary>
        /// <param name="ev">The <see cref="UnlockingGeneratorEventArgs"/> instance. </param>
        public virtual void OnUnlockingGenerator(UnlockingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> opens a generator.
        /// </summary>
        /// <param name="ev">The <see cref="OpeningGeneratorEventArgs"/> instance. </param>
        public virtual void OnOpeningGenerator(OpeningGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> closes a generator.
        /// </summary>
        /// <param name="ev">The <see cref="ClosingGeneratorEventArgs"/> instance. </param>
        public virtual void OnClosingGenerator(ClosingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> turns on the generator by switching lever.
        /// </summary>
        /// <param name="ev">The <see cref="ActivatingGeneratorEventArgs"/> instance. </param>
        public virtual void OnActivatingGenerator(ActivatingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> turns off the generator by switching lever.
        /// </summary>
        /// <param name="ev">The <see cref="StoppingGeneratorEventArgs"/> instance. </param>
        public virtual void OnStoppingGenerator(StoppingGeneratorEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> interacts with a door.
        /// </summary>
        /// <param name="ev">The <see cref="InteractingDoorEventArgs"/> instance. </param>
        public virtual void OnInteractingDoor(InteractingDoorEventArgs ev) { }

        /// <summary>
        /// Called before dropping ammo.
        /// </summary>
        /// <param name="ev">The <see cref="DroppingAmmoEventArgs"/> instance. </param>
        public virtual void OnDroppingAmmo(DroppingAmmoEventArgs ev) { }

        /// <summary>
        /// Called after dropping ammo.
        /// </summary>
        /// <param name="ev">The <see cref="DroppedAmmoEventArgs"/> instance. </param>
        public virtual void OnDroppedAmmo(DroppedAmmoEventArgs ev) { }

        /// <summary>
        /// Called before muting a user.
        /// </summary>
        /// <param name="ev">The <see cref="IssuingMuteEventArgs"/> instance. </param>
        public virtual void OnIssuingMute(IssuingMuteEventArgs ev) { }

        /// <summary>
        /// Called before unmuting a user.
        /// </summary>
        /// <param name="ev">The <see cref="RevokingMuteEventArgs"/> instance. </param>
        public virtual void OnRevokingMute(RevokingMuteEventArgs ev) { }

        /// <summary>
        /// Called before a user's radio preset is changed.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingRadioPresetEventArgs"/> instance. </param>
        public virtual void OnChangingRadioPreset(ChangingRadioPresetEventArgs ev) { }

        /// <summary>
        /// Called before hurting a player.
        /// </summary>
        /// <param name="ev">The <see cref="HurtingEventArgs"/> instance. </param>
        public virtual void OnHurting(HurtingEventArgs ev) { }

        /// <summary>
        /// Called ater a <see cref="API.Features.Player"/> being hurt.
        /// </summary>
        /// <param name="ev">The <see cref="HurtingEventArgs"/> instance. </param>
        public virtual void OnHurt(HurtEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> is healed.
        /// </summary>
        /// <param name="ev">The <see cref="HealingEventArgs"/> instance. </param>
        public virtual void OnHealing(HealingEventArgs ev) { }

        /// <summary>
        /// Called after a <see cref="API.Features.Player"/> is healed.
        /// </summary>
        /// <param name="ev">The <see cref="HealedEventArgs"/> instance. </param>
        public virtual void OnHealed(HealedEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="API.Features.Player"/> dies.
        /// </summary>
        /// <param name="ev">The <see cref="DyingEventArgs"/> instance. </param>
        public virtual void OnDying(DyingEventArgs ev) { }

        /// <summary>
        /// Called before a <see cref="Player"/>'s custom display name is changed.
        /// </summary>
        /// <param name="ev">The <see cref="ChangingNicknameEventArgs"/> instance.</param>
        public virtual void OnChangingNickname(ChangingNicknameEventArgs ev) { }
    }
}
