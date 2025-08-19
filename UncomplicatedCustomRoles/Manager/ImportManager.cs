/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Interfaces;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.API.Attributes;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Manager
{
    internal class ImportManager
    {
        public static readonly List<IPlugin<IConfig>> ActivePlugins = new();

        public static readonly List<Assembly> AvailableAssemblies = new();

        public const float WaitingTime = 5f;

        private static bool _alreadyLoaded = false;

        public static void Init()
        {
            if (_alreadyLoaded)
                return;

            ActivePlugins.Clear();
            // Call a delayed task
            Task.Run(Actor);
        }

        private static void Actor()
        {
            LogManager.Debug($"Checking for CustomRole registered in other plugins to import...");

            _alreadyLoaded = true;
            AvailableAssemblies.Add(Plugin.Instance.Assembly);

            foreach (IPlugin<IConfig> plugin in Loader.Plugins.Where(plugin => plugin.Name != Plugin.Instance.Name))
            {
                AvailableAssemblies.Add(plugin.Assembly);
                LogManager.Silent($"[Import Manager] Passing plugin {plugin.Name}");
                foreach (Type type in plugin.Assembly.GetTypes())
                    try
                    {
                        object[] attribs = type.GetCustomAttributes(typeof(PluginCustomRole), false);
                        if (attribs != null && attribs.Length > 0 && (type.IsSubclassOf(typeof(ICustomRole)) || type.IsSubclassOf(typeof(CustomRole)) || type.IsSubclassOf(typeof(EventCustomRole))))
                        {
                            ActivePlugins.TryAdd(plugin);

                            ICustomRole Role = Activator.CreateInstance(type) as ICustomRole;

                            CustomRole.Register(Role);
                            LogManager.Info($"CustomRole {Role} imported from external plugin {plugin.Name} (v{plugin.Version.ToString(3)})");
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"Error while registering CustomRole from class by Attribute:\nType: {type.FullName} [{plugin.Name}]\nException: {e}");
                    }
            }
        }
    }
}
