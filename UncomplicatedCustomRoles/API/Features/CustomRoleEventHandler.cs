using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;
using PluginAPI.Enums;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using UncomplicatedCustomRoles.Events.Args;

namespace UncomplicatedCustomRoles.API.Features
{
#pragma warning disable IDE0059
#pragma warning disable IDE1006

    public class CustomRoleEventHandler
    {
        /// <summary>
        /// Gets the <see cref="SummonedCustomRole"/> instance related to this <see cref="CustomRoleEventHandler"/>
        /// </summary>
        [JsonIgnore] // I love every plugin dev
        [YamlDotNet.Serialization.YamlIgnore] // They don't deserve that :/
        public SummonedCustomRole SummonedInstance { get; }

        public ICustomRole Role => SummonedInstance.Role;

        public Dictionary<Type, Tuple<object, MethodInfo>> Listeners { get; } = new();

        private bool _isPuppet { get; set; }

        internal CustomRoleEventHandler() => _isPuppet = true;

        internal CustomRoleEventHandler(SummonedCustomRole summonedInstance)
        {
            SummonedInstance = summonedInstance;
            _isPuppet = false;
            LoadListeners();
        }

        private void LoadListeners()
        {
            try
            {
                if (Role is EventCustomRole customRoleEventsRole)
                {
                    Type baseType = typeof(EventCustomRole);
                    Type declaredType = customRoleEventsRole.GetType();

                    foreach (MethodInfo method in baseType.GetMethods())
                    {
                        MethodInfo derivedMethod = declaredType.GetMethod(method.Name);
                        bool isOverride = derivedMethod != null && derivedMethod.DeclaringType != baseType;

                        if (isOverride && derivedMethod.GetParameters().Length > 0)
                            Listeners.Add(derivedMethod.GetParameters()[0].ParameterType, new(customRoleEventsRole, derivedMethod));
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error($"Failed to act CustomRoleEventHandler::LoadListeners() - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Invoke a <see cref="IExiledEvent"/> for the current instance
        /// </summary>
        /// <param name="eventArgs"></param>
        public void InvokeSafely(object eventArgs, Player player)
        {
            if (!_isPuppet && player.PlayerId == SummonedInstance.Player.PlayerId && Role is EventCustomRole && Listeners.ContainsKey(eventArgs.GetType()))
            {
                MethodInfo method = Listeners[eventArgs.GetType()].Item2;
                object[] args = new[] { eventArgs };
                method?.Invoke(Listeners[eventArgs.GetType()].Item1, args);
                eventArgs = args[0] as IEventArguments;
            }
        }

        private static bool TryGetPlayer(object eventArgs, out Player player)
        {
            player = (Player)eventArgs.GetType().GetProperty("Player")?.GetValue(eventArgs, null);
            return player != null;
        }

        /// <summary>
        /// Invoke a <see cref="IExiledEvent"/> for all the instances
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventArgs"></param>
        public static void InvokeAllSafely(object eventArgs)
        {
            if (TryGetPlayer(eventArgs, out Player player))
                foreach (SummonedCustomRole summonedCustomRole in SummonedCustomRole.List)
                    summonedCustomRole.EventHandler.InvokeSafely(eventArgs, player);
        }

        [PluginEvent(ServerEventType.PlayerKicked)]
        public void OnKicked(PlayerKickedEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerBanned)]
        public void OnBanned(PlayerBannedEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerUseItem)]
        public void OnUsingItem(PlayerUseItemEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public void OnUsedItem(PlayerUsedItemEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerCancelUsingItem)]
        public void OnCancelledItemUse(PlayerCancelUsingItemEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.RagdollSpawn)]
        public void OnSpawnedRagdoll(RagdollSpawnEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerLeft)]
        public void OnLeft(PlayerLeftEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void OnDied(PlayerDeathEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        public void OnChangingRole(PlayerChangeRoleEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerThrowProjectile)]
        public void OnThrowedProjectile(PlayerThrowProjectileEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerThrowItem)]
        public void OnThrowedItem(PlayerThrowItemEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerDropItem)]
        public void OnDroppedItem(PlayerDropItemEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerSearchPickup)]
        public void OnPickingUpItem(PlayerSearchPickupEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerSearchedPickup)]
        public void OnPickedUpItem(PlayerSearchedPickupEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerHandcuff)]
        public void OnHandcuffed(PlayerHandcuffEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
        public void OnRemovedHandcuffs(PlayerRemoveHandcuffsEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerEscape)]
        public void OnEscaped(PlayerEscapeEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerUsingIntercom)]
        public void OnIntercomSpeaking(PlayerUsingIntercomEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        public void OnShot(PlayerShotWeaponEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerEnterPocketDimension)]
        public void OnEnteredPocketDimension(PlayerEnterPocketDimensionEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerExitPocketDimension)]
        public void OnExitedPocketDimension(PlayerExitPocketDimensionEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerReloadWeapon)]
        public void OnReloadedWeapon(PlayerReloadWeaponEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerSpawn)]
        public void OnSpawned(PlayerSpawnEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerChangeItem)]
        public void OnChangedItem(PlayerChangeItemEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerInteractElevator)]
        public void OnInteractedElevator(PlayerInteractElevatorEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        public void OnInteractedLocker(PlayerInteractLockerEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerReceiveEffect)]
        public void OnReceivedEffect(PlayerReceiveEffectEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerPreCoinFlip)]
        public void OnFlippingCoin(PlayerPreCoinFlipEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerCoinFlip)]
        public void OnFlippedCoin(PlayerCoinFlipEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerToggleFlashlight)]
        public void OnToggledFlashlight(PlayerToggleFlashlightEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerUnloadWeapon)]
        public void OnUnloadedWeapon(PlayerUnloadWeaponEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerAimWeapon)]
        public void OnAimedWapon(PlayerAimWeaponEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerMakeNoise)]
        public void OnMadeNoise(PlayerMakeNoiseEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerUsingRadio)]
        public void OnUsingRadio(PlayerUsingRadioEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerUseHotkey)]
        public void OnUsingHotKey(PlayerUseHotkeyEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerRadioToggle)]
        public void OnToggledRadio(PlayerRadioToggleEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerDamagedWindow)]
        public void OnPlayerDamageWindow(PlayerDamagedWindowEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerUnlockGenerator)]
        public void OnUnlockedGenerator(PlayerUnlockGeneratorEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerOpenGenerator)]
        public void OnOpenedGenerator(PlayerOpenGeneratorEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerCloseGenerator)]
        public void OnClosedGenerator(PlayerCloseGeneratorEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.GeneratorActivated)]
        public void OnActivatedGenerator(GeneratorActivatedEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        public void OnInteractedDoor(PlayerInteractDoorEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerDroppedAmmo)]
        public void OnDroppedAmmo(PlayerDroppedAmmoEvent ev) => InvokeAllSafely(ev);

        [PluginEvent(ServerEventType.PlayerMuted)]
        public void OnMuted(PlayerMutedEvent ev) => InvokeAllSafely(ev);
    }
}