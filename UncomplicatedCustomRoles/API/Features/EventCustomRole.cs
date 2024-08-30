using PlayerRoles;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Events.Args;
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
        public virtual CustomFlags? CustomFlags { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the custom role should be evaluated during normal spawn events or not
        /// </summary>
        public virtual bool IgnoreSpawnSystem { get; set; } = false;

        [PluginEvent(ServerEventType.PlayerKicked)]
        public virtual void OnKicked(PlayerKickedEvent ev) { }

        [PluginEvent(ServerEventType.PlayerBanned)]
        public virtual void OnBanned(PlayerBannedEvent ev) { }

        [PluginEvent(ServerEventType.PlayerUseItem)]
        public virtual void OnUsingItem(PlayerUseItemEvent ev) { }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public virtual void OnUsedItem(PlayerUsedItemEvent ev) { }

        [PluginEvent(ServerEventType.PlayerCancelUsingItem)]
        public virtual void OnCancelledItemUse(PlayerCancelUsingItemEvent ev) { }

        [PluginEvent(ServerEventType.RagdollSpawn)]
        public virtual void OnSpawnedRagdoll(RagdollSpawnEvent ev) { }

        [PluginEvent(ServerEventType.PlayerLeft)]
        public virtual void OnLeft(PlayerLeftEvent ev) { }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public virtual void OnDied(PlayerDeathEvent ev) { }

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        public virtual void OnChangingRole(PlayerChangeRoleEvent ev) { }

        [PluginEvent(ServerEventType.PlayerThrowProjectile)]
        public virtual void OnThrowedProjectile(PlayerThrowProjectileEvent ev) { }

        [PluginEvent(ServerEventType.PlayerThrowItem)]
        public virtual void OnThrowedItem(PlayerThrowItemEvent ev) { }

        [PluginEvent(ServerEventType.PlayerDropItem)]
        public virtual void OnDroppedItem(PlayerDropItemEvent ev) { }

        [PluginEvent(ServerEventType.PlayerSearchPickup)]
        public virtual void OnPickingUpItem(PlayerSearchPickupEvent ev) { }

        [PluginEvent(ServerEventType.PlayerSearchedPickup)]
        public virtual void OnPickedUpItem(PlayerSearchedPickupEvent ev) { }

        [PluginEvent(ServerEventType.PlayerHandcuff)]
        public virtual void OnHandcuffed(PlayerHandcuffEvent ev) { }

        [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
        public virtual void OnRemovedHandcuffs(PlayerRemoveHandcuffsEvent ev) { }

        [PluginEvent(ServerEventType.PlayerEscape)]
        public virtual void OnEscaped(PlayerEscapeEvent ev) { }

        [PluginEvent(ServerEventType.PlayerUsingIntercom)]
        public virtual void OnIntercomSpeaking(PlayerUsingIntercomEvent ev) { }

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        public virtual void OnShot(PlayerShotWeaponEvent ev) { }

        [PluginEvent(ServerEventType.PlayerEnterPocketDimension)]
        public virtual void OnEnteredPocketDimension(PlayerEnterPocketDimensionEvent ev) { }

        [PluginEvent(ServerEventType.PlayerExitPocketDimension)]
        public virtual void OnExitedPocketDimension(PlayerExitPocketDimensionEvent ev) { }

        [PluginEvent(ServerEventType.PlayerReloadWeapon)]
        public virtual void OnReloadedWeapon(PlayerReloadWeaponEvent ev) { }

        [PluginEvent(ServerEventType.PlayerSpawn)]
        public virtual void OnSpawned(PlayerSpawnEvent ev) { }

        [PluginEvent(ServerEventType.PlayerChangeItem)]
        public virtual void OnChangedItem(PlayerChangeItemEvent ev) { }

        [PluginEvent(ServerEventType.PlayerInteractElevator)]
        public virtual void OnInteractedElevator(PlayerInteractElevatorEvent ev) { }

        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        public virtual void OnInteractedLocker(PlayerInteractLockerEvent ev) { }

        [PluginEvent(ServerEventType.PlayerReceiveEffect)]
        public virtual void OnReceivedEffect(PlayerReceiveEffectEvent ev) { }

        [PluginEvent(ServerEventType.PlayerPreCoinFlip)]
        public virtual void OnFlippingCoin(PlayerPreCoinFlipEvent ev) { }

        [PluginEvent(ServerEventType.PlayerCoinFlip)]
        public virtual void OnFlippedCoin(PlayerCoinFlipEvent ev) { }

        [PluginEvent(ServerEventType.PlayerToggleFlashlight)]
        public virtual void OnToggledFlashlight(PlayerToggleFlashlightEvent ev) { }

        [PluginEvent(ServerEventType.PlayerUnloadWeapon)]
        public virtual void OnUnloadedWeapon(PlayerUnloadWeaponEvent ev) { }

        [PluginEvent(ServerEventType.PlayerAimWeapon)]
        public virtual void OnAimedWapon(PlayerAimWeaponEvent ev) { }

        [PluginEvent(ServerEventType.PlayerMakeNoise)]
        public virtual void OnMadeNoise(PlayerMakeNoiseEvent ev) { }

        [PluginEvent(ServerEventType.PlayerUsingRadio)]
        public virtual void OnUsingRadio(PlayerUsingRadioEvent ev) { }

        [PluginEvent(ServerEventType.PlayerUseHotkey)]
        public virtual void OnUsingHotKey(PlayerUseHotkeyEvent ev) { }

        [PluginEvent(ServerEventType.PlayerRadioToggle)]
        public virtual void OnToggledRadio(PlayerRadioToggleEvent ev) { }

        [PluginEvent(ServerEventType.PlayerDamagedWindow)]
        public virtual void OnPlayerDamageWindow(PlayerDamagedWindowEvent ev) { }

        [PluginEvent(ServerEventType.PlayerUnlockGenerator)]
        public virtual void OnUnlockedGenerator(PlayerUnlockGeneratorEvent ev) { }

        [PluginEvent(ServerEventType.PlayerOpenGenerator)]
        public virtual void OnOpenedGenerator(PlayerOpenGeneratorEvent ev) { }

        [PluginEvent(ServerEventType.PlayerCloseGenerator)]
        public virtual void OnClosedGenerator(PlayerCloseGeneratorEvent ev) { }

        [PluginEvent(ServerEventType.GeneratorActivated)]
        public virtual void OnActivatedGenerator(GeneratorActivatedEvent ev) { }

        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        public virtual void OnInteractedDoor(PlayerInteractDoorEvent ev) { }

        [PluginEvent(ServerEventType.PlayerDroppedAmmo)]
        public virtual void OnDroppedAmmo(PlayerDroppedAmmoEvent ev) { }

        [PluginEvent(ServerEventType.PlayerMuted)]
        public virtual void OnMuted(PlayerMutedEvent ev) { }
    }
}