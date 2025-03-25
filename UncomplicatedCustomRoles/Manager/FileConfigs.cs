using Exiled.API.Features;
using Exiled.Loader;
using LiteNetLib4Mirror.Open.Nat;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Manager.Compatibility;

namespace UncomplicatedCustomRoles.Manager
{
    internal class FileConfigs
    {
        internal string Dir = Path.Combine(Paths.Configs, "UncomplicatedCustomRoles");

        public bool Is(string localDir = "") => Directory.Exists(Path.Combine(Dir, localDir));

        public string[] List(string localDir = "") => Directory.GetFiles(Path.Combine(Dir, localDir));

        public void LoadAll(string localDir = "", Action<CustomRole> action = null)
        {
            LoadAction(action ?? ((CustomRole Role) =>
            {
                CustomRole.Register(Role);
            }), localDir);
            
            foreach (string dir in Directory.GetDirectories(Path.Combine(Dir, localDir)))
            {
                string name = dir.Replace(Dir, string.Empty);
                if (name[0] is '/' or '\\')
                    name = name.Remove(0, 1);

                if (int.TryParse(name, out int num) && num < 990000)
                    continue;

                if (name is "")
                    continue;

                LoadAction((CustomRole Role) =>
                {
                    CustomRole.Register(Role);
                }, name);
            }
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

                    if (CustomTypeChecker(FileName, out CustomRole role, out string error))
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
                        LogManager.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.\n\nThis is a YAML error that YOU CAUSED and therefore >>YOU<< NEED TO FIX IT!\nDON'T COME TO US WITH THIS ERROR!");
                    else
                        LogManager.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.\nStack trace: {ex.StackTrace}\n\nThis is a YAML error that YOU CAUSED and therefore >>YOU<< NEED TO FIX IT!\nDON'T COME TO US WITH THIS ERROR!");
                }
            }
        }

        private bool CustomTypeChecker(string path, out CustomRole role, out string error)
        {
            string content = File.ReadAllText(path);
            Dictionary<string, object> data = Loader.Deserializer.Deserialize<Dictionary<string, object>>(content);
            role = default;
            error = null;

            SnakeCaseNamingStrategy namingStrategy = new();

            foreach (PropertyInfo property in typeof(CustomRole).GetProperties().Where(p => p.CanWrite && p is not null && p.GetType() is not null))
            {
                if (!data.ContainsKey(namingStrategy.GetPropertyName(property.Name, false)))
                    error = $"Given CustomRole doesn't contain the required property '{namingStrategy.GetPropertyName(property.Name, false)}' ({namingStrategy.GetPropertyName(property.PropertyType.Name, false)})\nAt '{path}'";

                if (error is not null)
                    break;
            }

            if (error is null)
            {
                try
                {
                    role = Loader.Deserializer.Deserialize<CustomRole>(content);
                    return true;
                } catch (Exception)
                {
                    try
                    {
                        role = Loader.Deserializer.Deserialize<OldCustomRole>(content).ToCustomRole();
                        File.WriteAllText(path, Loader.Serializer.Serialize(role));
                        LogManager.Warn($"Role {role.Name} ({role.Id}) was NOT updated! - Auto updated correctly, you shouldn't see this message again :D\nAt {path}");
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

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
            return "N/D";
        }
    }
}
