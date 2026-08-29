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
using LabApi.Loader;
using UncomplicatedCustomRoles.API.Attributes;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using LabApiPlugin = LabApi.Loader.Features.Plugins.Plugin;

namespace UncomplicatedCustomRoles.Manager;

internal class ImportManager
{
    public static readonly List<LabApiPlugin> ActivePlugins = [];

    public static readonly List<Assembly> AvailableAssemblies = [];

    private static bool _alreadyLoaded;

    public static void Init()
    {
        if (_alreadyLoaded)
            return;

        Actor();
    }

    public static void Reload()
    {
        _alreadyLoaded = false;
        Actor();
    }

    public static void Unload()
    {
        ActivePlugins.Clear();
        AvailableAssemblies.Clear();
        AvailableAssemblies.Add(Plugin.Assembly);
    }

    private static void Actor()
    {
        LogManager.Debug("Checking for CustomRole registered in other plugins to import...");

        _alreadyLoaded = true;

        if (!AvailableAssemblies.Contains(Plugin.Assembly))
            AvailableAssemblies.Add(Plugin.Assembly);

        foreach (KeyValuePair<LabApiPlugin, Assembly> plugin in PluginLoader.Plugins.Where(plugin => plugin.Key.Name != Plugin.Instance.Name))
        {
            if (!AvailableAssemblies.Contains(plugin.Value))
                AvailableAssemblies.Add(plugin.Value);
            LogManager.Silent($"[Import Manager] Passing plugin {plugin.Key.Name}");
            foreach (Type type in plugin.Value.GetTypes())
            {
                try
                {
                    object[] attribs = type.GetCustomAttributes(typeof(PluginCustomRole), false);
                    if (attribs != null && attribs.Length > 0 && typeof(ICustomRole).IsAssignableFrom(type) &&
                        !type.IsAbstract && !type.IsInterface)
                    {
                        ActivePlugins.TryAdd(plugin.Key);

                        ICustomRole Role = Activator.CreateInstance(type) as ICustomRole;

                        CustomRole.Register(Role);
                        LogManager.Info($"CustomRole {Role} imported from external plugin {plugin.Key.Name} (v{plugin.Key.Version.ToString(3)})");
                    }
                }
                catch (Exception e)
                {
                    LogManager.Error(
                        $"Error while registering CustomRole from class by Attribute:\nType: {type.FullName} [{plugin.Key.Name}]\nException: {e}");
                }
            }
        }

        YamlFlagsHandler.InvalidateCache();
    }
}