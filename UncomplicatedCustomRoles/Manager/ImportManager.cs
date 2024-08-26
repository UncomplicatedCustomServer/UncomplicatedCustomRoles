using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.API.Attributes;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Manager
{
    internal class ImportManager
    {
        public static List<Assembly> ActivePlugins => new();

        public const float WaitingTime = 5f;

        public static void Init()
        {
            ActivePlugins.Clear();
            // Call a delayed task
            Task.Run(Actor);
        }

        private static async void Actor()
        {
            await Task.Delay((int)(WaitingTime * 10000));

            foreach (Assembly assembly in PluginAPI.Loader.AssemblyLoader.Plugins.Keys.Where(assembly => assembly != Assembly.GetExecutingAssembly()))
                foreach (Type type in assembly.GetTypes())
                    try
                    {
                        object[] attribs = type.GetCustomAttributes(typeof(PluginCustomRole), false);
                        if (attribs != null && attribs.Length > 0 && type == typeof(ICustomRole) || type == typeof(CustomRole))
                        {
                            ActivePlugins.TryAdd(assembly);

                            ICustomRole Role = Activator.CreateInstance(type) as ICustomRole;
                            CustomRole.Register(Role);
                            if (Plugin.Instance.Config.EnableBasicLogs)
                                LogManager.Info($"Imported CustomRole {Role.Name} ({Role.Id}) through Attribute from plugin {assembly.FullName} (v{assembly.ImageRuntimeVersion})");
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"Error while registering CustomRole from class by Attribute: {e.GetType().FullName} - {e.Message}\nType: {type.FullName} [{assembly.FullName}] - Source: {e.Source}");
                    }
        }
    }
}
