/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Loader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;

// FULL REWORK NEEDED (SEE PATCH)

namespace UncomplicatedCustomRoles.API.Features
{
#pragma warning disable IDE0059

    public class CustomRoleEventHandler
    {
        /// <summary>
        /// Gets the <see cref="Assembly"/> of the plugin Exiled.Loader
        /// </summary>
        public static Assembly EventHandlerAssembly => PluginLoader.Plugins.FirstOrDefault(p => p.Key.Name is "Exiled.Events").Value;

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

        internal static void InvokeAll(object ev, ref object evArgs)
        {

        }
    }
}
