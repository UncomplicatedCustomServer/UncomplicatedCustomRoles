using Exiled.Events.EventArgs.Interfaces;
using Exiled.Loader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.Events.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.API.Features
{
#pragma warning disable IDE0059

    public class CustomRoleEventHandler
    {
        /// <summary>
        /// Gets the <see cref="Assembly"/> of the plugin Exiled.Loader
        /// </summary>
        public static Assembly EventHandlerAssembly => Loader.Plugins.Where(plugin => plugin.Name is "Exiled.Events").FirstOrDefault()?.Assembly;

        /// <summary>
        /// Gets the <see cref="Type"/> of the class Exiled.Events.Handlers.Players
        /// </summary>
        public static Type PlayerHandler => EventHandlerAssembly?.GetTypes().Where(x => x.FullName == "Exiled.Events.Handlers.Player").FirstOrDefault();

        // A big thanks to Zer0Two -> https://discord.com/channels/656673194693885975/656673194693885981/1275184277146828932 && https://github.com/UnifiedSL/UnifiedEconomy/blob/master/UnifiedEconomy/Helpers/Events/EventHandlerUtils.cs
        private static readonly List<Tuple<EventInfo, Delegate>> DynamicHandlers = new();

        /// <summary>
        /// Gets the <see cref="SummonedCustomRole"/> instance related to this <see cref="CustomRoleEventHandler"/>
        /// </summary>
        [JsonIgnore] // I love every plugin dev
        [YamlDotNet.Serialization.YamlIgnore] // They don't deserve that :/
        public SummonedCustomRole SummonedInstance { get; }

        public ICustomRole Role => SummonedInstance.Role;

        public Dictionary<Type, MethodInfo> Listeners { get; } = new();

        internal CustomRoleEventHandler(SummonedCustomRole summonedInstance)
        {
            SummonedInstance = summonedInstance;
            LoadListeners();
        }

        private void LoadListeners()
        {
            if (Role is ICustomRoleEvents customRoleEventsRole)
            {
                Type baseType = typeof(EventCustomRole);
                Type declaredType = (customRoleEventsRole as EventCustomRole).GetType();

                foreach (MethodInfo method in baseType.GetMethods())
                {
                    var derivedMethod = declaredType.GetMethod(method.Name);
                    bool isOverride = derivedMethod != null && derivedMethod.DeclaringType != baseType;
                    
                    if (isOverride)
                        Listeners.Add(derivedMethod.GetParameters()[0].ParameterType, derivedMethod);
                }
            }
        }

        /// <summary>
        /// Invoke a <see cref="IExiledEvent"/> for the current instance
        /// </summary>
        /// <param name="eventArgs"></param>
        public void InvokeSafely(IExiledEvent eventArgs)
        {
            if (eventArgs is IPlayerEvent playerEventArgs && playerEventArgs.Player.Id == SummonedInstance.Player.Id && Role is ICustomRoleEvents && Listeners.ContainsKey(eventArgs.GetType()))
            {
                MethodInfo method = Listeners[eventArgs.GetType()];
                object[] args = new[] { eventArgs };
                method.Invoke(null, args);
                eventArgs = args[0] as IPlayerEvent;
            }
        }

        /// <summary>
        /// Invoke a <see cref="IExiledEvent"/> for all the instances
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventArgs"></param>
        public static void InvokeAllSafely<T>(T eventArgs) where T : IExiledEvent
        {
            foreach (SummonedCustomRole summonedCustomRole in SummonedCustomRole.List)
                summonedCustomRole.EventHandler.InvokeSafely(eventArgs);
        }

        /// <summary>
        /// Register every <see cref="IExiledEvent"/>
        /// </summary>
        // A big thanks to Zer0Two -> https://discord.com/channels/656673194693885975/656673194693885981/1275184277146828932 && https://github.com/UnifiedSL/UnifiedEconomy/blob/master/UnifiedEconomy/Helpers/Events/EventHandlerUtils.cs
        public static void RegisterEvents()
        {
            if (PlayerHandler is not null)
            {
                foreach (PropertyInfo property in PlayerHandler.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.PropertyType != typeof(Event)))
                {
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Event<>))
                    {
                        Type eventArgsType = property.PropertyType.GetGenericArguments().FirstOrDefault();

                        if (eventArgsType != null && typeof(IPlayerEvent).IsAssignableFrom(eventArgsType))
                        {
                            EventInfo eventInfo = property.PropertyType.GetEvent("InnerEvent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                            Delegate handler = typeof(CustomRoleEventHandler).GetMethod(nameof(CustomRoleEventHandler.InvokeAllSafely)).MakeGenericMethod(eventInfo.EventHandlerType.GenericTypeArguments).CreateDelegate(typeof(CustomEventHandler<>).MakeGenericType(eventInfo.EventHandlerType.GenericTypeArguments));

                            MethodInfo addMethod = eventInfo.GetAddMethod(true);
                            addMethod.Invoke(property.GetValue(null), new[] { handler });

                            DynamicHandlers.Add(new(eventInfo, handler));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unregister every <see cref="IExiledEvent"/>
        /// </summary>
        // A big thanks to Zer0Two -> https://discord.com/channels/656673194693885975/656673194693885981/1275184277146828932 && https://github.com/UnifiedSL/UnifiedEconomy/blob/master/UnifiedEconomy/Helpers/Events/EventHandlerUtils.cs
        public static void UnregisterEvents()
        {
            for (int i = 0; i < DynamicHandlers.Count; i++)
            {
                Tuple<EventInfo, Delegate> tuple = DynamicHandlers[i];
                EventInfo eventInfo = tuple.Item1;
                Delegate handler = tuple.Item2;

                if (eventInfo.DeclaringType != null)
                {
                    MethodInfo removeMethod = eventInfo.DeclaringType.GetMethod($"remove_{eventInfo.Name}", BindingFlags.Instance | BindingFlags.NonPublic);
                    removeMethod.Invoke(null, new object[] { handler });
                }
                else
                {
                    MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
                    removeMethod.Invoke(null, new[] { handler });
                }

                DynamicHandlers.Remove(tuple);
            }
        }

        /*public static void RegisterEvents()
        {
            Player.Kicking += InvokeAllSafely;
            Player.Kicked += InvokeAllSafely;
            Player.Banning += InvokeAllSafely;
            Player.Banned += InvokeAllSafely;
            Player.ChangingDangerState += InvokeAllSafely;
            Player.EarningAchievement += InvokeAllSafely;
            Player.UsingItem += InvokeAllSafely;
            Player.UsingItemCompleted += InvokeAllSafely;
            Player.UsedItem += InvokeAllSafely;
            Player.CancelledItemUse += InvokeAllSafely;
            Player.CancellingItemUse += InvokeAllSafely;
            Player.Interacted += InvokeAllSafely;
            Player.SpawningRagdoll += InvokeAllSafely;
            Player.SpawnedRagdoll += InvokeAllSafely;
            Player.ActivatingWarheadPanel += InvokeAllSafely;
            Player.ActivatingWorkstation += InvokeAllSafely;
            Player.DeactivatingWorkstation += InvokeAllSafely;
            Player.Left += InvokeAllSafely;
            Player.Died += InvokeAllSafely;
            Player.ChangingRole += InvokeAllSafely;
            Player.ThrownProjectile += InvokeAllSafely;
            Player.ThrowingRequest += InvokeAllSafely;
            Player.DroppingItem += InvokeAllSafely;
            Player.DroppedItem += InvokeAllSafely;
            Player.DroppingNothing += InvokeAllSafely;
            Player.PickingUpItem += InvokeAllSafely;
            Player.Handcuffing += InvokeAllSafely;
            Player.RemovingHandcuffs += InvokeAllSafely;
            Player.Escaping += InvokeAllSafely;
            Player.IntercomSpeaking += InvokeAllSafely;
            Player.Shot += InvokeAllSafely;
            Player.Shooting += InvokeAllSafely;
            Player.EnteringPocketDimension += InvokeAllSafely;
            Player.EscapingPocketDimension += InvokeAllSafely;
            Player.FailingEscapePocketDimension += InvokeAllSafely;
            Player.EnteringKillerCollision += InvokeAllSafely;
            Player.ReloadingWeapon += InvokeAllSafely;
            Player.Spawning += InvokeAllSafely;
            Player.Spawned += InvokeAllSafely;
            Player.ChangedItem += InvokeAllSafely;
            Player.ChangingItem += InvokeAllSafely;
            Player.ChangingGroup += InvokeAllSafely;
            Player.InteractingElevator += InvokeAllSafely;
            Player.InteractingLocker += InvokeAllSafely;
            Player.TriggeringTesla += InvokeAllSafely;
            Player.ReceivingEffect += InvokeAllSafely;
            Player.UsingRadioBattery += InvokeAllSafely;
            Player.ChangingMicroHIDState += InvokeAllSafely;
            Player.UsingMicroHIDEnergy += InvokeAllSafely;
            Player.InteractingShootingTarget += InvokeAllSafely;
            Player.DamagingShootingTarget += InvokeAllSafely;
            Player.FlippingCoin += InvokeAllSafely;
            Player.TogglingFlashlight += InvokeAllSafely;
            Player.UnloadingWeapon += InvokeAllSafely;
            Player.AimingDownSight += InvokeAllSafely;
            Player.TogglingWeaponFlashlight += InvokeAllSafely;
            Player.DryfiringWeapon += InvokeAllSafely;
            Player.VoiceChatting += InvokeAllSafely;
            Player.MakingNoise += InvokeAllSafely;
            Player.Jumping += InvokeAllSafely;
            Player.Landing += InvokeAllSafely;
            Player.Transmitting += InvokeAllSafely;
            Player.ChangingMoveState += InvokeAllSafely;
            Player.ChangingSpectatedPlayer += InvokeAllSafely;
            Player.TogglingNoClip += InvokeAllSafely;
            Player.TogglingOverwatch += InvokeAllSafely;
            Player.TogglingRadio += InvokeAllSafely;
            Player.SearchingPickup += InvokeAllSafely;
            Player.SendingAdminChatMessage += InvokeAllSafely;
            Player.KillingPlayer += InvokeAllSafely;
            Player.ItemAdded += InvokeAllSafely;
            Player.ItemRemoved += InvokeAllSafely;
            Player.EnteringEnvironmentalHazard += InvokeAllSafely;
            Player.StayingOnEnvironmentalHazard += InvokeAllSafely;
            Player.ExitingEnvironmentalHazard += InvokeAllSafely;
            Player.PlayerDamageWindow += InvokeAllSafely;
            Player.UnlockingGenerator += InvokeAllSafely;
            Player.OpeningGenerator += InvokeAllSafely;
            Player.ClosingGenerator += InvokeAllSafely;
            Player.ActivatingGenerator += InvokeAllSafely;
            Player.StoppingGenerator += InvokeAllSafely;
            Player.InteractingDoor += InvokeAllSafely;
            Player.DroppingAmmo += InvokeAllSafely;
            Player.DroppedAmmo += InvokeAllSafely;
            Player.IssuingMute += InvokeAllSafely;
            Player.RevokingMute += InvokeAllSafely;
            Player.ChangingRadioPreset += InvokeAllSafely;
            Player.Hurting += InvokeAllSafely;
            Player.Hurt += InvokeAllSafely;
            Player.Healing += InvokeAllSafely;
            Player.Healed += InvokeAllSafely;
            Player.Dying += InvokeAllSafely;
            Player.ChangingNickname += InvokeAllSafely;
        }*/

        /*public static void UnregisterEvents()
        {
            Player.Kicking -= InvokeAllSafely;
            Player.Kicked -= InvokeAllSafely;
            Player.Banning -= InvokeAllSafely;
            Player.Banned -= InvokeAllSafely;
            Player.ChangingDangerState -= InvokeAllSafely;
            Player.EarningAchievement -= InvokeAllSafely;
            Player.UsingItem -= InvokeAllSafely;
            Player.UsingItemCompleted -= InvokeAllSafely;
            Player.UsedItem -= InvokeAllSafely;
            Player.CancelledItemUse -= InvokeAllSafely;
            Player.CancellingItemUse -= InvokeAllSafely;
            Player.Interacted -= InvokeAllSafely;
            Player.SpawningRagdoll -= InvokeAllSafely;
            Player.SpawnedRagdoll -= InvokeAllSafely;
            Player.ActivatingWarheadPanel -= InvokeAllSafely;
            Player.ActivatingWorkstation -= InvokeAllSafely;
            Player.DeactivatingWorkstation -= InvokeAllSafely;
            Player.Left -= InvokeAllSafely;
            Player.Died -= InvokeAllSafely;
            Player.ChangingRole -= InvokeAllSafely;
            Player.ThrownProjectile -= InvokeAllSafely;
            Player.ThrowingRequest -= InvokeAllSafely;
            Player.DroppingItem -= InvokeAllSafely;
            Player.DroppedItem -= InvokeAllSafely;
            Player.DroppingNothing -= InvokeAllSafely;
            Player.PickingUpItem -= InvokeAllSafely;
            Player.Handcuffing -= InvokeAllSafely;
            Player.RemovingHandcuffs -= InvokeAllSafely;
            Player.Escaping -= InvokeAllSafely;
            Player.IntercomSpeaking -= InvokeAllSafely;
            Player.Shot -= InvokeAllSafely;
            Player.Shooting -= InvokeAllSafely;
            Player.EnteringPocketDimension -= InvokeAllSafely;
            Player.EscapingPocketDimension -= InvokeAllSafely;
            Player.FailingEscapePocketDimension -= InvokeAllSafely;
            Player.EnteringKillerCollision -= InvokeAllSafely;
            Player.ReloadingWeapon -= InvokeAllSafely;
            Player.Spawning -= InvokeAllSafely;
            Player.Spawned -= InvokeAllSafely;
            Player.ChangedItem -= InvokeAllSafely;
            Player.ChangingItem -= InvokeAllSafely;
            Player.ChangingGroup -= InvokeAllSafely;
            Player.InteractingElevator -= InvokeAllSafely;
            Player.InteractingLocker -= InvokeAllSafely;
            Player.TriggeringTesla -= InvokeAllSafely;
            Player.ReceivingEffect -= InvokeAllSafely;
            Player.UsingRadioBattery -= InvokeAllSafely;
            Player.ChangingMicroHIDState -= InvokeAllSafely;
            Player.UsingMicroHIDEnergy -= InvokeAllSafely;
            Player.InteractingShootingTarget -= InvokeAllSafely;
            Player.DamagingShootingTarget -= InvokeAllSafely;
            Player.FlippingCoin -= InvokeAllSafely;
            Player.TogglingFlashlight -= InvokeAllSafely;
            Player.UnloadingWeapon -= InvokeAllSafely;
            Player.AimingDownSight -= InvokeAllSafely;
            Player.TogglingWeaponFlashlight -= InvokeAllSafely;
            Player.DryfiringWeapon -= InvokeAllSafely;
            Player.VoiceChatting -= InvokeAllSafely;
            Player.MakingNoise -= InvokeAllSafely;
            Player.Jumping -= InvokeAllSafely;
            Player.Landing -= InvokeAllSafely;
            Player.Transmitting -= InvokeAllSafely;
            Player.ChangingMoveState -= InvokeAllSafely;
            Player.ChangingSpectatedPlayer -= InvokeAllSafely;
            Player.TogglingNoClip -= InvokeAllSafely;
            Player.TogglingOverwatch -= InvokeAllSafely;
            Player.TogglingRadio -= InvokeAllSafely;
            Player.SearchingPickup -= InvokeAllSafely;
            Player.SendingAdminChatMessage -= InvokeAllSafely;
            Player.KillingPlayer -= InvokeAllSafely;
            Player.ItemAdded -= InvokeAllSafely;
            Player.ItemRemoved -= InvokeAllSafely;
            Player.EnteringEnvironmentalHazard -= InvokeAllSafely;
            Player.StayingOnEnvironmentalHazard -= InvokeAllSafely;
            Player.ExitingEnvironmentalHazard -= InvokeAllSafely;
            Player.PlayerDamageWindow -= InvokeAllSafely;
            Player.UnlockingGenerator -= InvokeAllSafely;
            Player.OpeningGenerator -= InvokeAllSafely;
            Player.ClosingGenerator -= InvokeAllSafely;
            Player.ActivatingGenerator -= InvokeAllSafely;
            Player.StoppingGenerator -= InvokeAllSafely;
            Player.InteractingDoor -= InvokeAllSafely;
            Player.DroppingAmmo -= InvokeAllSafely;
            Player.DroppedAmmo -= InvokeAllSafely;
            Player.IssuingMute -= InvokeAllSafely;
            Player.RevokingMute -= InvokeAllSafely;
            Player.ChangingRadioPreset -= InvokeAllSafely;
            Player.Hurting -= InvokeAllSafely;
            Player.Hurt -= InvokeAllSafely;
            Player.Healing -= InvokeAllSafely;
            Player.Healed -= InvokeAllSafely;
            Player.Dying -= InvokeAllSafely;
            Player.ChangingNickname -= InvokeAllSafely;
        }*/
    }
}
