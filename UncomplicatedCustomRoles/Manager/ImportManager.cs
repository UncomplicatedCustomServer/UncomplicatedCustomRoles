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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.API.Attributes;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Manager
{
    internal class ImportManager
    {
        public static readonly List<IPlugin<IConfig>> ActivePlugins = new();

        public static readonly List<Assembly> AvailableAssemblies = new()
        {
            Plugin.Instance.Assembly
        };

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
                            LogManager.Silent("Importing it!");
                            ActivePlugins.TryAdd(plugin);

                            ICustomRole Role = Activator.CreateInstance(type) as ICustomRole;

                            if (Plugin.Instance.Config.EnableBasicLogs)
                                LogManager.Info($"Imported CustomRole {Role.Name} ({Role.Id}) through Attribute from plugin {plugin.Name} (v{plugin.Version})");

                            // The public function is not CustomRole.Register BUT CompatibilityManager.RegisterCustomRole - it's hidden btw
                            //CustomRole.Register(Role);
                            CompatibilityManager.RegisterCustomRole(Role);
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"Error while registering CustomRole from class by Attribute: {e.GetType().FullName} - {e.Message}\nType: {type.FullName} [{plugin.Name}] - Source: {e.Source}");
                    }
            }
        }

        internal static Assembly GetCallingAssembly()
        {
            var stackTrace = new StackTrace();

            // Frame 0 = GetCallingAssembly
            // Frame 1 = Foo
            // Frame 2 = Main (o chi ha chiamato Foo)
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var method = stackTrace.GetFrame(i).GetMethod();
                var declaringType = method.DeclaringType;

                if (declaringType != null)
                {
                    return declaringType.Assembly;
                }
            }

            return null;
        }

        public static MethodBase GetCallingMethod()
        {
            StackTrace stackTrace = new();
            return stackTrace.GetFrame(1).GetMethod();
        }
    }
}
