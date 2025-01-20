using Exiled.API.Features;
using Exiled.Loader;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.Manager
{
    internal class FileConfigs
    {
        internal string Dir = Path.Combine(Paths.Configs, "UncomplicatedCustomRoles");

        public bool Is(string localDir = "")
        {
            return Directory.Exists(Path.Combine(Dir, localDir));
        }

        public string[] List(string localDir = "")
        {
            return Directory.GetFiles(Path.Combine(Dir, localDir));
        }

        public void LoadAll(string localDir = "")
        {
            LoadAction((CustomRole Role) =>
            {
                CustomRole.Register(Role);
            }, localDir);
        }

        public void LoadAction(Action<CustomRole> action, string localDir = "")
        {
            foreach (string FileName in List(localDir))
            {
                try
                {
                    if (Directory.Exists(FileName))
                        continue;

                    if (FileName.Split().First() == ".")
                        return;

                    if (CustomTypeChecker(File.ReadAllText(FileName), out CustomRole role, out string error))
                    {
                        LogManager.Debug($"Proposed to the registerer the external role {role.Id} [{role.Name}] from file:\n{FileName}");
                        action(role);
                    }
                    else
                        LogManager.Error($"Error during the deserialization of the CustomRole at {FileName}: {error}");
                }
                catch (Exception ex)
                {
                    // Add the role to the not-loaded list
                    CustomRole.NotLoadedRoles.Add(new(TryGetRoleId(File.ReadAllText(FileName)), FileName, ex.GetType().Name, ex.Message));

                    if (!Plugin.Instance.Config.Debug)
                        LogManager.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.");
                    else
                        LogManager.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.\nStack trace: {ex.StackTrace}");
                }
            }
        }

        private bool CustomTypeChecker(string content, out CustomRole role, out string error)
        {
            Dictionary<string, object> data = Loader.Deserializer.Deserialize<Dictionary<string, object>>(content);
            role = default;
            error = null;

            SnakeCaseNamingStrategy namingStrategy = new();

            foreach (PropertyInfo property in typeof(CustomRole).GetProperties().Where(p => p.CanWrite && p is not null && p.GetType() is not null))
                if (!data.ContainsKey(namingStrategy.GetPropertyName(property.Name, false)) && error is null)
                    error = $"Given CustomRole doesn't contain the required property '{namingStrategy.GetPropertyName(property.Name, false)}' ({namingStrategy.GetPropertyName(property.PropertyType.Name, false)})";

            if (error is null)
            {
                role = Loader.Deserializer.Deserialize<CustomRole>(content);
                return true;
            }

            return false;
        }

        public void Echo() { }

        public void Welcome(string localDir = "")
        {
            if (!Is(localDir))
            {
                Directory.CreateDirectory(Path.Combine(Dir, localDir));
                File.WriteAllText(Path.Combine(Dir, localDir, "example-role.yml"), Loader.Serializer.Serialize(new CustomRole()
                {
                    Id = CustomRole.GetFirstFreeID(1)
                }));

                LogManager.Info($"Plugin does not have a role folder, generated one in {Path.Combine(Dir, localDir)}");
            }
        }

        public static string TryGetRoleId(string content)
        {
            string[] pieces = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            if (pieces.Contains("id:"))
                return pieces.FirstOrDefault(l => l.Contains("id:")).Replace(" ", "").Replace("id:", "");
            return "ND";
        }
    }
}
