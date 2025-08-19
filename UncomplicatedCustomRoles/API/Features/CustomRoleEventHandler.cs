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
            foreach (SummonedCustomRole summonedCustomRole in SummonedCustomRole.List.Values)
                summonedCustomRole.EventHandler.InvokeSafely(eventArgs);
        }
    }
}
