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
            if (playerEvent is ICancellableEvent cancellableEvent && !cancellableEvent.IsAllowed)
                return;

            Listener listener = Listeners.FirstOrDefault(l => l.Event == playerEvent.GetType());

            listener?.Method.Invoke(listener.Instance, new object[] { playerEvent });
        }

        internal static void InvokeAll(IPlayerEvent ev)
        {
            foreach (SummonedCustomRole summonedCustomRole in SummonedCustomRole.List)
                summonedCustomRole.EventHandler?.InvokeSafely(ev);
        }
    }

    public class Listener
    {
        public Type Event { get; }

        public MethodInfo Method { get; }

        public object Instance { get; }

        public Listener(Type @event, MethodInfo method, object instance)
        {
            Event = @event;
            Method = method;
            Instance = instance;
        }
    }
}
