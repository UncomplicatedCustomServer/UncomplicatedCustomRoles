using Exiled.API.Interfaces;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.API.Attributes;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Manager
{
    internal class ImportManager
    {
        public static List<IPlugin<IConfig>> ActivePlugins => new();

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

            foreach (IPlugin<IConfig> plugin in Loader.Plugins.Where(plugin => plugin.Name != Plugin.Instance.Name))
                foreach (Type type in plugin.Assembly.GetTypes())
                    try
                    {
                        object[] attribs = type.GetCustomAttributes(typeof(PluginCustomRole), false);
                        if (attribs != null && attribs.Length > 0 && (type == typeof(ICustomRole) || type == typeof(CustomRole)))
                        {
                            ActivePlugins.TryAdd(plugin);

                            ICustomRole Role = Activator.CreateInstance(type) as ICustomRole;
                            CustomRole.Register(Role);
                            if (Plugin.Instance.Config.EnableBasicLogs)
                                LogManager.Info($"Imported CustomRole {Role.Name} ({Role.Id}) through Attribute from plugin {plugin.Name} (v{plugin.Version})");
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"Error while registering CustomRole from class by Attribute: {e.GetType().FullName} - {e.Message}\nType: {type.FullName} [{plugin.Name}] - Source: {e.Source}");
                    }
        }
    }
}
