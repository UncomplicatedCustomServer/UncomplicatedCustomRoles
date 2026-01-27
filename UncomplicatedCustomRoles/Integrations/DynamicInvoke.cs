/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations
{
    public static class DynamicInvoke
    {
        private static readonly Dictionary<string, MethodInfo> _methods = new();

        private static readonly Dictionary<string, Type> _types = new();

        private static readonly Dictionary<string, Assembly> _assemblies = new();

        /// <summary>
        /// Get the <see cref="MethodInfo"/> of a method or property from a specified plugin.<br></br>
        /// '_get' and '_set' will load the getter and setter of a property respectively.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="address"></param>
        /// <param name="isLabapi"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(string plugin, string address, bool isLabapi = false, int methodCounter = -1, string[] requiredParamNames = null)
        {
            if (_methods.TryGetValue(address, out MethodInfo method))
                return method;

            if (!_assemblies.TryGetValue(plugin, out Assembly assembly))
            {
                assembly = isLabapi ? GetLabAPIAssembly(plugin) : GetExiledAssembly(plugin);
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
                IEnumerable<MethodInfo> resultMethods = type.GetMethods().Where(m => m.Name == argument);
                MethodInfo resultMethod;
                
                if (methodCounter != -1 || (requiredParamNames is not null && requiredParamNames.Length > 0))
                {
                    IEnumerable<MethodInfo> filtered = resultMethods;

                    if (methodCounter != -1)
                        filtered = filtered.Where(m => m.GetParameters().Length == methodCounter);

                    if (requiredParamNames is not null && requiredParamNames.Length > 0)
                    {
                        filtered = filtered.Where(m =>
                        {
                            var paramNames = m.GetParameters().Select(p => p.Name).ToArray();
                            return requiredParamNames.All(rpn => paramNames.Contains(rpn, StringComparer.OrdinalIgnoreCase));
                        });
                    }

                    resultMethod = filtered.FirstOrDefault();
                } else
                {
                    resultMethod = resultMethods.FirstOrDefault();
                }

                if (resultMethod is null)
                {
                    LogManager.Warn($"[DynamicInvoke] Failed to locate method {argument} in type {stringType} in assembly {assembly.FullName}!");
                    return null;
                }

                _methods.Add(address, resultMethod);
                return resultMethod;
            }
        }

        private static Assembly GetLabAPIAssembly(string pluginName)
        {
            try
            {
                KeyValuePair<LabApi.Loader.Features.Plugins.Plugin, Assembly>? plugin = LabApi.Loader.PluginLoader.Plugins.FirstOrDefault(p => p.Key.Name == pluginName);

                if (plugin is not null)
                    return plugin.Value.Value;

                return null;
            }
            catch (Exception e)
            {
                LogManager.Error(e.ToString());
                return null;
            }
        }
        
        private static Assembly GetExiledAssembly(string pluginName)
        {
            try
            {
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p => p.FullName.Contains(pluginName));
                return assembly;
            }
            catch (Exception e)
            {
                LogManager.Error(e.ToString());
                return null;
            }
        }
    }
}