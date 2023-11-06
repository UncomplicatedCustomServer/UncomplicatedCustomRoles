using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using UncomplicatedCustomRoles.Elements;
using UncomplicatedCustomRoles.Structures;
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
                List<CustomRole> Deserialized = Deserializer.Deserialize<List<CustomRole>>(File.ReadAllText(FileName));
                foreach (CustomRole Role in Deserialized)
                {
                    SpawnManager.RegisterCustomSubclass(Role);
                    Log.Debug($"Registered external role {Role.Id} from file:\n{FileName}");
                }
            }
        }
        public void Welcome()
        {
            if (!Is())
            {
                ISerializer Serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                Directory.CreateDirectory(Dir);
                File.WriteAllText(Path.Combine(Dir, "example-role.yml"), Serializer.Serialize(new List<CustomRole>()
                {
                    new CustomRole()
                    {
                        Id = 10
                    }
                }));
                Log.Debug("Plugin does not have a role folder, generated one in configDir/UncomplicatedCustomRoles/");
            }
        }
    }
}
