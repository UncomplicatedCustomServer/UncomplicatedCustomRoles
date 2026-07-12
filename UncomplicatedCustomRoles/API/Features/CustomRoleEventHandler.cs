/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Events.Arguments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features
{
    public class CustomRoleEventHandler
    {
        public SummonedCustomRole SummonedInstance { get; }

        public ICustomRole Role => SummonedInstance.Role;

        public List<Listener> Listeners { get; } = new();
        
        private static int _activeListeners;

        internal CustomRoleEventHandler(SummonedCustomRole summonedInstance)
        {
            SummonedInstance = summonedInstance;
            LoadListeners();
            _activeListeners += Listeners.Count;
        }
        
        internal void Unload()
        {
            _activeListeners -= Listeners.Count;
            if (_activeListeners < 0)
                _activeListeners = 0;
            Listeners.Clear();
        }

        private void LoadListeners()
        {
            try
            {
                if (Role is EventCustomRole customRoleEventsRole)
                {
                    Type baseType = typeof(EventCustomRole);
                    Type declaredType = (customRoleEventsRole as EventCustomRole).GetType();

                    foreach (MethodInfo method in declaredType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(m => m.GetBaseDefinition().DeclaringType == baseType && !m.IsSpecialName && m.Name is not "OnSpawned"))
                    {
                        MethodInfo derivedMethod = declaredType.GetMethod(method.Name);
                        bool isOverride = derivedMethod != null && derivedMethod.DeclaringType != baseType;

                        if (isOverride && derivedMethod.GetParameters().Length > 0)
                        {
                            Listeners.Add(new(derivedMethod.GetParameters()[0].ParameterType, derivedMethod, customRoleEventsRole));
                            LogManager.Debug($"Loaded listener for [Event]CustomRole {customRoleEventsRole}: EVENT={derivedMethod.GetParameters()[0].ParameterType}, METHOD={derivedMethod.Name}()");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error($"Failed to act CustomRoleEventHandler::LoadListeners() - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
            }
        }

        internal void InvokeSafely(IPlayerEvent playerEvent)
        {
            if (Listeners.Count == 0)
                return;

            if (playerEvent is ICancellableEvent { IsAllowed: false })
                return;

            Type eventType = playerEvent.GetType();
            foreach (Listener listener in Listeners)
                if (listener.Event == eventType)
                {
                    listener.Method.Invoke(listener.Instance, [playerEvent]);
                    return;
                }
        }

        internal static void InvokeAll(IPlayerEvent ev)
        {
            if (_activeListeners == 0)
                return;
            
            foreach (KeyValuePair<string, SummonedCustomRole> pair in SummonedCustomRole.List)
                pair.Value.EventHandler?.InvokeSafely(ev);
        }
    }

    public class Listener(Type @event, MethodInfo method, object instance)
    {
        public Type Event { get; } = @event;

        public MethodInfo Method { get; } = method;

        public object Instance { get; } = instance;
    }
}
