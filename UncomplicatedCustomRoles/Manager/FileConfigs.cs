using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using UncomplicatedCustomRoles.Elements;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UncomplicatedCustomRoles.Manager
{
    internal class FileConfigs
    {
        internal string Dir = Path.Combine(Paths.Configs, "UncomplicatedCustomRoles");

        public bool Is(string LocalDir = "")
        {
            return Directory.Exists(Path.Combine(Dir, LocalDir));
        }

        public string[] List(string LocalDir = "")
        {
            return Directory.GetFiles(Path.Combine(Dir, LocalDir));
        }

        public void LoadAll(string LocalDir = "")
        {
            IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            foreach (string FileName in List(LocalDir))
            {
                if (Directory.Exists(FileName))
                {
                    continue;
                }
                if (FileName.Split().First() == ".")
                {
                    return;
                }
                Dictionary<string, List<ExternalCustomRole>> Deserialized = Deserializer.Deserialize<Dictionary<string, List<ExternalCustomRole>>>(File.ReadAllText(FileName));
                foreach (ExternalCustomRole Role in Deserialized["custom_roles"])
                {
                    Log.Debug($"Proposed to the registerer the external role {Role.Id} [{Role.Name}] from file:\n{FileName}");
                    SpawnManager.RegisterCustomSubclass(SpawnManager.RenderExportMethodToInternal(Role));
                }
            }
        }

        public void Welcome(string LocalDir = "")
        {
            if (!Is(LocalDir))
            {
                ISerializer Serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                Directory.CreateDirectory(Path.Combine(Dir, LocalDir));
                File.WriteAllText(Path.Combine(Dir, LocalDir, "example-role.yml"), Serializer.Serialize(new Dictionary<string, List<ExternalCustomRole>>() {
                  {
                    "custom_roles", new List<ExternalCustomRole>()
                    {
                        new()
                    }
                  }
                }));
                Log.Info($"Plugin does not have a role folder, generated one in {Path.Combine(Dir, LocalDir)}");
            }
        }
    }
}
