using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using MapEditorReborn.Commands.ModifyingCommands.Scale;
using UncomplicatedCustomRoles.Elements;
using UnityEngine;
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
                Dictionary<string, List<ExternalCustomRole>> Deserialized = Deserializer.Deserialize<Dictionary<string, List<ExternalCustomRole>>>(File.ReadAllText(FileName));
                foreach (ExternalCustomRole Role in Deserialized["custom_roles"])
                {
                    SpawnManager.RegisterCustomSubclass(SpawnManager.RenderExportMethodToInternal(Role));
                    Log.Debug($"Registered external role {Role.Id} [{Role.Name}] from file:\n{FileName}");
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
                        new ExternalCustomRole()
                    }
                  }
                }));
                Log.Debug("Plugin does not have a role folder, generated one in configDir/UncomplicatedCustomRoles/");
            }
        }
    }
}
