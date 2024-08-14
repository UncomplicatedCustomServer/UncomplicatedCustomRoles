using PluginAPI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.Manager
{
    internal class FileConfigs
    {
        internal string Dir = Path.Combine(Paths.Plugins, "UncomplicatedCustomRoles");

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

                    string Content = File.ReadAllText(FileName);

                    if (Content.Contains("custom_roles:") && Content.Contains("- id:"))
                        LoadLegacyRoles(FileName, Content, action);
                    else
                        LoadRoles(FileName, Content, action);
                }
                catch (Exception ex)
                {
                    if (!Plugin.Instance.Config.Debug)
                    {
                        LogManager.Error($"Failed to parse {FileName} - {ex.GetType().FullName}. YAML Exception: {ex.Message}.");
                    }
                    else
                    {
                        LogManager.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.\nStack trace: {ex.StackTrace}");
                    }
                }
            }
        }

        private void LoadLegacyRoles(string fileName, string content, Action<CustomRole> action)
        {
            Dictionary<string, List<CustomRole>> Roles = YamlHelper.Deserializer.Deserialize<Dictionary<string, List<CustomRole>>>(content);

            if (!Roles.ContainsKey("custom_roles"))
            {
                LogManager.Error($"Error during the deserialization of file {fileName}: Node name 'custom_roles' not found!");
                return;
            }
            foreach (CustomRole Role in Roles["custom_roles"])
            {
                LogManager.Debug($"Proposed to the registerer the external LEGACY role {Role.Id} [{Role.Name}] from file:\n{fileName}");
                action(Role);
            }

            // Convert the role to a decent thing
            if (Roles["custom_roles"].Count == 1)
                File.WriteAllText(fileName,YamlHelper.Serializer.Serialize(Roles["custom_roles"][0]));
            else
                for (int i = 0; i < Roles["custom_roles"].Count; i++)
                    File.WriteAllText(fileName.Replace(".yml", $"-{i}.yml"), YamlHelper.Serializer.Serialize(Roles["custom_roles"][i]));
        }

        private void LoadRoles(string fileName, string content, Action<CustomRole> action)
        {
            CustomRole Role = YamlHelper.Deserializer.Deserialize<CustomRole>(content);
            LogManager.Debug($"Proposed to the registerer the external role {Role.Id} [{Role.Name}] from file:\n{fileName}");
            action(Role);
        }

        public void Welcome(string localDir = "")
        {
            if (!Is(localDir))
            {
                Directory.CreateDirectory(Path.Combine(Dir, localDir));
                File.WriteAllText(Path.Combine(Dir, localDir, "example-role.yml"), YamlHelper.Serializer.Serialize(new CustomRole()
                {
                    Id = CustomRole.GetFirstFreeID(1)
                }));

                LogManager.Info($"Plugin does not have a role folder, generated one in {Path.Combine(Dir, localDir)}");
            }
        }
    }
}
