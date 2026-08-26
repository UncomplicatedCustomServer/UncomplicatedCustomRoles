using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UncomplicatedCustomRoles.API.Attributes;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Manager;

internal static class PluginImportManager
{
    private static Dictionary<Assembly, string> List { get; } = new();

    public static void Load(string file)
    {
        try
        {
            var assembly = Assembly.Load(File.ReadAllBytes(file));

            List.Add(assembly, file);

            ImportCustomRoles(assembly);
            ImportCustomModules(assembly);
        }
        catch (Exception e)
        {
            LogManager.Error(e.ToString());
        }
    }

    public static void UnloadAll()
    {
        List.Clear();
    }

    private static void ImportCustomRoles(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
            try
            {
                var attribs = type.GetCustomAttributes(typeof(PluginCustomRole), false);
                if (attribs != null && attribs.Length > 0 && typeof(ICustomRole).IsAssignableFrom(type) &&
                    !type.IsAbstract && !type.IsInterface)
                {
                    var Role = Activator.CreateInstance(type) as ICustomRole;

                    CustomRole.Register(Role);
                    LogManager.Info($"CustomRole {Role} imported from external UCR Plugin {List[assembly]}");
                }
            }
            catch (Exception e)
            {
                LogManager.Error(
                    $"Error while registering CustomRole from class by Attribute:\nType: {type.FullName} [{List[assembly]}]\nException: {e}");
            }
    }

    private static void ImportCustomModules(Assembly assembly)
    {
        if (!ImportManager.AvailableAssemblies.Contains(assembly))
            ImportManager.AvailableAssemblies.Add(assembly);
    }
}