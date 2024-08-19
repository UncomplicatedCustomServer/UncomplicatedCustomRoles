using Exiled.Events.EventArgs.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomRoles.Interfaces;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomRoles.API.Features
{
#pragma warning disable IDE0059

    public class CustomRoleEventHandler
    {
        [JsonIgnore] // I love every plugin dev
        [YamlIgnore] // They don't deserve that :(
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
                Type baseType = typeof(CustomRoleEvents);
                Type declaredType = (customRoleEventsRole as CustomRoleEvents).GetType();

                foreach (MethodInfo method in baseType.GetMethods())
                {
                    var derivedMethod = declaredType.GetMethod(method.Name);
                    bool isOverride = derivedMethod != null && derivedMethod.DeclaringType != baseType;
                    
                    if (isOverride)
                        Listeners.Add(derivedMethod.GetParameters()[0].ParameterType, derivedMethod);
                }
            }
        }

        public void InvokeSafely(IPlayerEvent eventArgs)
        {
            if (eventArgs.Player.Id == SummonedInstance.Player.Id && Role is ICustomRoleEvents && Listeners.ContainsKey(eventArgs.GetType()))
            {
                MethodInfo method = Listeners[eventArgs.GetType()];
                object[] args = new[] { eventArgs };
                method.Invoke(null, args);
                eventArgs = args[0] as IPlayerEvent;
            }
        }

        public static void InvokeAllSafely(IPlayerEvent eventArgs)
        {
            foreach (SummonedCustomRole summonedCustomRole in SummonedCustomRole.List)
                summonedCustomRole.EventHandler.InvokeSafely(eventArgs);
        }

        public static void RegisterEvents()
        {

        }
    }
}
