using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LabApi.Loader;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations
{
    public static class DynamicInvoke
    {
        private static readonly Dictionary<string, MethodInfo> _methods = new();

        private static readonly Dictionary<string, Type> _types = new();

        private static readonly Dictionary<string, Assembly> _assemblies = new();

        public static MethodInfo GetMethod(string plugin, string address)
        {
            if (_methods.TryGetValue(address, out MethodInfo method))
                return method;

            if (!_assemblies.TryGetValue(plugin, out Assembly assembly))
            {
                assembly = PluginLoader.Plugins.FirstOrDefault(plugins => string.Equals(plugins.Key.Name, plugin, StringComparison.OrdinalIgnoreCase)).Value;
                _assemblies.Add(plugin, assembly);
            }

            if (assembly is null)
                return null; // Soft dependency not found - chill

            string argument = address.Split('.')?.Last();
            string stringType = address.Replace($".{argument}", string.Empty);

            if (!_types.TryGetValue(stringType, out Type type))
            {
                type = assembly.GetType(stringType);
                _types.Add(stringType, type);
            }

            if (type is null)
            {
                LogManager.Warn($"[DynamicInvoke] Failed to locate type {stringType} in assembly {assembly.FullName}!");
                return null;
            }

            if (argument.Contains('_')) // Handle <property>_get and <property>_set cases - Element IS a property
            {
                string stringProperty = argument.Split('_')[0]; // Cannot be null
                PropertyInfo property = type.GetProperty(stringProperty);
                MethodInfo resultMethod;

                if (property is null)
                {
                    LogManager.Warn($"[DynamicInvoke] Failed to locate property {stringProperty} in type {stringType} in assembly {assembly.FullName}!");
                    return null;
                }

                if (argument.EndsWith("_get")) // Handle getter
                    resultMethod = property.GetGetMethod();
                else
                    resultMethod = property.GetSetMethod();

                if (resultMethod is null)
                {
                    LogManager.Warn($"[DynamicInvoke] Failed to locate method _get() or _set() in property {stringProperty} in type {stringType} in assembly {assembly.FullName}!");
                    return null;
                }

                _methods.Add(address, resultMethod);
                return resultMethod;
            } 
            else // Normal method
            {
                MethodInfo resultMethod = type.GetMethods().FirstOrDefault(m => m.Name == argument);

                if (resultMethod is null)
                {
                    LogManager.Warn($"[DynamicInvoke] Failed to locate method {argument} in type {stringType} in assembly {assembly.FullName}!");
                    return null;
                }

                _methods.Add(address, resultMethod);
                return resultMethod;
            }
        }
    }
}