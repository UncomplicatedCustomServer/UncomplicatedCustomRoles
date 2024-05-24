using Exiled.API.Features;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UncomplicatedCustomRoles.Elements;

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
            foreach (string FileName in List(localDir))
            {
                try
                {
                    if (Directory.Exists(FileName))
                        continue;

                    if (FileName.Split().First() == ".")
                        return;

                    Dictionary<string, List<CustomRole>> Roles = Loader.Deserializer.Deserialize<Dictionary<string, List<CustomRole>>>(File.ReadAllText(FileName));

                    if (!Roles.ContainsKey("custom_roles"))
                    {
                        Log.Error($"Error during the deserialization of file {FileName}: Node name 'custom_roles' not found!");
                        return;
                    }
                    foreach (CustomRole Role in Roles["custom_roles"])
                    {
                        Log.Debug($"Proposed to the registerer the external role {Role.Id} [{Role.Name}] from file:\n{FileName}");
                        SpawnManager.RegisterCustomSubclass(Role);
                    }
                }
                catch (Exception ex)
                {
                    if (!Plugin.Instance.Config.Debug)
                    {
                        Log.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.");
                    }
                    else
                    {
                        Log.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.\nStack trace: {ex.StackTrace}");
                    }
                }
            }
        }

        public void Welcome(string localDir = "")
        {
            if (!Is(localDir))
            {
                Directory.CreateDirectory(Path.Combine(Dir, localDir));

                File.WriteAllText(Path.Combine(Dir, localDir, "example-role.yml"), Loader.Serializer.Serialize(new Dictionary<string, List<CustomRole>>() {
                  {
                    "custom_roles", new List<CustomRole>()
                    {
                        new()
                    }
                  }
                }));

                Log.Info($"Plugin does not have a role folder, generated one in {Path.Combine(Dir, localDir)}");
            }
        }
    }
}
