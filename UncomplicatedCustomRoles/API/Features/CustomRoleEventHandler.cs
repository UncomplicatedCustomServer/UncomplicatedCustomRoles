/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.Events.EventArgs.Interfaces;
using Exiled.Loader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.Events.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;

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

        public Dictionary<Type, Tuple<object, MethodInfo>> Listeners { get; } = new();

        internal CustomRoleEventHandler(SummonedCustomRole summonedInstance)
        {
            SummonedInstance = summonedInstance;
            LoadListeners();
        }

        private void LoadListeners()
        {
            try
            {
                if (Role is EventCustomRole customRoleEventsRole)
                {
                    Type baseType = typeof(EventCustomRole);
                    Type declaredType = (customRoleEventsRole as EventCustomRole).GetType();

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
        public void InvokeSafely(IExiledEvent eventArgs)
        {
            if (eventArgs is IPlayerEvent playerEventArgs && playerEventArgs.Player is not null && playerEventArgs.Player.Id == SummonedInstance.Player.Id && Role is EventCustomRole && Listeners.ContainsKey(eventArgs.GetType()))
            {
                MethodInfo method = Listeners[eventArgs.GetType()].Item2;
                object[] args = new[] { eventArgs };
                method?.Invoke(Listeners[eventArgs.GetType()].Item1, args);
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
    }
}
