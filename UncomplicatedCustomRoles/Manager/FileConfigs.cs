using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;
using UncomplicatedCustomRoles.Elements;
using UnityEngine.Playables;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UncomplicatedCustomRoles.Manager
{
    internal class FileConfigs
    {
        internal string Dir = Path.Combine(Paths.Configs, "UncomplicatedCustomRoles");

        public bool Is()
        {
            return Directory.Exists(Dir);
        }

        public string[] List()
        {
            return Directory.GetFiles(Dir);
        }

        public void LoadAll()
        {
            IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            foreach (string FileName in List())
            {
                if (Directory.Exists(FileName))
                {
                    continue;
                }
                Dictionary<string, List<ExternalCustomRole>> Deserialized = Deserializer.Deserialize<Dictionary<string, List<ExternalCustomRole>>>(File.ReadAllText(FileName));
                foreach (ExternalCustomRole Role in Deserialized["custom_roles"])
                {
                    Log.Debug($"Proposed to the registerer the external role {Role.Id} [{Role.Name}] from file:\n{FileName}");
                    SpawnManager.RegisterCustomSubclass(SpawnManager.RenderExportMethodToInternal(Role));
                }
            }
        }

        public void Welcome()
        {
            if (!Is())
            {
                ISerializer Serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                Directory.CreateDirectory(Dir);
                File.WriteAllText(Path.Combine(Dir, "example-role.yml"), Serializer.Serialize(new Dictionary<string, List<ExternalCustomRole>>() {
                  {
                    "custom_roles", new List<ExternalCustomRole>()
                    {
                        new()
                    }
                  }
                }));
                Log.Debug("Plugin does not have a role folder, generated one in configDir/UncomplicatedCustomRoles/");
            }
        }
    }
}
